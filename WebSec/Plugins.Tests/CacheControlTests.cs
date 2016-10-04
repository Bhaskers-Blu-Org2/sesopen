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
    /// The cache control tests.
    /// </summary>
    [TestClass]
    public class CacheControlTests : PluginTestBase<CacheControlDetector>
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
        /// The cache control positive.
        /// </summary>
        [TestMethod]
        public void CacheControlPositive()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddressSsl}PluginsTestPages/CacheControl.aspx");

            // This detector requires a valid HTTPS connection which is not something we currently have on the test site.
            // Instead we inject the expected headers via the pre-inspection hook.
            this.DetectorPreInspectionAction = () => { this.ResponseHolder.Headers["Cache-Control"] = "public"; };

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            var vuln = vulns.Single(v => v.Title.Equals("HTTPS pages should not be cached"));
            vuln.Evidence.ShouldEqual("Cache-Control: public");
        }

        /// <summary>
        /// The cache control negative no store.
        /// </summary>
        [TestMethod]
        public void CacheControlNegativeNoStore()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddressSsl}PluginsTestPages/CacheControl.aspx");

            this.DetectorPreInspectionAction = () => { this.ResponseHolder.Headers["Cache-Control"] = "no-store"; };

            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The cache control negative unspecified.
        /// </summary>
        [TestMethod]
        public void CacheControlNegativeUnspecified()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddressSsl}PluginsTestPages/CacheControl.aspx");

            this.DetectorPreInspectionAction = () => this.ResponseHolder.Headers.Remove("Cache-Control");

            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The cache control negative no ssl.
        /// </summary>
        [TestMethod]
        public void CacheControlNegativeNoSsl()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/CacheControl.aspx");

            this.DetectorPreInspectionAction = () => { this.ResponseHolder.Headers["Cache-Control"] = "public"; };

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}