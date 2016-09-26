//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System.Linq;
    using Common;
    using Common.TestInfrastructure;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The http response splitting tests.
    /// </summary>
    [TestClass]
    public class HttpResponseSplittingTests : PluginTestBase<HttpResponseSplittingInjection>
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
        /// The test http response splitting vulnerability.
        /// </summary>
        [TestMethod]
        public void TestHttpResponseSplittingVuln()
        {
            // Setup
            var target =
                Target.Create(
                    $"{Constants.VulnerabilitiesAddress}PluginsTestPages/HttpResponseSplittingVulnPage.aspx?testedParam=value");

            // Execute
            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.Count.ShouldEqual(5);

            var firstVuln = vulns.ElementAt(0);
            firstVuln.TestPlugin.Equals("HttpResponseSplittingInjection").ShouldBeTrue();
            firstVuln.Evidence.Equals("From : someone@bing.com").ShouldBeTrue();
            firstVuln.Title.Equals("HTTP Response Splitting - Newline Injection").ShouldBeTrue();
        }

        /// <summary>
        /// The test http response splitting negative.
        /// </summary>
        [TestMethod]
        public void TestHttpResponseSplittingNegative()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptions.aspx?q=value");

            // Execute
            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}