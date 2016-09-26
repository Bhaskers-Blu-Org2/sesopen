//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// A HTTP response splitting injection.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "HTTP-response splitting",
        Description = "Test for HTTP-response splitting vulnerabilities")]
    public sealed class HttpResponseSplittingInjection : PluginBaseAbstract
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
                InjectionString = new[]
                {
                    @"\r\nFrom:%20someone@bing.com",
                    @"%0d%0aFrom:%20someone@bing.com",
                    @"%%0d0d%%0a0aFrom:%20someone@bing.com",
                    @"%00%0d%0a0a0aFrom:%20someone@bing.com",
                    @"%250d%0a0aFrom:%20someone@bing.com"
                }
            });
        }

        /// <summary>
        /// Overriding the Check Vulnerability implementation because we just want to look in the response headers.
        /// </summary>
        /// <param name="webRequestContext">
        /// The web Request Context.
        /// </param>
        /// <param name="testcase">
        /// The test case.
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
            TestCase testcase,
            string testedParam,
            string testValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;

            var fromHeaderValue = response.Headers["From"];
            if (response.Headers == null
                || string.IsNullOrWhiteSpace(fromHeaderValue))
            {
                return;
            }

            // Have we injected a from header and does it contain the domain we are attempting to redirect to?
            if (fromHeaderValue.IndexOf("someone@bing.com", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                Vulnerabilities.Enqueue(new Vulnerability
                {
                    Title = "HTTP Response Splitting - Newline Injection",
                    Level = (int)VulnerabilityLevelEnum.High,
                    TestedParam = testedParam,
                    TestedVal = testValue,
                    HttpResponse = response,
                    Evidence = $"From : {fromHeaderValue}",
                    MatchString = testcase.MatchString,
                    TestPlugin = GetType().Name
                });
            }
        }
    }
}