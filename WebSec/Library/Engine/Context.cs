//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using Browser;
    using Browser.Interfaces;
    using Common;
    using Fiddler;
    using Interfaces;
    using Logger;
    using Payloads;
    using PluginBase;

    /// <summary>
    /// The "network" object that does the sending and receiving of HTTP requests 
    /// and manages the current state.
    /// </summary>
    public class Context : IContext
    {
        /// <summary>
        /// Initializes a new instance of the WebSec.Library.Engine.Context class.
        /// </summary>
        public Context()
        {
            this.CurrentCookies = new ConcurrentDictionary<string, CookieCollection>();
            this.Vulnerabilities = new ConcurrentDictionary<string, Vulnerability>();
            this.Payloads = ObjectResolver.Resolve<IPayloads>();
            this.ActiveDetectors = new List<Type>();
        }

        /// <summary>
        /// Gets the active detectors.
        /// </summary>
        /// <value>
        /// The active detectors.
        /// </value>
        public List<Type> ActiveDetectors { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<string, Vulnerability> Vulnerabilities { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<string, CookieCollection> CurrentCookies { get; }

        /// <inheritdoc />
        public IPayloads Payloads { get; set; }

        /// <inheritdoc />
        public WebRequestContext SendRequest(
            ContextSendRequestParameter contextSendRequestParameter)
        {
            // create web request context to be returned
            var webRequestContext = new WebRequestContext();

            // create uri
            Uri uri = CreateUri(contextSendRequestParameter.Url);

            // execute a web request
            BrowserNavigateToPage(contextSendRequestParameter, uri, webRequestContext);

            return webRequestContext;
        }

        /// <inheritdoc />
        public void RunDetectors(
            HttpWebResponseHolder httpWebResponseHolder,
            ITarget target,
            string plugin,
            TestCase testCase,
            string testParameter,
            string testedValue)
        {
            if (this.ActiveDetectors.Count == 0)
            {
                Logger.WriteWarning("No active detectors have been found.");
                return;
            }

            // if no response then exit
            if (httpWebResponseHolder?.ResponseUri == null)
            {
                return;
            }

            // iterate through all active detectors and run them
            foreach (Type activeDetector in this.ActiveDetectors)
            {
                var pluginBaseAbstract = Activator.CreateInstance(activeDetector) as PluginBaseAbstract;

                if (pluginBaseAbstract == null)
                {
                    throw new NullReferenceException("{0} can't be instantiated".FormatIc(activeDetector.Name));
                }

                // execute all detectors
                if (!pluginBaseAbstract.TestBaseType.Equals(TestBaseType.Detector))
                {
                    continue;
                }

                this.RunDetector(
                    pluginBaseAbstract,
                    target,
                    Target.Create(httpWebResponseHolder.ResponseUri.AbsoluteUri),
                    httpWebResponseHolder,
                    plugin,
                    testCase,
                    testParameter,
                    testedValue);
            }
        }

        /// <inheritdoc />
        public BrowserAbstract AcquireBrowser(BrowserType browserType)
        {
            var browserManager = ObjectResolver.Resolve<IBrowserManager>();

            // there is no browser pool created
            if (browserManager.AvailableBrowserTypes.All(t => t != browserType))
            {
                throw new ArgumentException(
                    $"There is no browser pool created for type {browserType}.",
                    browserType.ToString());
            }

            // try to acquire a browser
            BrowserAbstract browser = browserManager.AcquireBrowser(browserType);

            if (browser == null)
            {
                throw new ArgumentException(
                    "Browser type {0} not available.".FormatIc(browserType.ToString()),
                    browserType.ToString());
            }

            return browser;
        }

        /// <inheritdoc />
        public void ReleaseBrowser(BrowserAbstract browser)
        {
            var browserManager = ObjectResolver.Resolve<IBrowserManager>();
            browserManager.ReleaseBrowser(browser);
        }

        /// <summary>
        /// The create uri.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="Uri"/>.
        /// </returns>
        private static Uri CreateUri(string url)
        {
            Uri uri = null;

            try
            {
                uri = new Uri(url);
            }
            catch (UriFormatException ex)
            {
                Logger.WriteError(ex, url);
            }

            return uri;
        }

        /// <summary>
        /// The check for unexpected dialog.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        private static void CheckForUnexpectedDialog(Uri uri, BrowserAbstract browser, Exception exception)
        {
            if (browser != null &&
                (exception.Message.Contains("unexpected alert open")
                 || (exception.InnerException != null &&
                     exception.InnerException.Message.Contains("unexpected alert open"))))
            {
                // ReSharper disable once RedundantAssignment
                string text = string.Empty;

                var visible = browser.DismissedIfAlertDisplayed(out text);

                // did we miss a vuln?
                if (string.IsNullOrEmpty(text))
                {
                    Logger.WriteError(
                        "Context: Missed a vuln? Browser {0} (PID {1}) threw UnhandledAlertException at {2}, verified {3} text '{4}'",
                        browser.BrowserType.ToString(),
                        browser.ProcessId,
                        uri.OriginalString,
                        visible,
                        text);
                }
            }
        }

        /// <summary>
        /// Browser navigate to page.
        /// </summary>
        /// <param name="contextSendRequestParameter">
        /// The context send request parameter.
        /// </param>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="webRequestContext">
        /// The web request context.
        /// </param>
        private void BrowserNavigateToPage(
            ContextSendRequestParameter contextSendRequestParameter,
            Uri uri,
            WebRequestContext webRequestContext)
        {
            webRequestContext.ResponseHolder = new HttpWebResponseHolder();
            BrowserAbstract browser = null;

            try
            {
                var browserManager = ObjectResolver.Resolve<IBrowserManager>();

                // there is no browser pool created for this browser type
                if (browserManager.AvailableBrowserTypes.All(t => t != contextSendRequestParameter.BrowserType))
                {
                    return;
                }

                // try to acquire a browser if we don't have one already
                browser = webRequestContext.Browser ??
                          browserManager.AcquireBrowser(contextSendRequestParameter.BrowserType);

                if (browser == null)
                {
                    Library.Logger.Logger.WriteWarning(
                        "Browser pool failed allocation (measured by BrowserPoolCounters.FailedAllocationsPerMinute).");

                    return;
                }

                // in case we acquired then set this in the request context
                webRequestContext.Browser = browser;

                 // Add request cookies.
                if (this.CurrentCookies.ContainsKey(uri.Host))
                {
                    foreach (Cookie cookie in this.CurrentCookies[uri.Host])
                    {
                        browser.AddCookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Expires);
                    }
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                browser.NavigateTo(uri.OriginalString);

                stopWatch.Stop();

                // FireFox does not block on the presence of an alert dialog, so we manually check here.
                string alertText;
                browser.DismissedIfAlertDisplayed(out alertText);

                browser.WaitForPageLoad(Constants.BrowserWaitForPageLoadInMilliseconds);

                // check for messages again following page load - do this before querying other browser properties
                browser.DismissedIfAlertDisplayed(out alertText);

                var fiddlerResponseSessionKey = Library.Constants.FiddlerResponseSessionKey.FormatIc(
                    browser.ProcessId,
                    browser.Url);

                // get fiddler response
                var fiddlerResponse = FiddlerProxy.ResponseSession.ContainsKey(fiddlerResponseSessionKey)
                    ? FiddlerProxy.ResponseSession[fiddlerResponseSessionKey].oResponse
                    : null;

                var fiddlerRequest = FiddlerProxy.ResponseSession.ContainsKey(fiddlerResponseSessionKey)
                    ? FiddlerProxy.ResponseSession[fiddlerResponseSessionKey].oRequest
                    : null;

                // populate response holder
                webRequestContext.ResponseHolder.StatusCode = fiddlerResponse != null
                    ? (HttpStatusCode)fiddlerResponse.headers.HTTPResponseCode
                    : HttpStatusCode.OK;

                var source = browser.PageSource;
                webRequestContext.ResponseHolder.ResponseContent = source;

                webRequestContext.ResponseHolder.RequestAbsolutUri = uri.AbsoluteUri;
                webRequestContext.ResponseHolder.RequestHost = uri.Host;

                Uri resultUri;
                if (Uri.TryCreate(browser.Url, UriKind.Absolute, out resultUri))
                {
                    webRequestContext.ResponseHolder.ResponseUri = resultUri;
                }

                webRequestContext.ResponseHolder.ResponseDateTime = DateTime.UtcNow;
                webRequestContext.ResponseHolder.RequestUserAgent =
                    contextSendRequestParameter.BrowserType.ToString();
                webRequestContext.ResponseHolder.BrowserPageTitle = browser.PageTitle;
                webRequestContext.ResponseHolder.ResponseContentType = "text/html";
                webRequestContext.ResponseHolder.Latency = new TimeSpan(0, 0, 0, 0, (int)stopWatch.Elapsed.TotalMilliseconds);

                // set headers from fiddler for this request
                if (fiddlerRequest != null)
                {
                    webRequestContext.ResponseHolder.RequestHeaders =
                        Regex.Replace(fiddlerRequest.headers.ToString().Trim(), @"[\u0000-\u001F]", string.Empty);
                }

                // set headers from fiddler for this response
                if (fiddlerResponse != null)
                {
                    webRequestContext.ResponseHolder.SetResponseFiddlerHeaders(fiddlerResponse.headers);
                }

                // set back response cookies
                webRequestContext.ResponseHolder.Cookies = browser.AllCookies;
            }
            catch (Exception exception)
            {
                webRequestContext.ResponseHolder.HttpError = exception.Message;
                CheckForUnexpectedDialog(uri, browser, exception);
            }
        }

        /// <summary>
        /// Executes the detector operation.
        /// </summary>
        /// <param name="detectorInstance">
        /// The detector instance.
        /// </param>
        /// <param name="requestTarget">
        /// The request target.
        /// </param>
        /// <param name="responseTarget">
        /// The response target.
        /// </param>
        /// <param name="webResponseHolder">
        /// The web response holder.
        /// </param>
        /// <param name="plugInName">
        /// Name of the plug in.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        /// <param name="testParameter">
        /// The tested parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested value.
        /// </param>
        private void RunDetector(
            PluginBaseAbstract detectorInstance,
            ITarget requestTarget,
            ITarget responseTarget,
            HttpWebResponseHolder webResponseHolder,
            string plugInName,
            TestCase testCase,
            string testParameter,
            string testValue)
        {
            try
            {
                Logger.WriteDebug("Run detector: {0}", detectorInstance.GetType().Name);

                detectorInstance.Init(
                    this,
                    responseTarget);

                detectorInstance.InspectResponse(
                    requestTarget,
                    responseTarget,
                    webResponseHolder,
                    plugInName,
                    testCase,
                    testParameter,
                    testValue);

                foreach (var vuln in detectorInstance.Vulnerabilities)
                {
                    this.Vulnerabilities[Guid.NewGuid().ToString()] = vuln;
                }
            }
            catch (Exception ex)
            {
                // write to the tests and detectors log.
                Library.Logger.Logger.WriteError(ex);
            }
        }
    }
}