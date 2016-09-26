//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Library.Browser;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Logger;
    using Library.PluginBase;
    using OpenQA.Selenium;

    /// <summary>
    /// Using user interaction tests for any XSS behavioral manifestation.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Client-side user interaction request XSS test",
        Description = "Detects user interaction XSS vulnerabilities within client side script.")]
    public sealed class UserInteractionXssTest : PluginBaseAbstract
    {
        /// <summary>
        /// The inspect post page.
        /// </summary>
        private InspectUserInteractionPage inspectPostPage;

        /// <summary>
        /// The vulnerability found.
        /// </summary>
        private bool vulnerabilityFound;

        /// <inheritdoc/>
        public override void Init(IContext context, ITarget target)
        {
            base.Init(context, target);

            TestCases.AddRange(TestBaseHelper.XssTestCases(target, context));

            this.inspectPostPage = new InspectUserInteractionPage(
                context,
                new[] { "//input[@type!='hidden' and @type!='submit']" });
        }

        /// <summary>
        /// Override the do tests function, since it has the default get request behavior.Executes the tests.
        /// </summary>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.DoTests()"/>
        public override void DoTests()
        {
            BrowserAbstract browser = null;
            var postElements = this.inspectPostPage.Inspect(this.TestTarget, ref browser).ToArray();

            // a container representing the id of the element and the element itself
            var webElementsToBeTested = new Dictionary<string, IWebElement>();

            foreach (var postElement in postElements)
            {
                string elementUniqueIdentification = GetElementIdentification(postElement);

                webElementsToBeTested[elementUniqueIdentification] = postElement;
            }

            try
            {
                // test all user interaction controls
                while (webElementsToBeTested.Count > 0)
                {
                    // if we found a vulnerability then exit, the page is already compromised
                    if (this.vulnerabilityFound)
                    {
                        break;
                    }

                    // take the first element and start testing
                    KeyValuePair<string, IWebElement> postElement = webElementsToBeTested.First();

                    // for each test case
                    foreach (var testCase in this.TestCases)
                    {
                        // if we found a vulnerability then exit, the page is already compromised
                        if (this.vulnerabilityFound)
                        {
                            break;
                        }

                        if (string.IsNullOrEmpty(postElement.Key))
                        {
                            break;
                        }

                        // for each payload in the test case
                        foreach (var payload in testCase.InjectionString)
                        {
                            // if we found a vulnerability then exit, the page is already compromised
                            if (this.vulnerabilityFound)
                            {
                                break;
                            }

                            try
                            {
                                this.SendPayload(postElement.Value, payload, browser, testCase);
                            }
                            catch (StaleElementReferenceException ex)
                            {
                                // before re-inspecting the page check for vulnerabilities
                                this.CheckForVulnerabilities(browser, testCase, payload);

                                // the web elements had become stale the refresh them, re- inspect the elements and continue testing
                                postElements = this.inspectPostPage.Inspect(this.TestTarget, ref browser).ToArray();

                                // update the new reflected elements
                                var newWebElements = this.UpdateWebElementsToBeTested(
                                    webElementsToBeTested,
                                    postElements);

                                webElementsToBeTested.Clear();
                                webElementsToBeTested.AddRange(newWebElements);

                                postElement =
                                    webElementsToBeTested.SingleOrDefault(
                                        e => e.Key.Equals(postElement.Key, StringComparison.InvariantCultureIgnoreCase));

                                if (string.IsNullOrEmpty(postElement.Key))
                                {
                                    break;
                                }

                                Logger.WriteWarning(ex);
                            }
                            catch (UnhandledAlertException ex)
                            {
                                this.CheckForVulnerabilities(browser, testCase, payload);
                                Logger.WriteError(ex);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteError(ex);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(postElement.Key))
                    {
                        // after the testing is done remove it from container
                        webElementsToBeTested.Remove(postElement.Key);
                    }
                }
            }
            finally
            {
                Context.ReleaseBrowser(browser);
            }
        }

        /// <inheritdoc/>
        protected override void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testcase,
            string testedParam,
            string testValue)
        {
            // don't do anything here, just override the default behavior
        }

        /// <summary>
        /// The get element identification.
        /// </summary>
        /// <param name="postElement">
        /// The post element.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetElementIdentification(IWebElement postElement)
        {
            var ret = new StringBuilder();

            try
            {
                ret.Append(postElement.TagName);
                ret.Append(postElement.Text);
                ret.Append(postElement.Size.Width);
                ret.Append(postElement.Size.Height);
            }
            catch (StaleElementReferenceException ex)
            {
                Logger.WriteWarning(ex);
            }

            return ret.ToString();
        }

        /// <summary>
        /// The send payload.
        /// </summary>
        /// <param name="postElement">
        /// The post element.
        /// </param>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        private void SendPayload(IWebElement postElement, string payload, BrowserAbstract browser, TestCase testCase)
        {
            if (!postElement.Displayed)
            {
                Logger.WriteWarning(
                    "Element {0} not displayed, url ={1}",
                    GetElementIdentification(postElement),
                    this.TestTarget.Uri.OriginalString);
                return;
            }

            // send payload
            postElement.SendKeys(payload);

            // send enter
            postElement.SendKeys(Keys.Return);

            // waiting for page to load
            browser.WaitForPageLoad(5000);

            // dismiss any present alert
            string alertText;
            browser.DismissedIfAlertDisplayed(out alertText);

            // log xss vulnerabilities
            this.CheckForVulnerabilities(browser, testCase, payload);
        }

        /// <summary>
        /// The update web elements to be tested.
        /// </summary>
        /// <param name="webElementsToBeTested">
        /// The web elements to be tested.
        /// </param>
        /// <param name="postElements">
        /// The post elements.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// The Returned value.
        /// </returns>
        private IEnumerable<KeyValuePair<string, IWebElement>> UpdateWebElementsToBeTested(
            IReadOnlyDictionary<string, IWebElement> webElementsToBeTested,
            IEnumerable<IWebElement> postElements)
        {
            var newWebElements = new Dictionary<string, IWebElement>();

            foreach (var postElement in postElements)
            {
                string elementUniqueIdentification = GetElementIdentification(postElement);

                // if is still in the queue to be tested then update the element
                if (webElementsToBeTested.ContainsKey(elementUniqueIdentification))
                {
                    newWebElements[elementUniqueIdentification] = postElement;
                }
            }

            return newWebElements;
        }

        /// <summary>
        /// The check for vulnerabilities.
        /// </summary>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="testCase">
        /// The testCase.
        /// </param>
        /// <param name="testValue">
        /// The tested val.
        /// </param>
        private void CheckForVulnerabilities(
            BrowserAbstract browser,
            TestCase testCase,
            string testValue)
        {
            // waiting for page to load
            browser.WaitForPageLoad(5000);

            string alertText;
            browser.DismissedIfAlertDisplayed(out alertText);

            if (browser.AlertMessageDisplayed.Contains(TestBaseHelper.AttackSignature))
            {
                // since we use the same browser instance clear the alerts after the issue has been logged
                browser.AlertMessageDisplayed.Clear();

                // we found one issue
                this.vulnerabilityFound = true;

                Vulnerabilities.Enqueue(new Vulnerability
                {
                    Title = testCase.TestName,
                    Level = (int)VulnerabilityLevelEnum.High,
                    TestedParam = string.Empty,
                    TestedVal = testValue,
                    Evidence = "Found by {0}".FormatIc(browser.BrowserType.ToString()),
                    MatchString = testCase.MatchString,
                    TestPlugin = GetType().Name
                });
            }
        }
    }
}