//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Browser;
    using Engine;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Payloads;
    using WebSec.Common;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) an engine context tests.
    /// </summary>
    [TestClass]
    public class EngineContextTests
    {
        /// <summary>
        /// 404 target.
        /// </summary>
        private readonly string nonExistingTargetUrl = $"{Constants.VulnerabilitiesAddress}not";

        /// <summary>
        /// Test target.
        /// </summary>
        private readonly string targetUrl = $"{Constants.VulnerabilitiesAddress}SimplePageWithForm.html";

        /// <summary>
        /// My class initialize.
        /// </summary>
        /// <param name="testContext">
        /// Context for the test.
        /// </param>
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // configure the firewall for browser control since these tests use live browsers.
            BrowserManagerTests.CreateFirewallRules();
            ObjectResolver.RegisterInstance<IPayloads>(new Payloads(Library.Constants.PayloadsQuickDataFolder));
        }

        /// <summary>
        /// (Unit Test Method) tests send request using browser web request.
        /// </summary>
        [TestMethod]
        public void TestSendRequestUsingBrowserWebRequest()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);

                var context = new Context();
                WebRequestContext webRequestContext =
                    context.SendRequest(new ContextSendRequestParameter
                    {
                        Url = targetUrl,
                        BrowserType = BrowserType.Chrome
                    });

                // check that we got the proper response back
                webRequestContext.ResponseHolder.ShouldNotBeNull();
                webRequestContext.ResponseHolder.ResponseContent.ShouldNotBeNullOrEmpty();
                webRequestContext.ResponseHolder.RequestUserAgent.ShouldContain(BrowserType.Chrome.ToString());
            }
        }

        /// <summary>
        /// (Unit Test Method) tests send request using browser web request handling HTTP exception.
        /// </summary>
        [TestMethod]
        public void TestSendRequestUsingBrowserWebRequestHandlingHttpException()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);
                var context = new Context();
                WebRequestContext webRequestContext =
                    context.SendRequest(new ContextSendRequestParameter
                    {
                        Url = nonExistingTargetUrl,
                        BrowserType = BrowserType.Chrome
                    });

                // check that we got the proper response back
                webRequestContext.ResponseHolder.ShouldNotBeNull();
                webRequestContext.ResponseHolder.ResponseContent.ShouldNotBeEmpty();
                webRequestContext.ResponseHolder.RequestUserAgent.ShouldEqual(BrowserType.Chrome.ToString());
                webRequestContext.ResponseHolder.StatusCode.ShouldEqual(HttpStatusCode.OK);
                webRequestContext.ResponseHolder.BrowserPageTitle.IndexOfOi("404").ShouldBeGreaterThan(-1);
            }
        }
    }
}