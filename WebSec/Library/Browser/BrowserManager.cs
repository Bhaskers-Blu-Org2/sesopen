//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using Common.Validation;
    using Fiddler;
    using Interfaces;
    using Logger;

    /// <summary>
    ///     Implements a browser pool manager.
    /// </summary>
    public class BrowserManager : IBrowserManager, IDisposable
    {
        /// <summary>
        ///     The default number of browser instances that populate the browser pool.
        /// </summary>
        public const int DefaultMaxBrowserInstances = 1;

        /// <summary>
        /// The browser pools.
        /// </summary>
        private readonly ReadOnlyDictionary<BrowserType, BrowserPool> browserPools;

        /// <summary>
        /// The factory.
        /// </summary>
        private readonly IBrowserFactory factory;

        /// <summary>
        /// The sync lock.
        /// </summary>
        private readonly SemaphoreSlim syncLock;

        /// <summary>
        /// The disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrowserManager" /> class.
        /// </summary>
        /// <param name="browserMaxInstances">A dictionary of the maximum instances per <see cref="BrowserType"/> to be maintained in the pool.</param>
        public BrowserManager(IDictionary<BrowserType, int> browserMaxInstances)
            : this(new DefaultBrowserFactory(), browserMaxInstances)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrowserManager" /> class.
        /// </summary>
        /// <param name="factory">The browser factory implementation to use.</param>
        /// <param name="browserMaxInstances">A dictionary of the maximum instances per <see cref="BrowserType"/> to be maintained in the pool.</param>
        internal BrowserManager(IBrowserFactory factory, IDictionary<BrowserType, int> browserMaxInstances)
        {
            Require.NotNull(() => factory);
            Require.NotNull(() => browserMaxInstances);

            var browserCount = browserMaxInstances.Count;
            Require.GreaterThanOrEqualTo(() => browserCount, 1);

            this.factory = factory;

            var tempDictionary = new Dictionary<BrowserType, BrowserPool>();
            foreach (var type in browserMaxInstances.Keys)
            {
                var maxInstances = browserMaxInstances[type];
                Require.GreaterThanOrEqualTo(() => maxInstances, 1);

                tempDictionary[type] = new BrowserPool(type, maxInstances);
            }

            this.browserPools = new ReadOnlyDictionary<BrowserType, BrowserPool>(tempDictionary);

            this.syncLock = new SemaphoreSlim(1);

            // block on browser creation - we don't want to return from the ctor with the object state in flux
            this.CreateBrowserInstances();
        }

        /// <inheritdoc />
        public int AvailableBrowsers
        {
            get { return this.browserPools.Sum(b => b.Value.AvailableCount); }
        }

        /// <inheritdoc />
        public IEnumerable<BrowserType> AvailableBrowserTypes => this.browserPools.Keys;

        /// <inheritdoc />
        public BrowserAbstract AcquireBrowser(BrowserType browserType)
        {
            BrowserAbstract browser = null;

            if (this.browserPools.ContainsKey(browserType))
            {
                var pool = this.browserPools[browserType];
                browser = pool.WaitForABrowser();

                if (browser == null)
                {
                    Logger.WriteDebug(
                        "Unable to acquire browser {0}", browserType);
                }
            }

            return browser;
        }

        /// <inheritdoc />
        public void ReleaseBrowser(BrowserAbstract browser)
        {
            // This method is not expected to leak any predictable WebDriver exceptions.
            if (browser == null)
            {
                return;
            }

            FiddlerProxy.ClearHeaders(browser.ProcessId);

            try
            {
                // cleanup the previous state
                browser.DeleteAllCookies();

                // Selenium issue: must navigate away from page after cleaning up cookies otherwise some cookies will persist.
                browser.NavigateTo(browser.DefaultPage);
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // suppress any exceptions while removing cookies.
                // If there is a problem, the browser will be marked as Faulted and handled in the next block.
            }

            if (this.browserPools.ContainsKey(browser.BrowserType) &&
                !this.browserPools[browser.BrowserType].PoolContains(browser))
            {
                // put the browser back in rotation
                this.browserPools[browser.BrowserType].ReleaseBrowser(browser);
            }
        }

        /// <inheritdoc />
        public int GetAvailableBrowsersByType(BrowserType browserType)
        {
            return this.browserPools[browserType].AvailableCount;
        }

        /// <inheritdoc />
        public void CreateBrowserInstances()
        {
            // wait a short while for the semaphore, if we can't get it then assume someone else is maintaining the pool
            if (this.syncLock.Wait(100))
            {
                try
                {
                    foreach (var pool in this.browserPools.Values)
                    {
                        for (int i = pool.InstanceCount; i < pool.MaxInstances; i++)
                        {
                            try
                            {
                                BrowserAbstract browser = this.factory.Create(pool.BrowserType);

                                Logger.WriteInfo(
                                    "Registering browser: {0} (PID {1} parent PID {2})",
                                    browser.GetType().Name,
                                    browser.ProcessId,
                                    browser.ParentProcessId);

                                pool.AddInstance(browser);
                            }
                            catch (Exception exception)
                            {
                                Logger.WriteWarning(
                                    exception,
                                    "Unexpected failure while creating a browser of type {0}",
                                    pool.BrowserType);
                            }
                        }
                    }
                }
                finally
                {
                    this.syncLock.Release();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (var pool in this.browserPools.Values)
                    {
                        pool.Dispose();
                    }
                }
            }

            this.syncLock.Dispose();

            this.disposed = true;
        }
    }
}