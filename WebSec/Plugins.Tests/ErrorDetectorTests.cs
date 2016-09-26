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
    /// The error detector tests.
    /// </summary>
    [TestClass]
    public class ErrorDetectorTests : PluginTestBase<ErrorDetector>
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
        /// The dot net exception positive.
        /// </summary>
        [TestMethod]
        public void DotNetExceptionPositive()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}InvalidOperationExceptionScreen.aspx");

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            // There should be one well formed vuln
            vulns.Count.ShouldEqual(1);
            var vuln = vulns.Single(x => x.Title == ".Net Exception");

            // An exception should be found.
            vuln.Evidence.ShouldContain(@"System.InvalidOperationException");
        }

        /// <summary>
        /// The server error positive.
        /// </summary>
        [TestMethod]
        public void ServerErrorPositive()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/ServerError200.aspx");

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            // There should be one well formed vuln
            vulns.Count.ShouldEqual(1);
            var vuln = vulns.Single(x => x.Title == "Page Unavailable");

            // An expected message should be returned.
            (vuln.Evidence.IndexOfOi(@"The page you want isn't available") > -1).ShouldBeTrue();
        }

        /// <summary>
        /// The negative case.
        /// </summary>
        [TestMethod]
        public void NegativeCase()
        {
            // Setup
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");

            var vulns = ExecutePluginDetectorRequest(target);

            // Validate
            vulns.ShouldBeEmpty();
        }
    }
}