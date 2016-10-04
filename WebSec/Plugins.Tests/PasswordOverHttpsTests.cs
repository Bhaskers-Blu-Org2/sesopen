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
    /// The password over https tests.
    /// </summary>
    [TestClass]
    public class PasswordOverHttpsTests : PluginTestBase<PasswordOverHttpsDetector>
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
        /// The test_password_https_passive_vuln.
        /// </summary>
        [TestMethod]
        public void TestPasswordHttpsPassiveVuln()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}vuln_password.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            // a vuln for not requiring HTTPS.
            vulns.Count.ShouldEqual(1);
            var vuln = vulns.Single(x => x.Title == "Password fields should only be served over HTTPS");
            vuln.Evidence.ShouldEqual($"{Constants.VulnerabilitiesAddress}vuln_password.aspx");
        }

        /// <summary>
        /// The test_password_https_negative_case.
        /// </summary>
        [TestMethod]
        public void TestPasswordHttpsNegativeCase()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddressSsl}vuln_password.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            // nothing to see here
            vulns.ShouldBeEmpty();
        }
    }
}