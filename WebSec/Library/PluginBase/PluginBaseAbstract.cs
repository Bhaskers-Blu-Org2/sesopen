//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.PluginBase
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using Browser;
    using Common.Validation;
    using Engine;
    using Engine.Interfaces;
    using Logger;

    /// <summary>
    /// Test and detector base abstract class.
    /// </summary>
    public abstract class PluginBaseAbstract
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBaseAbstract" /> class.
        /// </summary>
        protected PluginBaseAbstract()
        {
            this.Vulnerabilities = new ConcurrentQueue<Vulnerability>();

            this.TestCases = new List<TestCase>();

            // Only one TestDetectorBaseTags per instance.
            var attribute =
                GetType().GetCustomAttributes(true).SingleOrDefault(b => b is TestBaseAttribute) as TestBaseAttribute;

            if (attribute != null)
            {
                this.Name = attribute.Name;
                this.Description = attribute.Description;
                this.TestBaseType = attribute.TestBaseType;
            }
            else
            {
                throw new InvalidOperationException("Type is missing a TestDetectorBaseTags attribute.");
            }
        }

        /// <summary>
        /// Gets the vulnerabilities.
        /// </summary>
        public ConcurrentQueue<Vulnerability> Vulnerabilities { get; }

        /// <summary>
        /// Gets the type of the test base.
        /// </summary>
        /// <value>
        /// The type of the test base.
        /// </value>
        internal TestBaseType TestBaseType { get; private set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        protected internal IContext Context { get; private set; }

        /// <summary>
        /// Gets the test target.
        /// </summary>
        protected internal ITarget TestTarget { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        protected string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        protected string Description { get; private set; }

        /// <summary>
        /// Gets the test cases.
        /// </summary>
        protected List<TestCase> TestCases { get; }

        /// <summary>
        /// Initializer for the test plugin - the test manager will invoke this at the start of a test run.
        /// Tests that override this method must be sure to call the base.Init function, to ensure that the base class
        /// has the context, target, and results objects created and available, and if using the default DoTests function would add
        /// test case objects to the test cases collection (which the default DoTests function will use).
        /// </summary>
        /// <param name="context">The test context.</param>
        /// <param name="target">The test target.</param>
        public virtual void Init(IContext context, ITarget target)
        {
            this.Context = context;
            this.TestTarget = target;
            this.TestCases.Clear();
        }

        /// <summary>
        /// Executes the tests.
        /// </summary>
        /// <remarks>
        /// The default implementation applies the test cases list and iterates over all "injectable" parameters.
        /// Finally it inspects the response page for the match string (regex).
        /// </remarks>
        public virtual void DoTests()
        {
            try
            {
                foreach (var test in this.TestCases)
                {
                    foreach (string injectionString in test.InjectionString)
                    {
                        foreach (var param in this.TestTarget.Params)
                        {
                            // test for integer parameters
                            if (test.FuzzOnlyNumericParam && !TestBaseHelper.IsParamValueNumeric(param.Value))
                            {
                                continue;
                            }

                            string injectedParameters = this.TestTarget.GetParam(param.Key, injectionString);
                            this.SendRequest(test, injectedParameters, injectionString, param.Key);
                        }

                        if (test.FuzzAlsoAllParamsAtTheSameTime)
                        {
                            this.SendRequest(
                                test,
                                this.TestTarget.GetAllParams(injectionString),
                                injectionString, 
                                Constants.AllParams);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex);
            }
        }

        /// <summary>
        /// The inspect response.
        /// </summary>
        /// <param name="requestTarget">
        /// The request target.
        /// </param>
        /// <param name="responseTarget">
        /// The response target.
        /// </param>
        /// <param name="response">
        /// The current response.
        /// </param>
        /// <param name="plugIn">
        /// The plug in.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        /// <param name="testParameter">
        /// The tested parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested val.
        /// </param>
        public virtual void InspectResponse(
            ITarget requestTarget,
            ITarget responseTarget,
            HttpWebResponseHolder response,
            string plugIn,
            TestCase testCase,
            string testParameter,
            string testValue)
        {
        }

        /// <summary>
        /// Inside a test check to see if the match string gets found on the response page,regex used for matching
        /// if that is the case then log a vulnerabilities.
        /// </summary>
        /// <param name="webRequestContext">web request context</param>
        /// <param name="testCase">test case</param>
        /// <param name="testParameter">tested parameter</param>
        /// <param name="testValue">tested value</param>
        protected virtual void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testCase,
            string testParameter,
            string testValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;
            if (string.IsNullOrEmpty(testCase.MatchString))
            {
                return;
            }

            MatchCollection matches = Regex.Matches(
                response.ResponseContent, 
                testCase.MatchString,
                RegexOptions.IgnoreCase);

            if (matches.Count > 0)
            {
                this.Vulnerabilities.Enqueue(new Vulnerability
                {
                    Title = testCase.TestName,
                    Level = 1,
                    TestedParam = testParameter,
                    TestedVal = testValue,
                    HttpResponse = response,
                    MatchString = testCase.MatchString,
                    TestPlugin = GetType().Name
                });
            }
        }

        /// <summary>
        /// Send web request.
        /// </summary>
        /// <param name="test">test case</param>
        /// <param name="injectedparameters">injected parameters</param>
        /// <param name="injectParamValue">injected parameter value</param>
        /// <param name="paramKey">parameter key</param>
        protected virtual void SendRequest(
            TestCase test,
            string injectedparameters,
            string injectParamValue,
            string paramKey)
        {
            var cookies = this.GetRequestCookieCollection(test);
            var url = this.TestTarget.UrlBase + "?" + injectedparameters;

            var webRequestContext = this.Context.SendRequest(new ContextSendRequestParameter
            {
                Url = url,
                Cookies = cookies,
                PluginName = GetType().Name,
                ParameterNameFuzzed = paramKey,
                CustomContentType = test.ContentType,
                BrowserType = BrowserType.Chrome
            });

            this.EndWebRequestEvent(
                webRequestContext,
                test,
                paramKey,
                injectParamValue);
        }

        /// <summary>
        /// Add vulnerability.
        /// </summary>
        /// <param name="title">vulnerability title</param>
        /// <param name="testParameter">tested parameter</param>
        /// <param name="testValue">tested value</param>
        /// <param name="evidence">the evidence</param>
        /// <param name="response">web response ( mandatory )</param>
        /// <param name="testCase">test case</param>
        /// <param name="vulnerabilityLevel">vulnerability Level</param>
        protected void AddVulnerability(
            string title,
            string testParameter,
            string testValue,
            string evidence,
            HttpWebResponseHolder response,
            TestCase testCase,
            VulnerabilityLevelEnum vulnerabilityLevel)
        {
            Require.NotNull(() => response);

            this.Vulnerabilities.Enqueue(new Vulnerability
            {
                Title = title,
                Level = (int)vulnerabilityLevel,
                TestedParam = testParameter,
                TestedVal = testValue,
                HttpResponse = response,
                Evidence = evidence,
                MatchString = testCase == null ? string.Empty : testCase.MatchString,
                TestPlugin = GetType().Name
            });
        }

        /// <summary>
        /// The is content type valid.
        /// </summary>
        /// <param name="test">
        /// The test.
        /// </param>
        /// <param name="contentType">
        /// The content type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool IsContentTypeValid(TestCase test, string contentType)
        {
            if (test?.FilterResponseContentType != null && contentType.IsNotNullOrEmpty())
            {
                return !test.FilterResponseContentType.Any(
                    t =>
                        t.IndexOfOi(contentType) > -1 ||
                        contentType.IndexOfOi(t) > -1);
            }

            return false;
        }

        /// <summary>
        /// The get request cookie collection.
        /// </summary>
        /// <param name="test">
        /// The test.
        /// </param>
        /// <returns>
        /// The <see cref="CookieCollection"/>.
        /// </returns>
        private CookieCollection GetRequestCookieCollection(TestCase test)
        {
            var isCookiePresent = this.Context.CurrentCookies != null && this.TestTarget.Uri != null &&
                                  this.Context.CurrentCookies.ContainsKey(this.TestTarget.Uri.Host);

            // if we fuzz cookies then we return, don't save fuzzed cookies
            if (test.InjectionCookieCallBack != null && isCookiePresent)
            {
                return test.InjectionCookieCallBack(
                    this.Context.CurrentCookies[this.TestTarget.Uri.Host]);
            }

            return isCookiePresent ? this.Context.CurrentCookies[this.TestTarget.Uri.Host] : new CookieCollection();
        }

        /// <summary>
        /// The end web request event.
        /// </summary>
        /// <param name="webRequestContext">
        /// The web request context.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        /// <param name="parameterKey">
        /// The parameter key.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        private void EndWebRequestEvent(
            WebRequestContext webRequestContext,
            TestCase testCase,
            string parameterKey,
            string parameterValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;

            // valid response type?
            if (string.IsNullOrEmpty(response.ResponseContent)
                || IsContentTypeValid(testCase, response.ResponseContentType))
            {
                BrowserHelper.ReleaseBrowser(webRequestContext);
                return;
            }

            // run detectors
            this.Context.RunDetectors(
                response,
                this.TestTarget,
                this.GetType().Name,
                testCase,
                parameterKey,
                parameterValue);

            // execute client callback
            this.ExecuteCheckForVulnerabilities(
                webRequestContext,
                testCase,
                parameterKey,
                parameterValue);
        }

        /// <summary>
        /// The execute check for vulnerabilities.
        /// </summary>
        /// <param name="webRequestContext">
        /// The web request context.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        /// <param name="parameterKey">
        /// The parameter key.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        private void ExecuteCheckForVulnerabilities(
            WebRequestContext webRequestContext, 
            TestCase testCase, 
            string parameterKey,
            string parameterValue)
        {
            try
            {
                // as long as we have a response, check to see if there was a vuln/match
                this.CheckForVulnerabilities(webRequestContext, testCase, parameterKey, parameterValue);
            }
            finally
            {
                // We are done with the browser, give it back.
                BrowserHelper.ReleaseBrowser(webRequestContext);
            }
        }
    }
}