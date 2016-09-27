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
    /// (Unit Test Class) a click jacking tests.
    /// </summary>
    /// <seealso cref="T:BackScatterIntegrationTests.Plugins.PluginTestBase{BackScatterPlugins.ClickJacking}"/>
    [TestClass]
    public class ClickJackingTests : PluginTestBase<ClickJacking>
    {
        /// <summary>
        /// The test protection headers not found.
        /// </summary>
        [TestMethod]
        public void TestProtectionHeadersNotFound()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}AutocompletePassword.aspx");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0)
                .Evidence.ShouldEqual(
                    "Custom headers, suitable for protection, x-frame-options and Content-Security-Policy are missing.");
            vulns.ElementAt(0)
                .Title.ShouldEqual("No custom headers to protect this target for being i-framed were found.");
        }

        /// <summary>
        /// The test protection found but allows wrong domain.
        /// </summary>
        [TestMethod]
        public void TestProtectionFoundButAllowsWrongDomain()
        {
            // Setup
            var target =
                Target.Create(
                    $"{Constants.VulnerabilitiesAddress}PluginsTestPages/ClickjackingTestPage.aspx?wrongProtection=true");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Evidence.ShouldEqual("Custom header x-frame-options, but the value domain is wrong.");
            vulns.ElementAt(0).Title.ShouldEqual("Custom header with the wrong value found.");
        }

        /// <summary>
        /// The test negative click jacking.
        /// </summary>
        [TestMethod]
        public void TestNegativeClickJacking()
        {
            // Setup
            var target =
                Target.Create(
                    $"{Constants.VulnerabilitiesAddress}PluginsTestPages/ClickjackingTestPage.aspx?protection=true");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The test click jacking empty headers.
        /// </summary>
        [TestMethod]
        public void TestClickJackingEmptyHeaders()
        {
            // Setup
            var target =
                Target.Create(
                    $"{Constants.VulnerabilitiesAddress}PluginsTestPages/ClickjackingTestPage.aspx?wrongProtection=true");

            // clear headers
            this.DetectorPreInspectionAction = () => this.ResponseHolder.Headers.Clear();

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}