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
    /// The directory browsing tests.
    /// </summary>
    [TestClass]
    public class DirectoryBrowsingTests : PluginTestBase<DirectoryBrowsingDetector>
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
        /// The test_directory_browsing_passive_vuln.
        /// </summary>
        [TestMethod]
        public void TestDirectoryBrowsingPassiveVuln()
        {
            // Setup
            var target = Target.Create(Constants.VulnerabilitiesAddress);

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.Single(x => x.Title == "Information Leakage - Directory Browsing Is Enabled").ShouldNotBeNull();
        }

        /// <summary>
        /// The test_directory_browsing_negative_case.
        /// </summary>
        [TestMethod]
        public void TestDirectoryBrowsingNegativeCase()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}NoBrowsing");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate - nothing to see
            vulns.ShouldBeEmpty();
        }
    }
}