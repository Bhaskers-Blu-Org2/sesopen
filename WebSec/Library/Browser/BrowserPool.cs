//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Logger;

    /// <summary>
    /// Contains state information for a pool of browsers of a particular <see cref="BrowserType"/>.
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    internal class BrowserPool : IDisposable
    {
        /// <summary>
        /// The instances.
        /// </summary>
        private readonly IList<BrowserAbstract> instances;

        /// <summary>
        /// The pool.
        /// </summary>
        private readonly ConcurrentBag<BrowserAbstract> pool;

        /// <summary>
        /// The timers.
        /// </summary>
        private readonly IDictionary<BrowserAbstract, Stopwatch> timers;

        /// <summary>
        /// true if disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserPool" /> class.
        /// </summary>
        /// <param name="browserType">
        ///     The <see cref="BrowserType"/> that the type will store information about.
        /// </param>
        /// <param name="maxInstances">
        ///     The number of browser instances to maintain in the pool for this browser type.
        /// </param>
        public BrowserPool(BrowserType browserType, int maxInstances)
        {
            this.BrowserType = browserType;
            this.MaxInstances = maxInstances;
            this.instances = new List<BrowserAbstract>();
            this.pool = new ConcurrentBag<BrowserAbstract>();
            this.timers = new ConcurrentDictionary<BrowserAbstract, Stopwatch>();
        }

        /// <summary>
        /// Gets the <see cref="BrowserType"/> that is managed by this pool.
        /// </summary>
        /// <value>
        /// The type of the browser.
        /// </value>
        public BrowserType BrowserType { get; }

        /// <summary>
        /// Gets the total number of browsers that will exist in this pool.
        /// </summary>
        /// <value>
        /// The maximum instances.
        /// </value>
        public int MaxInstances { get; private set; }

        /// <summary>
        /// Gets the count of browsers in the pool.
        /// </summary>
        /// <value>
        /// The number of instances.
        /// </value>
        public int InstanceCount => this.instances.Count;

        /// <summary>
        /// Gets the count of browsers currently available for lease.
        /// </summary>
        /// <value>
        /// The number of available.
        /// </value>
        public int AvailableCount => this.pool.Count;

        /// <summary>
        /// Gets, Returns a list of process identifiers used by browsers in this pool.
        /// </summary>
        /// <value>
        /// A list of identifiers of the process.
        /// </value>
        public IEnumerable<int> ProcessIdentifiers
        {
            get
            {
                int thisProcessId = Process.GetCurrentProcess().Id;

                var liveProcessIDs =
                    this.instances
                        .Select(b => b.ProcessId)
                        .Union(
                            this.instances
                                .Where(b => b.ParentProcessId != thisProcessId)
                                .Select(b => b.ParentProcessId));

                return liveProcessIDs;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Adds a browser instance to the pool.
        /// </summary>
        /// <param name="browser">
        ///     The browser to add.
        /// </param>
        public void AddInstance(BrowserAbstract browser)
        {
            if (browser.BrowserType == this.BrowserType)
            {
                if (!this.timers.ContainsKey(browser))
                {
                    this.timers.Add(new KeyValuePair<BrowserAbstract, Stopwatch>(browser, new Stopwatch()));
                }

                this.instances.Add(browser);
                this.pool.Add(browser);
            }
        }

        /// <summary>
        /// Removes a browser instance from the pool.
        /// </summary>
        /// <param name="browser">
        ///     The browser to remove.
        /// </param>
        public void RemoveInstance(BrowserAbstract browser)
        {
            if (browser != null)
            {
                this.instances.Remove(browser);
                this.timers.Remove(browser);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified browser is available in the pool.
        /// </summary>
        /// <param name="browser">
        ///     The browser to verify.
        /// </param>
        /// <returns>
        /// True if the browser is available in the pool, otherwise false.
        /// </returns>
        public bool PoolContains(BrowserAbstract browser)
        {
            return this.pool.Contains(browser);
        }

        /// <summary>
        /// Safely acquires a lease on the next available browser in the pool.
        /// </summary>
        /// <returns>
        /// The browser if one was available, otherwise null.
        /// </returns>
        public BrowserAbstract WaitForABrowser()
        {
            BrowserAbstract browser;

            this.pool.TryTake(out browser);

            if (browser != null && this.timers.ContainsKey(browser))
            {
                this.timers[browser].Start();
            }

            return browser;
        }

        /// <summary>
        /// Returns a previously acquired browser to the pool.
        /// </summary>
        /// <param name="browser">
        ///     The browser to release.
        /// </param>
        public void ReleaseBrowser(BrowserAbstract browser)
        {
            if (this.instances.Contains(browser))
            {
                this.pool.Add(browser);

                this.timers[browser].Stop();

                this.timers[browser].Reset();
            }
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    BrowserAbstract item;
                    while (this.pool.TryTake(out item))
                    {
                        // drain the queue to remove any object references. 
                    }

                    this.timers.Clear();

                    // cleanly shut down the browsers.
                    foreach (BrowserAbstract browser in this.instances)
                    {
                        try
                        {
                            Logger.WriteInfo(
                                "Disposing of browser: {0} (PID {1})",
                                browser.GetType().Name,
                                browser.ProcessId);

                            browser.Quit();
                        }
                            // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception)
                        {
                            // don't throw in the dispose method
                        }
                    }

                    this.instances.Clear();
                }
            }

            this.disposed = true;
        }
    }
}