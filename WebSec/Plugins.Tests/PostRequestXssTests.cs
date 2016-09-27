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
    /// The post request xss tests.
    /// </summary>
    [TestClass]
    public class PostRequestXssTests : PluginTestBase<UserInteractionXssTest>
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
        /// The test post request xss.
        /// </summary>
        [TestMethod]
        public void TestPostRequestXssPositive()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);

                var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/PostRequestXssTest.html");

                // Execute
                var vulns = ExecutePluginTestRequest(target);

                vulns.Count.ShouldEqual(1);
                vulns.ElementAt(0).TestedVal.ShouldEqual("alert(501337)");
            }
        }

        /// <summary>
        /// The test post request xss.
        /// </summary>
        [TestMethod]
        public void TestPostRequestXssNegative()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);

                // Setup
                var target = Target.Create("http://www.bing.com");

                // Execute
                var vulns = ExecutePluginTestRequest(target);

                vulns.Count.ShouldEqual(0);
            }
        }
    }
}