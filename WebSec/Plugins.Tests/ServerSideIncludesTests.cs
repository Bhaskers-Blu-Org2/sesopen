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
    /// The server side includes tests.
    /// </summary>
    [TestClass]
    public class ServerSideIncludesTests : PluginTestBase<ServerSideIncludesTest>
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
        /// The test server side includes positive.
        /// </summary>
        [TestMethod]
        public void TestServerSideIncludesPositive()
        {
            // Setup
            var target =
                Target.Create(
                    $"{Constants.VulnerabilitiesAddress}PluginsTestPages/ServerSideIncludesVulnPage.aspx?testedParam=value");

            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.Count.ShouldEqual(6);
            vulns.Count(
                v =>
                    v.Title.Equals("Server Side Includes Tester") &&
                    v.Evidence.Equals("<configuration></configuration> present")).ShouldEqual(3);
            vulns.Count(
                v =>
                    v.Title.Equals("Server Side Includes Tester") &&
                    v.Evidence.Equals("A server-side directive was executed:Volume Serial Number is")).ShouldEqual(3);
        }

        /// <summary>
        /// The test server side includes negative.
        /// </summary>
        [TestMethod]
        public void TestServerSideIncludesNegative()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptions.aspx");
            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}