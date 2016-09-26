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
    /// The silverlight cross domain tests.
    /// </summary>
    [TestClass]
    public class SilverlightCrossDomainTests : PluginTestBase<SilverlightCrossDomainTrust>
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
        /// The silverlight cross domain policy positive.
        /// </summary>
        [TestMethod]
        public void SilverlightCrossDomainPolicyPositive()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/");

            // Execute
            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);

            // allow access w/ wide open domain trust
            vulns.ElementAt(0).Title.ShouldEqual("Trust of all domains");
            (vulns.ElementAt(0).Evidence.IndexOfOi("<domain uri=\"*\"") > -1).ShouldBeTrue();
        }

        /// <summary>
        /// The silverlight cross domain policy negative.
        /// </summary>
        [TestMethod]
        public void SilverlightCrossDomainPolicyNegative()
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