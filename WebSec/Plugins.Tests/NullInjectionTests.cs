//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using Common;
    using Common.TestInfrastructure;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The null injection tests.
    /// </summary>
    [TestClass]
    public class NullInjectionTests : PluginTestBase<NullInjectionTester>
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
        /// The test null injection positive.
        /// </summary>
        [TestMethod]
        public void TestNullInjectionPositive()
        {
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/ServerError503.aspx?q=test");

            // Execute
            var vulns = ExecutePluginTestRequest(target);

            vulns.Count.ShouldEqual(2);
        }

        /// <summary>
        /// The test null injection negative.
        /// </summary>
        [TestMethod]
        public void TestNullInjectionNegative()
        {
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XNoJavascript.aspx?q=");

            // Execute
            var vulns = ExecutePluginTestRequest(target);
            vulns.ShouldBeEmpty();
        }
    }
}