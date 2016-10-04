//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Interfaces;
    using Logger;
    using PluginBase;
    using Constants = Library.Constants;

    /// <summary>
    /// The main worker of the scanner, the "controller" of all the other components.
    /// </summary>
    public static class Engine
    {
        /// <summary>
        /// Starts an engine.
        /// </summary>
        /// <param name="urls">
        /// The urls.
        /// </param>
        /// <param name="activePlugins">
        /// The active plugins. A collection of pairs plugin and state. If empty then run all plugins.
        /// </param>
        /// <param name="useFullPayloadsData">use full payloads data</param>
        /// <param name="verbose">
        /// true to verbose the output, otherwise false.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process start engine in this collection.
        /// </returns>
        public static IEnumerable<Vulnerability> StartEngine(
            IEnumerable<string> urls,
            IReadOnlyDictionary<string, bool> activePlugins,
            bool useFullPayloadsData,
            bool verbose)
        {
            Logger.Verbose = verbose;
            Logger.WriteInfo("Spin up a new scan");

            RegisterObjects.Register(useFullPayloadsData);
            var targetQueue = new Queue<ITarget>();
            var vulnerabilities = new List<Vulnerability>();

            // enqueue urls for scanning
            foreach (ITarget target in urls.Select(Target.Create).Where(target => target != null))
            {
                targetQueue.Enqueue(target);
            }

            // get tests and detectors
            IEnumerable<Type> plugins =
                FilterActivePlugin(
                    ReflectionHelper.GetAllTypes<PluginBaseAbstract>(Constants.PluginDllName).ToArray(),
                    activePlugins);

            // note this scan.AuthenticationPassword should be make secure in the database, and then pass trough entity framework as a secure string
            var context = new Context();
            context.ActiveDetectors.AddRange(plugins);

            // create a test manager
            var testManager = new TestManager();
            testManager.ActiveTests.AddRange(plugins);

            // keep running 
            while (targetQueue.Count > 0)
            {
                try
                {
                    // get a target for testing
                    ITarget target = targetQueue.Dequeue();

                    if (!IsValidUrl(context, target.Uri.OriginalString))
                    {
                        vulnerabilities.Add(new Vulnerability { Title = $"Invalid url : { target.Uri.OriginalString }" });
                        continue;
                    }

                    // give it to the test manager to run tests on
                    testManager.RunTests(context, target);

                    // Save vulns found in tests and detectors into database
                    var vulns = testManager.Vulnerabilities.Union(context.Vulnerabilities.Values).ToList();
                    if (!vulns.Any())
                    {
                        continue;
                    }

                    // clear and log found vulns, as we have saved them
                    Vulnerability vulnerability;
                    while (testManager.Vulnerabilities.TryDequeue(out vulnerability))
                    {
                        Logger.WriteInfo("Vulnerability '{0}' found", vulnerability.Title);
                        vulnerabilities.Add(vulnerability);
                    }

                    context.Vulnerabilities.Clear();

                    vulnerabilities.AddRange(vulns);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex);
                }
            }

            DisposeObjects.Dispose();

            Logger.WriteInfo("Scan completed, check the results.");

            return vulnerabilities;
        }

        /// <summary>
        /// Enumerates filter active plugin in this collection.
        /// </summary>
        /// <param name="plugins">
        /// The plugins.
        /// </param>
        /// <param name="activePlugins">
        /// The active plugins. A collection of pairs plugin and state.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process filter active plugin in this
        /// collection.
        /// </returns>
        private static IEnumerable<Type> FilterActivePlugin(
            IEnumerable<Type> plugins,
            IReadOnlyDictionary<string, bool> activePlugins)
        {
            // no plugins active configuration has been found, then load all plugins
            return activePlugins.Count == 0
                ? plugins
                : plugins.Where(p => activePlugins.ContainsKey(p.Name) && activePlugins[p.Name]);
        }

        /// <summary>
        /// Query if 'context' is valid URL.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="url">
        /// URL of the document.
        /// </param>
        /// <returns>
        /// true if valid url, false if not.
        /// </returns>
        private static bool IsValidUrl(IContext context, string url)
        {
            var webRequestContext =
                context.SendRequest(new ContextSendRequestParameter { Url = url });

            context.ReleaseBrowser(webRequestContext.Browser);

            var statuCode = (int)webRequestContext.ResponseHolder.StatusCode;

            // status code 9 means trusted failure, cert issues, that we want to surface
            return statuCode < 400 && statuCode != 9;
        }
    }
}