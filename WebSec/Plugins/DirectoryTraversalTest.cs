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
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Directory Traversal Tester",
        Description = "A directory traversal (or path traversal) consists in exploiting insufficient security validation" +
        " / sanitization of user-supplied input file names, so that characters representing \"traverse to parent directory\" are passed through to the file APIs")]
    public sealed class DirectoryTraversalTest : PluginBaseAbstract
    {
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
                TestName = "DT Injection",
                InjectionString = TestBaseHelper.LoadTestCase("DirectoryTraversalTest", Context).ToArray(),
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

            if (response.ResponseContent.IndexOfOi("<system.web>") > -1 &&
                response.ResponseContent.IndexOfOi("</system.web>") > -1)
            {
                 this.AddVulnerability(
                    this.Name,
                    testParameter,
                    testValue,
                    "Directory traversal attack succeeded.",
                    response,
                    testCase,
                    VulnerabilityLevelEnum.High);
            }
        }
    }
}