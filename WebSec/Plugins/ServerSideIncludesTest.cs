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
    using Library.PluginBase;

    /// <summary>
    /// A server side includes test.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Server Side Includes Tester",
        Description = "Server Side Includes")]
    public sealed class ServerSideIncludesTest : PluginBaseAbstract
    {
        /// <summary>
        /// The include virtual indicators.
        /// </summary>
        private readonly string[] includeVirtualIndicators = { "<configuration>", "</configuration>" };

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

            TestCases.Add(new TestCase
            {
                TestName = "SSI Injection",
                InjectionString = new[]
                {
                    "<!--#INCLUDE VIRTUAL=\"/web.config\"-->",
                    "<!--#exec cmd=\"ls\" -->",
                    "<!--#exec cmd=\"dir\" -->"
                },
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
        /// <param name="testParameter">
        /// The tested Parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested Val.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.CheckForVuln(WebRequestContext,TestCase,string,string)"/>
        protected override void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testCase,
            string testParameter,
            string testValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;

            if (string.IsNullOrWhiteSpace(response.ResponseContent))
            {
                return;
            }

            string evidence = string.Empty;
            if (
                response.ResponseContent.IndexOf(
                    "[an error occurred while processing this directive]",
                    StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                evidence += "[an error occurred while processing this directive]";
            }

            if (
                response.ResponseContent.IndexOf("Volume Serial Number is", StringComparison.InvariantCultureIgnoreCase) >
                -1)
            {
                evidence += "Volume Serial Number is";
            }

            if (
                response.ResponseContent.IndexOf(
                    "is not recognized as an internal or external command",
                    StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                evidence += "is not recognized as an internal or external command";
            }

            if (!string.IsNullOrEmpty(evidence))
            {
                this.AddVulnerability(
                    this.Name,
                    testParameter,
                    testValue,
                    "A server-side directive was executed:" + evidence,
                    response,
                    testCase,
                    VulnerabilityLevelEnum.High);
            }

            if (
                this.includeVirtualIndicators.All(
                    t => response.ResponseContent.IndexOf(t, StringComparison.InvariantCultureIgnoreCase) > -1))
            {
                this.AddVulnerability(
                    this.Name,
                    testParameter,
                    testValue,
                    "<configuration></configuration> present",
                    response,
                    testCase,
                    VulnerabilityLevelEnum.High);
            }
        }
    }
}