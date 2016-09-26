//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Common.TestInfrastructure;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The open redirect tester test.
    /// </summary>
    [TestClass]
    public class OpenRedirectTesterTest : PluginTestBase<OpenRedirectTester>
    {
        /// <summary>
        /// Class cleanup.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestCleanup.Cleanup();
        }

        /// <summary>
        /// The test no open redirects.
        /// </summary>
        [TestMethod]
        public void TestNoOpenRedirects()
        {
            // Setup
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);
                var target =
                    Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XNoJavascript.aspx?q=junk");

                // Execute
                var vulns = ExecutePluginTestRequest(target);

                // Validate
                vulns.ShouldBeEmpty();
            }
        }

        /// <summary>
        /// The test open redirect server side.
        /// </summary>
        [TestMethod]
        public void TestOpenRedirectServerSide()
        {
            // Setup
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);
                var target =
                    Target.Create(
                        $"{Constants.VulnerabilitiesAddress}PluginsTestPages/XOpenRedirectToBadPlaces.aspx?q=junk");

                // Execute
                var vulns = ExecutePluginTestRequest(target);

                // Validate
                vulns.Count.ShouldEqual(2);
                vulns.ElementAt(0).Title.ShouldEqual("Open Redirect");
                vulns.ElementAt(0).Evidence.ShouldEqual("www.ikeahackers.net");
                vulns.ElementAt(1).Title.ShouldEqual("Open Redirect");
                vulns.ElementAt(1).Evidence.ShouldEqual("www.ikeahackers.net");
            }
        }

        /// <summary>
        /// The test open redirect client side http.
        /// </summary>
        [TestMethod]
        public void TestOpenRedirectClientSideHttp()
        {
            // Setup
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);
                var target =
                    Target.Create(
                        $"{Constants.VulnerabilitiesAddress}PluginsTestPages/XOpenRedirectToBadPlacesHTTP.aspx?q=junk");

                // Execute
                var vulns = ExecutePluginTestRequest(target);

                // Validate
                vulns.Count.ShouldEqual(2);
                vulns.ElementAt(0).Title.ShouldEqual("Open Redirect");
                vulns.ElementAt(0).Evidence.ShouldEqual("www.ikeahackers.net");
                vulns.ElementAt(1).Title.ShouldEqual("Open Redirect");
                vulns.ElementAt(1).Evidence.ShouldEqual("www.ikeahackers.net");
            }
        }
    }
}