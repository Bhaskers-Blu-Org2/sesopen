//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Browser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Library.Fiddler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// The fiddler proxy tests.
    /// </summary>
    [TestClass]
    public class FiddlerProxyTests
    {
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
            FiddlerProxy.Cleanup(Constants.FiddlerPort);

            CreateFirewallRules();
        }

        /// <summary>
        /// (Unit Test Method) browser proxy injects per request headers multiple chrome.
        /// </summary>
        [TestMethod]
        public void BrowserProxyInjectsPerRequestHeadersMultipleChrome()
        {
            BrowserProxyInjectsPerRequestHeadersMultipleImplementation(BrowserType.Chrome);
        }

        /// <summary>
        /// The test fiddler proxy session fingerprint.
        /// </summary>
        [TestMethod]
        public void TestFiddlerProxySessionFingerprint()
        {
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            const BrowserType BrowserType = BrowserType.Chrome;

            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType, 1 } };

            try
            {
                FiddlerProxy.Initialize(new string[0], Constants.FiddlerPort);
                using (var manager = new BrowserManager(browserInstances))
                {
                    var browser = manager.AcquireBrowser(BrowserType);

                    // expected headers for this browser process
                    FiddlerProxy.RegisterInstanceHeaders(
                        browser.ProcessId,
                        "My User Agent 1.0",
                        "TestKey: Process" + browser.ProcessId);

                    browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                    browser.WaitForPageLoad(10);

                    // Then the expected request header should be present in the page
                    browser.PageSource.ShouldContain("User-Agent : My User Agent 1.0");
                    browser.PageSource.ShouldContain("TestKey : Process" + browser.ProcessId);

                    var fiddlerResponseSessionKey = Library.Constants.FiddlerResponseSessionKey.FormatIc(
                        browser.ProcessId,
                        browser.Url);

                    // validate the proxy session fingerprint
                    var fiddlerSession = FiddlerProxy.ResponseSession[fiddlerResponseSessionKey];
                    fiddlerSession.bHasResponse.ShouldBeTrue();
                    fiddlerSession.ResponseBody.Length.ShouldBeGreaterThan(0);
                    fiddlerSession.oResponse.headers.Count().ShouldBeGreaterThan(0);
                }
            }
            finally
            {
                FiddlerProxy.Cleanup(Constants.FiddlerPort);
            }
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
        /// Fiddler reporting the wrong
        /// browser process Id when more than one instance of Chrome was present. Here we test explicitly
        /// for that circumstance across all browsers.
        /// </summary>
        /// <param name="browserType">
        /// Browser type.
        /// </param>
        private void BrowserProxyInjectsPerRequestHeadersMultipleImplementation(
            BrowserType browserType)
        {
            /*
             * WARNING: Terminating this test while the proxy is running can lead to adverse side effects
             *          such as loss of web connectivity. Please allow the call to FiddlerProxy.Cleanup
             *          to complete.
             */

            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();

            var browserInstances = new Dictionary<BrowserType, int> { { browserType, 3 } };

            try
            {
                FiddlerProxy.Initialize(new string[0], Constants.FiddlerPort);
                using (var manager = new BrowserManager(browserInstances))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var browser = manager.AcquireBrowser(browserType);

                        // expected headers for this browser process
                        FiddlerProxy.RegisterInstanceHeaders(
                            browser.ProcessId, 
                            "My User Agent 1.0",
                            "TestKey: Process" + browser.ProcessId);

                        browser.NavigateTo($"{Constants.VulnerabilitiesAddress}dumpheaders.aspx");
                        browser.WaitForPageLoad(10);

                        // Then the expected request header should be present in the page
                        browser.PageSource.ShouldContain("User-Agent : My User Agent 1.0");
                        browser.PageSource.ShouldContain("TestKey : Process" + browser.ProcessId);
                    }
                }
            }
            finally
            {
                FiddlerProxy.Cleanup(Constants.FiddlerPort);
            }
        }
    }
}