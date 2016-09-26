//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System;
    using System.Linq;
    using Common;
    using Common.TestInfrastructure;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The information leakage tests.
    /// </summary>
    [TestClass]
    public class InformationLeakageTests : PluginTestBase<InformationLeakageDetector>
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
        /// The test_information_leakage_for_path_and_address_vulns.
        /// </summary>
        [TestMethod]
        public void TestInformationLeakageForPathAndAddressVulns()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}InformationLeakage.aspx");

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(2);
            var vuln = vulns.Single(x => x.Title == "Information Leakage - File Path Disclosure");

            var evidences = vuln.Evidence.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            evidences.ShouldContainOnly(
                @"\\server1\share\folder\file.ext",
                @"\\server2\share\folder",
                @"\\server3\share",
                @"\\server4",
                @"c:\folder\file.ext",
                @"z:\folder",
                @"J:\UPPER");

            // Validate
            vuln = vulns.Single(x => x.Title == "Information Leakage - IP Address Disclosure");

            evidences = vuln.Evidence.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // IPV4 cases
            evidences.ShouldContainOnly(
                @"192.168.100.100",
                @"10.55.100.1");
        }

        /// <summary>
        /// The test_information_leakage_for_aspdotnet_error_page.
        /// </summary>
        [TestMethod]
        public void TestInformationLeakageForAspdotnetErrorPage()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}InvalidOperationExceptionScreen.aspx");

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate

            // There should be one well formed vuln
            vulns.Count.ShouldEqual(1);
            vulns.SingleOrDefault(x => x.Title == "Information Leakage - File Path Disclosure").ShouldNotBeNull();
        }

        /// <summary>
        /// The test information leakage exchange secure cookies over http.
        /// </summary>
        [TestMethod]
        public void TestInformationLeakageExchangeSecureCookiesOverHttpPositive()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/CookieTesterPage.aspx?secure=test");

            var vulns = ExecutePluginDetectorRequest(target);

            // todo browser does not collect cookies yet, implement this
            // Validate
            vulns.Count.ShouldEqual(1);
            var vuln =
                vulns.Single(x => x.Title == "Information Leakage - Secured cookie exchanged over non-secure http.");

            var evidences = vuln.Evidence.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            evidences.ShouldContainOnly("SecuredCookie");
        }

        /// <summary>
        /// The test information leakage exchange secure cookies over http negative.
        /// </summary>
        [TestMethod]
        public void TestInformationLeakageExchangeSecureCookiesOverHttpNegative()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/CookieTesterPage.aspx");

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}