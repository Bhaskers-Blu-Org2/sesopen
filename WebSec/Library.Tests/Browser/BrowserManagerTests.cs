//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Browser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Library.Fiddler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using WebSec.Common;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) a browser manager tests.
    /// </summary>
    [TestClass]
    [DeploymentItem("chromedriver.exe", "")]
    public class BrowserManagerTests
    {
        /// <summary>
        /// The chrome only.
        /// </summary>
        private readonly IDictionary<BrowserType, int> chromeOnly = new Dictionary<BrowserType, int>
        {
            { BrowserType.Chrome, 1 }
        };

        /// <summary>
        /// Ensures the Windows Firewall will accept the connections needed to run integration tests.
        /// </summary>
        public static void CreateFirewallRules()
        {
            RunNetShCommands(new[]
            {
                @"advfirewall firewall delete rule name=""WebSec - IEDriver - Test"" ",
                @"advfirewall firewall delete rule name=""WebSec - ChromeDriver - Test"" ",
                @"advfirewall firewall delete rule name=""WebSec - Fiddler Proxy - Test"" ",
                @"advfirewall firewall add rule name=""WebSec - IEDriver - Test"" program=""{0}\IEDriverServer.exe"" dir=in action=allow profile=domain"
                    .FormatIc(AppDomain.CurrentDomain.BaseDirectory),
                @"advfirewall firewall add rule name=""WebSec - ChromeDriver - Test"" program=""{0}\chromedriver.exe"" dir=in action=allow profile=domain"
                    .FormatIc(AppDomain.CurrentDomain.BaseDirectory),
                @"advfirewall firewall add rule name=""WebSec - Fiddler Proxy - Test"" program=""{0}"" dir=in action=allow profile=domain"
                    .FormatIc(Process.GetCurrentProcess().MainModule.FileName)
            });
        }

        /// <summary>
        /// Browser manager tests initialize.
        /// </summary>
        /// <param name="testContext">
        ///     Context for the test.
        /// </param>
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            CreateFirewallRules();
        }

        /// <summary>
        /// Class cleanup.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            FiddlerProxy.Cleanup(Constants.FiddlerPort);
        }

        /// <summary>
        /// (Unit Test Method) browser waits for a page with a slow load time chrome.
        /// </summary>
        [TestMethod]
        public void BrowserWaitsForAPageWithASlowLoadTimeChrome()
        {
            this.BrowserWaitsForAPageWithASlowLoadTimeImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// (Unit Test Method) browser does not wait for ajax requests to complete chrome.
        /// </summary>
        [TestMethod]
        public void BrowserDoesNotWaitForAjaxRequestsToCompleteChrome()
        {
            this.BrowserDoesNotWaitForAjaxRequestsToCompleteImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// (Unit Test Method) browser reports the correct process identifiers chrome.
        /// </summary>
        [TestMethod]
        public void BrowserReportsTheCorrectProcessIdentifiersChrome()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            using (var manager = new BrowserManager(this.chromeOnly))
            {
                var browser = manager.AcquireBrowser(BrowserType.Chrome);

                var browserProcesses =
                    Process.GetProcessesByName("chrome").Select(p => p.Id);
                var driverprocess =
                    Process.GetProcessesByName("chromedriver").First();

                // Chrome opens separate processes for the rendering engine, plugins and each tab.
                browserProcesses.ShouldContain(browser.ProcessId);
                browser.ParentProcessId.ShouldEqual(driverprocess.Id);
            }
        }

        /// <summary>
        /// (Unit Test Method) browser waits for XML document chrome.
        /// </summary>
        [TestMethod]
        public void BrowserWaitsForXmlDocumentChrome()
        {
            this.BrowserWaitsForXmlDocumentImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// (Unit Test Method) browser can add and remove cookies IE.
        /// </summary>
        [TestMethod]
        public void BrowserCanAddAndRemoveCookiesChrome()
        {
            this.BrowserCanAddAndRemoveCookiesImplementation();
        }

        /// <summary>
        /// (Unit Test Method) browser does not download files IE.
        /// </summary>
        [TestMethod]
        public void BrowserDoesNotDownloadFilesChrome()
        {
            this.BrowserDoesNotDownloadFilesImplementation();
        }

        /// <summary>
        /// (Unit Test Method) browser handles missing page chrome.
        /// </summary>
        [TestMethod]
        public void BrowserHandlesMissingPageChrome()
        {
            this.BrowserHandlesMissingPageImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// (Unit Test Method) browser proxy injects mandatory headers chrome.
        /// </summary>
        [TestMethod]
        public void BrowserProxyInjectsMandatoryHeadersChrome()
        {
            this.BrowserProxyInjectsMandatoryHeadersImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// (Unit Test Method) browser proxy injects per request headers chrome.
        /// </summary>
        [TestMethod]
        public void BrowserProxyInjectsPerRequestHeadersChrome()
        {
            this.BrowserProxyInjectsPerRequestHeadersImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// (Unit Test Method) browser DOM access chrome.
        /// </summary>
        [TestMethod]
        public void BrowserFindElementChrome()
        {
            this.BrowserFindElementImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// Executes the net shell commands operation.
        /// </summary>
        /// <param name="arguments">
        ///     The arguments.
        /// </param>
        private static void RunNetShCommands(string[] arguments)
        {
            foreach (var argument in arguments)
            {
                var info = new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = argument,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var p = Process.Start(info);
                p.WaitForExit();
            }
        }

        /// <summary>
        /// The browser_waits_for_a_page_with_a_slow_load_time_ implementation.
        /// </summary>
        /// <param name="browserType">
        /// The browser type.
        /// </param>
        private void BrowserWaitsForAPageWithASlowLoadTimeImplementation(BrowserType browserType)
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };

            // the request will take 5 seconds to complete
            const int Delay = 5;
            using (var manager = new BrowserManager(browserInstances))
            {
                var browser = manager.AcquireBrowser(browserType);

                // make the request, wait for up to 10 seconds.
                browser.NavigateTo($"{Constants.VulnerabilitiesAddress}delay.html?a=" + Delay);
                browser.WaitForPageLoad(Constants.BrowserWaitForPageLoadInMilliseconds);

                // Then the page complete response should be present in the page
                browser.PageSource.ShouldContain("Done!");
            }
        }

        /// <summary>
        /// The browser_does_not_wait_for_ajax_requests_to_complete_ implementation.
        /// </summary>
        /// <param name="browserType">
        /// The browser type.
        /// </param>
        private void BrowserDoesNotWaitForAjaxRequestsToCompleteImplementation(BrowserType browserType)
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };

            // the request will take 5 seconds to complete
            const int Delay = 5;
            using (var manager = new BrowserManager(browserInstances))
            {
                var browser = manager.AcquireBrowser(browserType);

                // make the request, wait for up to 3 seconds.
                browser.NavigateTo($"{Constants.VulnerabilitiesAddress}delayajax.html?a=" + Delay);
                browser.WaitForPageLoad(3);

                // Then the AJAX response should not be present in the page
                browser.PageSource.ShouldNotContain("Done!");
            }
        }

        /// <summary>
        /// Browser waits for XML document implementation.
        /// </summary>
        /// <param name="browserType">
        /// Browser type.
        /// </param>
        private void BrowserWaitsForXmlDocumentImplementation(BrowserType browserType)
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };

            const int MaxWait = 10;
            using (var manager = new BrowserManager(browserInstances))
            {
                var browser = manager.AcquireBrowser(browserType);
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                // make the request, wait for up to 10 seconds.
                browser.NavigateTo($"{Constants.VulnerabilitiesAddress}XmlResponse.aspx");
                browser.WaitForPageLoad(MaxWait);

                stopWatch.Stop();

                // FF and Chrome will return the XML content, IE returns the XML with styles applied.
                browser.PageSource.ShouldContain("content");

                stopWatch.ElapsedMilliseconds.ShouldBeLessThan((MaxWait + 1) * 1000);
            }
        }

        /// <summary>
        /// Browser can add and remove cookies implementation.
        /// </summary>
        private void BrowserCanAddAndRemoveCookiesImplementation()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };
            var expiry = DateTime.Now.AddDays(1);

            using (var manager = new BrowserManager(browserInstances))
            {
                var browser = manager.AcquireBrowser(BrowserType.Chrome);

                // a new browser should have no cookies
                browser.AllCookies.Count.ShouldEqual(0);

                // adding a cookie without first loading the page should have no effect
                browser.AddCookie("TEST", "Hello", ".bing.com", "/", expiry);
                browser.AllCookies.Count.ShouldEqual(0);

                // navigating to a page should return some cookies
                // NOTE - this uses bing.com since adding cookies to localhost is problematic.
                browser.NavigateTo("http://bing.com");
                browser.WaitForPageLoad(2000);
                browser.AllCookies.Count.ShouldBeGreaterThan(0);

                // we should be able to add and retrieve a new cookie
                browser.AddCookie("TEST", "Hello", ".bing.com", "/", expiry);
                var cookies = browser.AllCookies;
                var cookie = cookies["TEST"];
                cookie.ShouldNotBeNull();
                cookie.Value.ShouldEqual("Hello");
                cookie.Path.ShouldEqual("/");
                cookie.Domain.ShouldEqual(".bing.com");

                // we should be able to remove all cookies
                browser.DeleteAllCookies();
                browser.AllCookies.Count.ShouldEqual(0);
            }
        }

        /// <summary>
        /// Browser does not download files implementation.
        /// </summary>
        private void BrowserDoesNotDownloadFilesImplementation()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            using (var manager = new BrowserManager(browserInstances))
            {
                const int MaxWait = 10;
                var browser = manager.AcquireBrowser(BrowserType.Chrome);

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                browser.NavigateTo($"{Constants.VulnerabilitiesAddress}file.js");
                browser.WaitForPageLoad(MaxWait);

                stopWatch.Stop();

                stopWatch.ElapsedMilliseconds.ShouldBeLessThan((MaxWait + 1) * 1000);
            }
        }

        /// <summary>
        /// Browser handles missing page implementation.
        /// </summary>
        /// <param name="browserType">
        /// Browser type.
        /// </param>
        private void BrowserHandlesMissingPageImplementation(BrowserType browserType)
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };

            const int MaxWait = 10;
            using (var manager = new BrowserManager(browserInstances))
            {
                var browser = manager.AcquireBrowser(browserType);
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                // make the request, wait for up to 10 seconds.
                browser.NavigateTo(
                    "http://ieonline.microsoft.com/pinnedconfig?action=1&CurrentPage=1&itemId=1&nextQuestionId=1&nextQuestionUserId=1");
                browser.WaitForPageLoad(MaxWait);

                stopWatch.Stop();

                // expect a browser specific 404 message
                browser.PageSource.ShouldContain("Page not found");

                stopWatch.ElapsedMilliseconds.ShouldBeLessThan((MaxWait * 1000) + 1500);
            }
        }

        /// <summary>
        /// Browser proxy injects mandatory headers implementation.
        /// </summary>
        /// <param name="browserType">
        /// Browser type.
        /// </param>
        private void BrowserProxyInjectsMandatoryHeadersImplementation(BrowserType browserType)
        {
            /*
             * WARNING: Terminating this test while the proxy is running can lead to adverse side effects
             *          such as loss of web connectivity. Please allow the call to FiddlerProxy.Cleanup
             *          to complete.
             */

            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };

            try
            {
                FiddlerProxy.Initialize(new[] { "TestKey:TestValue" }, Constants.FiddlerPort);

                using (var manager = new BrowserManager(browserInstances))
                {
                    var browser = manager.AcquireBrowser(browserType);

                    browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                    browser.WaitForPageLoad(10);

                    // Then the expected request header should be present in the page
                    browser.PageSource.ShouldContain("TestKey : TestValue");
                }

                // turn off the proxy and make sure the header is not injected
                FiddlerProxy.Initialize(new string[0], 0);

                using (var manager = new BrowserManager(browserInstances))
                {
                    var browser = manager.AcquireBrowser(browserType);

                    browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                    browser.WaitForPageLoad(10);

                    // Then the expected request header should be present in the page
                    browser.PageSource.ShouldNotContain("TestKey : TestValue");
                }
            }
            finally
            {
                FiddlerProxy.Cleanup(Constants.FiddlerPort);
                FiddlerProxy.Cleanup(0);
            }
        }

        /// <summary>
        /// Browser proxy injects per request headers implementation.
        /// </summary>
        /// <param name="browserType">
        /// Browser type.
        /// </param>
        private void BrowserProxyInjectsPerRequestHeadersImplementation(BrowserType browserType)
        {
            /*
             * WARNING: Terminating this test while the proxy is running can lead to adverse side effects
             *          such as loss of web connectivity. Please allow the call to FiddlerProxy.Cleanup
             *          to complete.
             */

            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };

            try
            {
                FiddlerProxy.Initialize(new string[0], Constants.FiddlerPort);

                using (var manager = new BrowserManager(browserInstances))
                {
                    var browser = manager.AcquireBrowser(browserType);

                    var browserId = browser.ProcessId;

                    // expected headers for this browser process
                    FiddlerProxy.RegisterInstanceHeaders(
                        browser.ProcessId, 
                        "My User Agent 1.0",
                        "TestKey: Process" + browserId);

                    browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                    browser.WaitForPageLoad(10);

                    // Then the expected request header should be present in the page
                    browser.PageSource.ShouldContain("User-Agent : My User Agent 1.0");
                    browser.PageSource.ShouldContain("TestKey : Process" + browserId);

                    // releasing the browser should forget the headers
                    manager.ReleaseBrowser(browser);
                    browser = manager.AcquireBrowser(browserType);
                    browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                    browser.WaitForPageLoad(10);

                    // Then the expected request header should be present in the page
                    browser.PageSource.ShouldNotContain("User-Agent : My User Agent 1.0");
                    browser.PageSource.ShouldNotContain("TestKey : Process" + browserId);
                }

                // turn off the proxy and make sure the header is not injected
                FiddlerProxy.Initialize(new string[0], 0);

                using (var manager = new BrowserManager(browserInstances))
                {
                    var browser = manager.AcquireBrowser(browserType);

                    browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                    browser.WaitForPageLoad(10);

                    // Then the expected request headers should not be present in the page
                    browser.PageSource.ShouldNotContain("TestKey:");
                    browser.PageSource.ShouldNotContain("User-Agent : My User Agent 1.0");
                }
            }
            finally
            {
                FiddlerProxy.Cleanup(Constants.FiddlerPort);
                FiddlerProxy.Cleanup(0);
            }
        }

        /// <summary>
        /// Browser DOM access test implementation.
        /// </summary>
        /// <param name="browserType">The type of the browser being tested.</param>
        private void BrowserFindElementImplementation(BrowserType browserType)
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 1 } };
            using (var manager = new BrowserManager(browserInstances))
            {
                var browser = manager.AcquireBrowser(browserType);
                browser.NavigateTo($"{Constants.VulnerabilitiesAddress}SimplePageWithForm.html");
                browser.WaitForPageLoad(10);
                var element = browser.FindWebElement(By.Id("textInputId"));
                element.ShouldNotBeNull();
                element.TagName.Equals("input").ShouldBeTrue();
                element.SendKeys("Hello");
                System.Threading.Thread.Sleep(1000);
                element.SendKeys(" world");
                element.GetAttribute("value").Equals("Hello world").ShouldBeTrue();

                element = browser.FindWebElement(By.Name("textInputName"));
                element.GetAttribute("value").Equals("Hello world").ShouldBeTrue();
            }
        }
    }
}