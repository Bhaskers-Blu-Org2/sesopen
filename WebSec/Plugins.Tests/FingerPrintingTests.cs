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
    /// The finger printing tests.
    /// </summary>
    [TestClass]
    public class FingerPrintingTests : PluginTestBase<FingerPrintingDetector>
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
        /// The finger print negative.
        /// </summary>
        [TestMethod]
        public void FingerPrintNegative()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/FingerPrintPage.aspx?noheaders=test");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate that we shouldn't have any vulnerability
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The test x content type options missing.
        /// </summary>
        [TestMethod]
        public void TestXContentTypeOptionsMissing()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptionsMissing.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.ShouldEqual("Fingerprint Detector");
            vulns.ElementAt(0).Evidence.ShouldEqual("x-content-type-options=random");
        }

        /// <summary>
        /// The test x content type options not missing.
        /// </summary>
        [TestMethod]
        public void TestXContentTypeOptionsNotMissing()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XContentTypeOptions.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate that we shouldn't have any vulnerability
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The test xss protection set to zero.
        /// </summary>
        [TestMethod]
        public void TestXXssProtectionSetToZero()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XXssProtectionSetToZero.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.ShouldEqual("Fingerprint Detector");
            vulns.ElementAt(0).Evidence.ShouldEqual("x-xss-protection=0");
        }

        /// <summary>
        /// The test x xss protection set to one.
        /// </summary>
        [TestMethod]
        public void TestXssProtectionSetToOne()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XXssProtectionSetToOne.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate that we don't have any vulnerability
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The test exposed possible authentication headers over http.
        /// </summary>
        [TestMethod]
        public void TestExposedPossibleAuthHeadersOverHttp()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/FingerPrintPage.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.ShouldEqual("Possible exposed secret over non secure protocol.");
            vulns.ElementAt(0).Evidence.ShouldEqual("X-AIS-AuthToken=test user token id");
        }

        /// <summary>
        /// The test exposed possible ports.
        /// </summary>
        [TestMethod]
        public void TestExposedPossiblePortsPositive()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/FingerPrintPage.aspx?port=test");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.ShouldEqual("Possible ports exposed.");
            vulns.ElementAt(0).Evidence.ShouldEqual("random=http://domain.com:12345");
        }

        /// <summary>
        /// The test exposed possible ports negative.
        /// </summary>
        [TestMethod]
        public void TestExposedPossiblePortsNegative()
        {
            // Setup
            var target =
                Target.Create(
                    $"{Constants.VulnerabilitiesAddress}PluginsTestPages/FingerPrintPage.aspx?portNegative=test");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate that we don't have any vulnerability
            vulns.ShouldBeEmpty();
        }
    }
}