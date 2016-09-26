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
    /// The content type mismatch detector tests.
    /// </summary>
    [TestClass]
    public class ContentTypeMismatchDetectorTests : PluginTestBase<ContentTypeMismatchDetector>
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
        /// The test empty content type vuln.
        /// </summary>
        [TestMethod]
        public void TestEmptyContentTypeVuln()
        {
            // Setup
            var target = Target.Create("http://www.bing.com");

            this.DetectorPreInspectionAction = () => { this.ResponseHolder.ResponseContentType = string.Empty; };

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.Equals("Content type header is missing.").ShouldBeTrue();
            vulns.ElementAt(0).Evidence.Equals("Content type header is missing.").ShouldBeTrue();
        }

        /// <summary>
        /// The test html content type mismatch vuln.
        /// </summary>
        [TestMethod]
        public void TestHtmlContentTypeMismatchVuln()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptions.aspx");

            this.DetectorPreInspectionAction = () => { this.ResponseHolder.ResponseContentType = "application/json"; };

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.Equals("Html content type mismatched.").ShouldBeTrue();
            vulns.ElementAt(0).Evidence.ShouldNotBeNullOrEmpty();
        }

        /// <summary>
        /// The test utf7 content type mismatch vuln.
        /// </summary>
        [TestMethod]
        public void TestUtf7ContentTypeMismatchVuln()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptions.aspx");
            this.DetectorPreInspectionAction =
                () => { this.ResponseHolder.ResponseContentType = "text/html; charset=UTF-7"; };

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.Equals("Charset of the page should not be utf-7.").ShouldBeTrue();
            vulns.ElementAt(0).Evidence.ShouldNotBeNullOrEmpty();
        }

        /// <summary>
        /// The test html content type negative.
        /// </summary>
        [TestMethod]
        public void TestHtmlContentTypeNegative()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptions.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}