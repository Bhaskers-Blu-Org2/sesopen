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
    /// The password auto-complete tests.
    /// </summary>
    [TestClass]
    public class PasswordAutocompleteTests : PluginTestBase<PasswordAutocompleteDetector>
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
        /// The test_password_autocomplete_passive_vuln.
        /// </summary>
        [TestMethod]
        public void TestPasswordAutocompletePassiveVuln()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}AutocompletePassword.aspx");

            var vulns = this.ExecutePluginDetectorRequest(target);

            // Validate           
            // A vuln for a password input with auto-complete enabled, 
            vulns.Count.ShouldEqual(1);
            var vuln = vulns.Single(x => x.Title == "Password input has autocomplete enabled");
            vuln.Evidence.ShouldEqual("<input type=\"password\" autocomplete=\"true\">");
        }

        /// <summary>
        /// The test_password_negative.
        /// </summary>
        [TestMethod]
        public void TestPasswordNegative()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}vuln_password.aspx");

            var vulns = this.ExecutePluginDetectorRequest(target);

            // Validate
            // There should be nothing to see
            vulns.Count.ShouldEqual(0);
        }
    }
}