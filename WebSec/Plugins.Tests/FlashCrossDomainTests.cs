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
    /// The flash cross domain tests.
    /// </summary>
    [TestClass]
    public class FlashCrossDomainTests : PluginTestBase<CrossDomainTrust>
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
        /// The flash cross domain policy positive.
        /// </summary>
        [TestMethod]
        public void FlashCrossDomainPolicyPositive()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/");

            // Execute
            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.Count.ShouldEqual(3);

            // allow-access-from w/ wide open domain trust
            vulns.ElementAt(0).Title.ShouldEqual("Trust of all domains");
            (vulns.ElementAt(0).Evidence.IndexOfOi("<allow-access-from domain=\"*\" secure=\"true\"") > -1).ShouldBeTrue();

            // allow-http-request-headers-from w/ wide open domain trust
            vulns.ElementAt(1).Title.ShouldEqual("Trust of all domains");
            (vulns.ElementAt(1)
                .Evidence.IndexOfOi("<allow-http-request-headers-from domain=\"*\" headers=\"Header\"") > -1)
                .ShouldBeTrue();

            // insecure trust
            vulns.ElementAt(2).Title.ShouldEqual("Insecure trust with a domain");
            (vulns.ElementAt(2).Evidence.IndexOfOi("<allow-access-from domain=\"*.contoso.com\" secure=\"false\"") > -1)
                .ShouldBeTrue();
        }

        /// <summary>
        /// The flash cross domain policy negative.
        /// </summary>
        [TestMethod]
        public void FlashCrossDomainPolicyNegative()
        {
            // Setup
            var target = Target.Create("http://www.bing.com");

            // Execute
            var vulns = ExecutePluginDetectorRequest(target);

            // Validate that we shouldn't have any vulnerability
            vulns.ShouldBeEmpty();
        }
    }
}