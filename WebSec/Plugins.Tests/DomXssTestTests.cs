//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Common;
    using Common.TestInfrastructure;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Library.Engine;
    using Library.Payloads;
    using Library.PluginBase;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Plugins;

    /// <summary>
    /// Unit and integration tests for <see cref="DomXssTestTests"/> type.
    /// </summary>
    [TestClass]
    public class DomXssTestTests
    {
        /// <summary>
        /// Browser manager tests initialize.
        /// </summary>
        /// <param name="testContext">
        ///     Context for the test.
        /// </param>
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            ObjectResolver.RegisterInstance<IPayloads>(new Payloads(WebSec.Library.Constants.PayloadsQuickDataFolder));
        }

        /// <summary>
        /// Class cleanup.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestCleanup.Cleanup();
        }

        /// <summary>
        /// Creates mock browser.
        /// </summary>
        /// <param name="id">
        /// The identifier.
        /// </param>
        /// <param name="parentId">
        /// Identifier for the parent.
        /// </param>
        /// <param name="browserType">
        /// The browser type.
        /// </param>
        /// <returns>
        /// The new mock browser.
        /// </returns>
        public static Mock<BrowserAbstract> CreateMockBrowser(int id = 0, int parentId = 0, BrowserType browserType = BrowserType.Chrome)
        {
            var mock = new Mock<BrowserAbstract>();
            mock.SetupGet(b => b.ProcessId).Returns(id);
            mock.SetupGet(b => b.ParentProcessId).Returns(parentId);
            mock.SetupGet(b => b.BrowserType).Returns(browserType);
            mock.Setup(b => b.DeleteAllCookies());
            mock.Setup(b => b.WaitForPageLoad(It.IsAny<int>())).Returns(false);
            mock.SetupGet(m => m.Url).Returns("http://expectedurl");

            return mock;
        }

        /// <summary>
        /// This is an integration test that verifies an end-to-end DOM XSS detection scenario.
        /// </summary>
        /// <remarks>
        /// As such, this test requires that the following be present:
        ///   A local instance of IIS hosting the WebSecProbeSite project
        ///   A local install of Chrome.
        /// </remarks>
        [TestMethod]
        public void DomXssTestIntegrationTestChrome()
        {
            this.DomXssTestIntegrationTest(BrowserType.Chrome);
        }

        /// <summary>
        /// The dom xss test_negative_result.
        /// </summary>
        [TestMethod]
        public void DomXssTestNegativeResult()
        {
            const string TestUrl = @"http://www.bing.com/search?q=seattle";

            var browserMock = CreateMockBrowser();
            string alertText;
            browserMock
                .Setup(m => m.DismissedIfAlertDisplayed(out alertText))
                .Returns(false);

            var browserManagerMock = this.CreateBrowserManager(new[] { BrowserType.Chrome });

            browserManagerMock
                .Setup(m => m.AcquireBrowser(It.Is<BrowserType>(b => b == BrowserType.Chrome)))
                .Returns(browserMock.Object);

            ObjectResolver.RegisterInstance(browserManagerMock.Object);

            var target = Target.Create(TestUrl);
            var context = new Context();

            var instance = new TestableDomXssTest();
            instance.Init(context, target);

            // introduce our known injection strings
            instance.InjectTestCaseStrings(new[] { "alert()", "alert2()" });

            // run the test
            instance.DoTests();

            // no vulns found
            instance.Vulnerabilities.ShouldNotBeNull();
            instance.Vulnerabilities.Count.ShouldEqual(0);

            // a browser was borrowed from the pool for request
            browserManagerMock.Verify(m => m.AcquireBrowser(It.Is<BrowserType>(b => b == BrowserType.Chrome)), Times.Exactly(2));

            // the browser requested the expected url combinations
            browserMock.Verify(m => m.NavigateTo(It.IsAny<string>()), Times.Exactly(2));
            browserMock.Verify(m => m.NavigateTo(It.Is<string>(u => u == @"http://www.bing.com/search?q=alert()")), Times.Once());
            browserMock.Verify(m => m.NavigateTo(It.Is<string>(u => u == @"http://www.bing.com/search?q=alert2()")), Times.Once());

            // each browser was returned to the pool
            browserManagerMock.Verify(m => m.ReleaseBrowser(It.Is<BrowserAbstract>(b => b == browserMock.Object)), Times.Exactly(2));
        }

        /// <summary>
        /// The dom xss test_positive_result.
        /// </summary>
        [TestMethod]
        public void DomXssTestPositiveResult()
        {
            const string TestUrl = @"http://www.bing.com/search?q=seattle";

            var propertyBag = new ConcurrentDictionary<int, bool>();

            var browserMock = CreateMockBrowser(browserType: BrowserType.Chrome);

            browserMock
                .Setup(m => m.NavigateTo(It.IsAny<string>()))
                .Callback<string>(s => propertyBag[Thread.CurrentThread.ManagedThreadId] = s.Contains("alert(456)"));

            string alertText;
            browserMock
                .Setup(m => m.DismissedIfAlertDisplayed(out alertText))
                .Returns(() => propertyBag.ContainsKey(Thread.CurrentThread.ManagedThreadId) && propertyBag[Thread.CurrentThread.ManagedThreadId]);

            browserMock
                .SetupGet(m => m.PageSource)
                .Returns("pagesource");

            browserMock
                .SetupGet(m => m.Url)
                .Returns("http://foo");

            browserMock
                .SetupGet(m => m.AlertMessageDisplayed)
                .Returns(new HashSet<string> { TestBaseHelper.AttackSignature });

            var factoryMock = new Mock<IBrowserFactory>();
            factoryMock
                .Setup(m => m.Create(It.IsAny<BrowserType>()))
                .Returns(browserMock.Object);

            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserManagerMock = this.CreateBrowserManager(new[] { BrowserType.Chrome });
            browserManagerMock
                .Setup(m => m.AcquireBrowser(It.Is<BrowserType>(b => b == BrowserType.Chrome)))
                .Returns(browserMock.Object);

            ObjectResolver.RegisterInstance(browserManagerMock.Object);
            
            var target = Target.Create(TestUrl);

            var context = new Context();
            var instance = new TestableDomXssTest();
            instance.Init(context, target);

            // introduce our known injection strings
            instance.InjectTestCaseStrings(new[] { "blah", "alert(" + TestBaseHelper.AttackSignature + ")", "foo" });

            // run the test
            instance.DoTests();
            
            // the scan should have run on multiple threads.
            propertyBag.Count.ShouldBeGreaterThanOrEqualTo(1);
            
            // a vuln should be found
            instance.Vulnerabilities.ShouldNotBeNull();
            instance.Vulnerabilities.Count.ShouldEqual(3);

            // the vuln record should be well formed
            var vuln = instance.Vulnerabilities.First(t => t.TestedVal == "blah");
            vuln.Level.ShouldEqual(0);
            vuln.TestedParam.ShouldEqual("q");
            vuln.TestedVal.ShouldEqual("blah");
            vuln.Evidence.ShouldEqual("Found by Chrome");
            vuln.TestPlugin.ShouldEqual("TestableDomXssTest");
            vuln.Title.ShouldEqual("DOM XSS - Script injection");

            // the http details should be consistent for browser generated request
            vuln.HttpResponse.ShouldNotBeNull();
            vuln.HttpResponse.Headers.Count.ShouldEqual(0);
            vuln.HttpResponse.RequestHeaders.ShouldEqual(string.Empty);
            vuln.HttpResponse.RequestAbsolutUri.ShouldEqual("http://www.bing.com/search?q=blah");
            vuln.HttpResponse.RequestHost.ShouldEqual("www.bing.com");
            vuln.HttpResponse.ResponseContent.ShouldEqual(browserMock.Object.PageSource);
            vuln.HttpResponse.ResponseUri.ShouldNotBeNull();
            vuln.HttpResponse.ResponseUri.ToString().ShouldEqual("http://foo/");
            vuln.HttpResponse.StatusCode.ShouldEqual(HttpStatusCode.OK);

            vuln.HttpResponse.HttpError.ShouldEqual(string.Empty);

            // the browser requested all of the expected urls, even after a vuln was found
            browserMock.Verify(m => m.NavigateTo(It.Is<string>(u => u == @"http://www.bing.com/search?q=blah")), Times.Once());
            browserMock.Verify(m => m.NavigateTo(It.Is<string>(u => u == @"http://www.bing.com/search?q=alert(" + TestBaseHelper.AttackSignature + ")")), Times.Once());
            browserMock.Verify(m => m.NavigateTo(It.Is<string>(u => u == @"http://www.bing.com/search?q=foo")), Times.Once());
        }

        /// <summary>
        /// The dom xss test_false_positive_result.
        /// </summary>
        [TestMethod]
        public void DomXssTestFalsePositiveResult()
        {
            const string TestUrl = @"http://www.bing.com/search?q=seattle";

            var propertyBag = new ConcurrentDictionary<int, bool>();

            var browserMock = CreateMockBrowser();

            browserMock
                .Setup(m => m.NavigateTo(It.IsAny<string>()))
                .Callback<string>(s => propertyBag[Thread.CurrentThread.ManagedThreadId] = s.Contains("alert()"));

            // some pages pop their own dialog boxes, we should ignore these
            string alertText = "** Expected message from the page **";
            browserMock
                .Setup(m => m.DismissedIfAlertDisplayed(out alertText))
                .Returns(() => propertyBag.ContainsKey(Thread.CurrentThread.ManagedThreadId) && propertyBag[Thread.CurrentThread.ManagedThreadId]);

            browserMock
                .SetupGet(m => m.PageSource)
                .Returns("pagesource");

            browserMock
                .SetupGet(m => m.Url)
                .Returns("http://foo");

            var factoryMock = new Mock<IBrowserFactory>();
            factoryMock
                .Setup(m => m.Create(It.IsAny<BrowserType>()))
                .Returns(browserMock.Object);

            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserManagerMock = this.CreateBrowserManager(new[] { BrowserType.Chrome });
            browserManagerMock
                .Setup(m => m.AcquireBrowser(It.Is<BrowserType>(b => b == BrowserType.Chrome)))
                .Returns(browserMock.Object);

            ObjectResolver.RegisterInstance(browserManagerMock.Object);

            var target = Target.Create(TestUrl);
            var context = new Context();
            var instance = new TestableDomXssTest();
            
            instance.Init(context, target);

            // introduce our known injection strings
            instance.InjectTestCaseStrings(new[] { "blah", "alert()", "foo" });

            // run the test
            instance.DoTests();

            // a vuln should be found
            instance.Vulnerabilities.ShouldNotBeNull();
            instance.Vulnerabilities.Count.ShouldEqual(0);            
        }

        /// <summary>
        /// The dom xss test_passes_and_receives_cookies_correctly.
        /// </summary>
        [TestMethod]
        public void DomXssTestPassesAndReceivesCookiesCorrectly()
        {
            const string TestUrl = @"http://www.bing.com/search?q=seattle";
            var url = new Uri(TestUrl);

            // create our own cookies
            var expectedCookie1 = this.CreateTestCookie(1);
            var expectedCookie2 = this.CreateTestCookie(2);
            var expectedCookie3 = this.CreateTestCookie(3);
            var expectedCookie4 = this.CreateTestCookie(4);
            var expectedCookie5 = this.CreateTestCookie(5);

            var requestCollection = new CookieCollection { expectedCookie1, expectedCookie2, expectedCookie3 };
            var responseCollection = new CookieCollection { expectedCookie4, expectedCookie5 };

            var browserMock = CreateMockBrowser();
            string alertText;
            browserMock
                .Setup(m => m.DismissedIfAlertDisplayed(out alertText))
                .Returns(true);

            browserMock
                .SetupGet(m => m.AllCookies)
                .Returns(responseCollection);

            browserMock
                .SetupGet(m => m.PageSource)
                .Returns("pagesource");
            
            browserMock
                .SetupGet(m => m.AlertMessageDisplayed)
                .Returns(new HashSet<string> { TestBaseHelper.AttackSignature });

            var browserManagerMock = this.CreateBrowserManager(new[] { BrowserType.Chrome });
            browserManagerMock
                .Setup(m => m.AcquireBrowser(It.Is<BrowserType>(b => b == BrowserType.Chrome)))
                .Returns(browserMock.Object);
            
            ObjectResolver.RegisterInstance(browserManagerMock.Object);

            var target = Target.Create(TestUrl);

            var context = new Context();
            context.CurrentCookies[url.Host] = requestCollection;

            var instance = new TestableDomXssTest();
            instance.Init(context, target);

            // introduce our known injection strings
            instance.InjectTestCaseStrings(new[] { "alert(" + TestBaseHelper.AttackSignature + ")" });

            // run the test
            instance.DoTests();
            var vuln = instance.Vulnerabilities.First();

            // the response cookies should be pulled back from the browser
            vuln.HttpResponse.Cookies.Count.ShouldEqual(responseCollection.Count);
            vuln.HttpResponse.Cookies[0].ShouldBeTheSameAs(expectedCookie4);
            vuln.HttpResponse.Cookies[1].ShouldBeTheSameAs(expectedCookie5);

            // The expected cookies were given to the browser
            this.CookieWasAddedToBrowser(browserMock, expectedCookie1);
            this.CookieWasAddedToBrowser(browserMock, expectedCookie2);
            this.CookieWasAddedToBrowser(browserMock, expectedCookie3);
        }

        /// <summary>
        /// The dom xss test_double_positive_result.
        /// </summary>
        [TestMethod]
        public void DomXssTestDoublePositiveResult()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var browserManager = new BrowserManager(browserInstances))
            {
                ObjectResolver.RegisterInstance<IBrowserManager>(browserManager);

                var target = Target.Create($"{Constants.VulnerabilitiesAddress}ShowAlert2.aspx?q=xyz");

                var context = new Context();
                var instance = new TestableDomXssTest();
                instance.Init(context, target);

                // introduce our known injection strings
                instance.InjectTestCaseStrings(new[] { "blah" });

                // run the test
                instance.DoTests();

                // a vuln should be found
                instance.Vulnerabilities.ShouldNotBeNull();
                instance.Vulnerabilities.Count.ShouldEqual(1);

                // the vuln record should be well formed
                var vuln = instance.Vulnerabilities.First();
                vuln.Level.ShouldEqual(0);
                vuln.TestedParam.ShouldEqual("q");
                vuln.TestedVal.ShouldEqual("blah");
                vuln.Evidence.ShouldEqual("Found by Chrome");
                vuln.TestPlugin.ShouldEqual("TestableDomXssTest");
                vuln.Title.ShouldEqual("DOM XSS - Script injection");

                // the http details should be consistent for browser generated request
                vuln.HttpResponse.ShouldNotBeNull();
                vuln.HttpResponse.Headers.Count.ShouldEqual(0);
                vuln.HttpResponse.RequestHeaders.ShouldEqual(string.Empty);
                vuln.HttpResponse.RequestAbsolutUri.ShouldEqual($"{Constants.VulnerabilitiesAddress}ShowAlert2.aspx?q=blah");
                vuln.HttpResponse.ResponseContent.ShouldNotBeNullOrEmpty();
                vuln.HttpResponse.ResponseUri.ShouldNotBeNull();
                vuln.HttpResponse.ResponseUri.ToString().ShouldEqual($"{Constants.VulnerabilitiesAddress}ShowAlert2.aspx?q=blah");
                vuln.HttpResponse.StatusCode.ShouldEqual(HttpStatusCode.OK);

                vuln.HttpResponse.HttpError.ShouldEqual(string.Empty);
            }
        }

        /// <summary>
        /// The dom xss test_integration_test.
        /// </summary>
        /// <param name="browserType">
        /// The browser type.
        /// </param>
        private void DomXssTestIntegrationTest(BrowserType browserType)
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 2 } };

            ObjectResolver.RegisterInstance<IBrowserManager>(new BrowserManager(browserInstances));

            var target = Target.Create($"{Constants.VulnerabilitiesAddress}vuln_domxss.html?a=1");
            var context = new Context();
            var instance = new TestableDomXssTest();

            instance.Init(context, target);

            // introduce our known injection strings
            instance.InjectTestCaseStrings(new[] { "alert(" + TestBaseHelper.AttackSignature + ")", "%22+onload=%22javascript:alert(" + TestBaseHelper.AttackSignature + ")%22" });

            instance.DoTests();

            // cleanly shutdown the browser manager
            var disposable = ObjectResolver.Resolve<IBrowserManager>() as IDisposable;
            Assert.IsNotNull(disposable);
            disposable.Dispose();

            // a vuln should be found
            instance.Vulnerabilities.ShouldNotBeNull();
            instance.Vulnerabilities.Count.ShouldEqual(1);

            // the vuln record should be well formed
            var vuln = instance.Vulnerabilities.First();
            vuln.Level.ShouldEqual(0);
            vuln.TestedParam.ShouldEqual("a");
            vuln.TestedVal.ShouldEqual("alert(" + TestBaseHelper.AttackSignature + ")");
            vuln.Evidence.ShouldEqual("Found by " + browserType);
            vuln.TestPlugin.ShouldStartWith("TestableDomXssTest");
            vuln.Title.ShouldEqual("DOM XSS - Script injection");

            instance.Vulnerabilities.Count(v => v.TestPlugin == "TestableDomXssTest")
                .ShouldEqual(1);
        }

        /// <summary>
        /// The cookie was added to browser.
        /// </summary>
        /// <param name="browserMock">
        /// The browser mock.
        /// </param>
        /// <param name="cookie">
        /// The cookie.
        /// </param>
        private void CookieWasAddedToBrowser(Mock<BrowserAbstract> browserMock, Cookie cookie)
        {
            browserMock.Verify(m => m.AddCookie(
                It.Is<string>(s => s == cookie.Name),
                It.Is<string>(s => s == cookie.Value),
                It.Is<string>(s => s == cookie.Domain),
                It.Is<string>(s => s == cookie.Path),
                It.Is<DateTime>(s => s == cookie.Expires)));
        }

        /// <summary>
        /// The create test cookie.
        /// </summary>
        /// <param name="identifier">
        /// The identifier.
        /// </param>
        /// <returns>
        /// The <see cref="Cookie"/>.
        /// </returns>
        private Cookie CreateTestCookie(int identifier)
        {
            return new Cookie
            {
                Name = "cookie" + identifier,
                Value = "value" + identifier,
                Domain = "domain" + identifier,
                Path = "path" + identifier,
                Expires = new DateTime(identifier, identifier, identifier)
            };
        }

        /// <summary>
        /// The create browser manager.
        /// </summary>
        /// <param name="browsers">
        /// The browsers.
        /// </param>
        /// <returns>
        /// The <see cref="Mock"/>.
        /// </returns>
        private Mock<IBrowserManager> CreateBrowserManager(IEnumerable<BrowserType> browsers)
        {
            var browserManagerMock = new Mock<IBrowserManager>();

            browserManagerMock
                .SetupGet(m => m.AvailableBrowserTypes)
                .Returns(browsers);

            return browserManagerMock;
        }

        /// <summary>
        /// An overlay for the DOMXssTest class that exposes protected members to aid testing.
        /// </summary>
        private class TestableDomXssTest : DomXssTest
        {
            /// <summary>
            /// Hook to override the base test cases and introduce our own unit-test values.
            /// </summary>
            /// <param name="strings">
            /// The strings.
            /// </param>
            public void InjectTestCaseStrings(string[] strings)
            {
                // the test must have some test cases, this is expected to change over time and we should not care how
                this.TestCases.ShouldNotBeEmpty();

                // there should be some injection strings, again we don't care about content
                var testcase = this.TestCases.First();
                testcase.InjectionString.Count().ShouldNotEqual(0);

                // introduce our predictable injection strings
                testcase.InjectionString = strings;
                this.TestCases.Clear();
                this.TestCases.Add(testcase);
            }
        }
    }
}