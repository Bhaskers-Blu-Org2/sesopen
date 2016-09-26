//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Logger;
    using Library.PluginBase;

    /// <summary>
    /// A SQL injection test.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Sql Injection Tester",
        Description = "Test if we can inject a sql delay or trigger a sql error.")]
    public class SqlInjection : PluginBaseAbstract
    {
        /// <summary>
        /// The timing test case.
        /// </summary>
        public const string TimingTestCase = "Sql Injection Time Delay";

        /// <summary>
        /// The SQL error test case.
        /// </summary>
        public const string SqlErrorTestCase = "Sql Injection generate error";

        /// <summary>
        /// The time out in seconds.
        /// </summary>
        private const int TimeOutInSeconds = 10;

        /// <summary>
        /// The baseline latency in seconds.
        /// </summary>
        private const int BaselineLatencyInSeconds = 10;

        /// <summary>
        /// Initializer for the test plugin - the test manager will invoke this at the start of a test
        /// run. Tests that override this method must be sure to call the base.Init function, to ensure
        /// that the base class has the context, target, and results objects created and available, and
        /// if using the default DoTests function would add test case objects to the test cases collection
        /// (which the default DoTests function will use).
        /// </summary>
        /// <param name="currentcontext">
        /// The current context.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.Init(IContext,ITarget)"/>
        public override void Init(IContext currentcontext, ITarget target)
        {
            base.Init(currentcontext, target);

            TestCases.Add(
                new TestCase
                {
                    TestName = SqlErrorTestCase,
                    InjectionString = TestBaseHelper.LoadTestCase("SqlInjectionErrorTestCase", Context).ToArray(),
                    FilterResponseContentType = new[] { "text/html" }
                });

            // time delay 14s choose based on the http request timeout that we have now in the system
            TestCases.Add(
                new TestCase
                {
                    TestName = TimingTestCase,
                    InjectionString = TestBaseHelper.LoadTestCase("SqlInjectionTimingTestCase", Context).ToArray(),
                    FilterResponseContentType = new[] { "text/html" }
                });
        }

        /// <summary>
        /// Inside a test check to see if the match string gets found on the response page,regex used for
        /// matching if that is the case then log a vulnerability.
        /// </summary>
        /// <param name="webRequestContext">
        /// The web Request Context.
        /// </param>
        /// <param name="testCase">
        /// The test Case.
        /// </param>
        /// <param name="testedParam">
        /// The tested Parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested Val.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.CheckForVuln(WebRequestContext,TestCase,string,string)"/>
        protected override void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testCase,
            string testedParam,
            string testValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;

            try
            {
                string[] sqlerrors =
                {
                    "sql syntax",
                    "sql error",
                    "syntax error"
                };

                var evidence =
                    sqlerrors.FirstOrDefault(
                        e => response.ResponseContent.IndexOf(e, StringComparison.InvariantCultureIgnoreCase) > -1);

                if (!string.IsNullOrEmpty(evidence) ||
                    (testCase.TestName.Equals(TimingTestCase, StringComparison.InvariantCultureIgnoreCase) &&
                     response.Latency.TotalSeconds >= BaselineLatencyInSeconds + TimeOutInSeconds + 1))
                {
                    this.AddVulnerability(
                        testCase.TestName,
                        testedParam, 
                        testValue,
                        evidence ?? TimingTestCase,
                        response,
                        testCase, 
                        VulnerabilityLevelEnum.High);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex);
            }
        }
    }
}