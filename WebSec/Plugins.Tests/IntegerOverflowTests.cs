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
    /// The integer overflow tests.
    /// </summary>
    [TestClass]
    public class IntegerOverflowTests : PluginTestBase<IntOverflowTester>
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
        /// The test integer overflow negative no value.
        /// </summary>
        [TestMethod]
        public void TestIntegerOverflowNegativeNoValue()
        {
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XNoJavascript.aspx?q=");

            // Execute
            var vulns = ExecutePluginTestRequest(target);
            vulns.ShouldBeEmpty();
        }

        /// <summary>
        /// The test integer overflow negative with value.
        /// </summary>
        [TestMethod]
        public void TestIntegerOverflowNegativeWithValue()
        {
            var target = Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/XNoJavascript.aspx?q=1");

            // Execute
            var vulns = ExecutePluginTestRequest(target);
            vulns.ShouldBeEmpty();
        }
    }
}