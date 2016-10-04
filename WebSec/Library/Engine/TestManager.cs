//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Interfaces;
    using Logger;
    using PluginBase;

    /// <summary>
    /// Manages a set of tests and runs them on any given target page.
    /// </summary>
    internal sealed class TestManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestManager" /> class.
        /// </summary>
        internal TestManager()
        {
            this.Vulnerabilities = new ConcurrentQueue<Vulnerability>();
            this.ActiveTests = new List<Type>();
        }

        /// <summary>
        /// Gets the active tests.
        /// </summary>
        /// <value>
        /// The active tests.
        /// </value>
        public List<Type> ActiveTests { get; }

        /// <summary>
        /// Gets the vulnerabilities.
        /// </summary>
        internal ConcurrentQueue<Vulnerability> Vulnerabilities { get; }

        /// <summary>
        /// Grabs a page and runs all tests against it.
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <param name="target">
        ///     The target.
        /// </param>
        internal void RunTests(IContext context, ITarget target)
        {
            if (this.ActiveTests.Count == 0)
            {
                Logger.WriteWarning("No active tests have been found.");
                return;
            }

            // iterate through all active tests and run them
            foreach (Type activeTest in this.ActiveTests)
            {
                var pluginBaseAbstract = Activator.CreateInstance(activeTest) as PluginBaseAbstract;

                if (pluginBaseAbstract == null)
                {
                    throw new NullReferenceException("{0} can't be instantiated".FormatIc(activeTest.Name));
                }

                // execute all tests
                if (!pluginBaseAbstract.TestBaseType.Equals(TestBaseType.Test))
                {
                    continue;
                }

                this.RunTest(pluginBaseAbstract, context, target);
            }
        }

        /// <summary>
        /// Executes the test operation.
        /// </summary>
        /// <param name="pluginInstance">
        /// The plugin instance.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        private void RunTest(
            PluginBaseAbstract pluginInstance,
            IContext context,
            ITarget target)
        {
            try
            {
                Logger.WriteDebug("Run test: {0}", pluginInstance.GetType().Name);

                pluginInstance.Init(context, target);
                pluginInstance.DoTests();

                // get vulnerabilities
                foreach (var vuln in pluginInstance.Vulnerabilities)
                {
                    this.Vulnerabilities.Enqueue(vuln);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex);
            }
        }
    }
}