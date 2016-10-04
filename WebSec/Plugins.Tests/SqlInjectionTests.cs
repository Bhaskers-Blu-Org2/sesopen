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
    using Library.Engine.Interfaces;
    using Library.PluginBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Sql injection tests.
    /// </summary>
    [TestClass]
    public class SqlInjectionTests : PluginTestBase<SqlInjectionTests.SqlInjectionClone>
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
        /// Test for sql injection discovered based on target delay response.
        /// </summary>
        [TestMethod]
        public void TestSqlInjectionDelayVulnPositive()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/SqlInjectionVulnPage.aspx?delay=");

            // Execute
            var vulns = ExecutePluginTestRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.ElementAt(0).Title.ShouldEqual("Sql Injection Time Delay");
            vulns.ElementAt(0).Evidence.ShouldEqual("Sql Injection Time Delay");
        }

        /// <summary>
        /// Test for sql injection discovered based on target delay response.
        /// </summary>
        [TestMethod]
        public void TestSqlInjectionVulnNegative()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/SqlInjectionVulnPage.aspx?noparam=");

            // Execute and validate that we don't have any vulns
            ExecutePluginTestRequest(target).ShouldBeNullOrEmpty();
        }

        /// <summary>
        /// Test for sql injection discovered based sql error message from the target.
        /// </summary>
        [TestMethod]
        public void TestSqlInjectionGenerateSqlErrorVulnPositive()
        {
            // Setup
            var target =
                Target.Create($"{Constants.VulnerabilitiesAddress}PluginsTestPages/SqlInjectionVulnPage.aspx?error=");

            // Execute
            var vulns = ExecutePluginTestRequest(target).OrderBy(v => v.Title);

            // Validate that both injections are triggering a syntax error
            vulns.Count().ShouldEqual(2);
            vulns.ElementAt(0).Title.ShouldEqual("Sql Injection generate error");
            vulns.ElementAt(0).Evidence.ShouldEqual("syntax error");

            vulns.ElementAt(1).Title.ShouldEqual("Sql Injection Time Delay");
            vulns.ElementAt(1).Evidence.ShouldEqual("syntax error");
        }

        /// <summary>
        /// USe this class for testing to trigger small number of signatures,
        /// one of the tests will delay the response 30 seconds.
        /// </summary>
        public class SqlInjectionClone : SqlInjection
        {
            /// <summary>
            /// The init.
            /// </summary>
            /// <param name="currentcontext">
            /// The current context.
            /// </param>
            /// <param name="target">
            /// The target.
            /// </param>
            public override void Init(IContext currentcontext, ITarget target)
            {
                base.Init(currentcontext, target);

                TestCases.Clear();

                TestCases.Add(
                    new TestCase
                    {
                        TestName = TimingTestCase,
                        InjectionString =
                            new[]
                            {
                                "%20and%201%20in%20(select%20SLEEP(10))"
                            },
                        FilterResponseContentType = new[] { "text/html" }
                    });

                TestCases.Add(
                    new TestCase
                    {
                        TestName = SqlErrorTestCase,
                        InjectionString =
                            new[]
                            {
                                "%20or%201%20=%201"
                            },
                        FilterResponseContentType = new[] { "text/html" }
                    });
            }
        }
    }
}