//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using Common;
    using Common.Validation;
    using Interfaces;
    using Logger;
    using OpenQA.Selenium;
    
    /// <summary>
    /// Provides a facade over the Selenium IWebDriver type.
    /// </summary>
    public abstract class BrowserAbstract
    {
        /// <summary>
        /// The process.
        /// </summary>
        protected readonly IProcess Process;

        /// <summary>
        /// The browser.
        /// </summary>
        private readonly IWebDriver browser;

        /// <summary>
        /// State of the action.
        /// </summary>
        private readonly ActionState actionState;

        /// <summary>
        /// The alert message displayed.
        /// </summary>
        private readonly HashSet<string> alertMessageDisplayed;

        /// <summary>
        /// The last navigated location.
        /// </summary>
        private string lastNavigatedLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserAbstract" /> class.
        /// </summary>
        protected BrowserAbstract()
        {
            // Do not use: this ctor exists only to satisfy the test infrastructure.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserAbstract" /> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="browser">
        ///     The Selenium driver to wrap.
        /// </param>
        /// <param name="windowTitleFormat">
        ///     The expected format for the browser window title.
        /// </param>
        protected BrowserAbstract(IWebDriver browser, string windowTitleFormat)
        {
            Require.NotNullOrEmpty(() => windowTitleFormat);

            this.browser = browser;
            this.alertMessageDisplayed = new HashSet<string>();
            this.actionState = new ActionState(s =>
            {
                // dialog is blocking page load, remember the message and unblock.
                string text;
                DismissIfAlertDisplayedImpl(out text);
            });

            var script = browser as IJavaScriptExecutor;
            if (script == null)
            {
                throw new InvalidOperationException("Browser does not support script execution");
            }

            string unique = Guid.NewGuid().ToString();
            script.ExecuteScript($"document.title ='{unique}'");

            var processManager = ObjectResolver.Resolve<IProcessManager>();

            this.Process = processManager.GetProcessByWindowTitle(string.Format(windowTitleFormat, unique));

            if (this.Process == null)
            {
                throw new InvalidOperationException("Browser process could not be found");
            }
        }

        /// <summary>
        /// Gets the <see cref="BrowserType"/> associated with this browser.
        /// </summary>
        /// <value>
        /// The type of the browser.
        /// </value>
        public abstract BrowserType BrowserType { get; }

        /// <summary>
        /// Gets the url of the initial page per browser.
        /// </summary>
        /// <value>
        /// The default page.
        /// </value>
        public abstract string DefaultPage { get; }

        /// <summary>
        /// Gets the underlying Selenium <see cref="IWebDriver"/> associated with this browser.
        /// </summary>
        /// <value>
        /// The raw driver.
        /// </value>
        public virtual IWebDriver RawDriver => this.browser;

        /// <summary>
        /// Gets,If during a new navigation to url an alert was displayed we keep the text.
        /// </summary>
        /// <value>
        /// The alert message displayed.
        /// </value>
        public virtual HashSet<string> AlertMessageDisplayed => this.alertMessageDisplayed;

        /// <summary>
        /// Gets the HTML source from the page.
        /// </summary>
        /// <value>
        /// The page source.
        /// </value>
        public virtual string PageSource
        {
            get
            {
                return new WebDriverExceptionRetry(this.actionState).Execute(
                    MethodBase.GetCurrentMethod().Name,
                    this.CreateContext(),
                    () => this.browser.PageSource);
            }
        }

        /// <summary>
        /// Gets the page title.
        /// </summary>
        /// <value>
        /// The page title.
        /// </value>
        public virtual string PageTitle
        {
            get
            {
                return new WebDriverExceptionRetry(this.actionState).Execute(
                    MethodBase.GetCurrentMethod().Name,
                    this.CreateContext(), 
                    () => this.browser.Title);
            }
        }

        /// <summary>
        /// Gets the current URL according to the browser.
        /// </summary>
        /// <remarks>
        /// This may be a non-routable/special browser address such as about:
        /// </remarks>
        /// <value>
        /// The URL.
        /// </value>
        public virtual string Url
        {
            get
            {
                return new WebDriverExceptionRetry(this.actionState).Execute(
                    MethodBase.GetCurrentMethod().Name,
                    this.CreateContext(), 
                    () => this.browser.Url);
            }
        }

        /// <summary>
        /// Gets the process identifier of the underlying browser.
        /// </summary>
        /// <value>
        /// The identifier of the process.
        /// </value>
        public virtual int ProcessId => this.Process.Id;

        /// <summary>
        /// Gets the process identifier of the underlying browser driver.
        /// </summary>
        /// <remarks>
        /// Will be 0 if no driver exists.
        /// </remarks>
        /// <value>
        /// The identifier of the parent process.
        /// </value>
        public virtual int ParentProcessId => this.Process.ParentId;

        /// <summary>
        /// Gets the collection of cookies from the browser.
        /// </summary>
        /// <value>
        /// all cookies.
        /// </value>
        public virtual CookieCollection AllCookies
        {
            get
            {
                return new WebDriverExceptionRetry(this.actionState)
                    .Execute(
                        MethodBase.GetCurrentMethod().Name,
                        this.CreateContext(),
                        () =>
                        {
                            var cookies = new CookieCollection();

                            foreach (var cookie in this.browser.Manage().Cookies.AllCookies)
                            {
                                if (!string.IsNullOrEmpty(cookie.Name))
                                {
                                    cookies.Add(ConvertCookie(cookie));
                                }
                            }

                            return cookies;
                        });
            }
        }

        /// <summary>
        /// Find an Html Element on the current page hosted in the browser.
        /// </summary>
        /// <param name="searchBy">The search condition.</param>
        /// <returns>An IWebElement if found.</returns>
        public virtual IWebElement FindWebElement(By searchBy)
        {
            var searchContext = this.browser as ISearchContext;
            try
            {
                return searchContext.FindElement(searchBy);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        /// <summary>
        /// Find a list of Html elements on the current page hosted in the browser.
        /// </summary>
        /// <param name="searchBy">The search condition.</param>
        /// <returns>A collection of IWebElements.</returns>
        public virtual IReadOnlyCollection<IWebElement> FindWebElements(By searchBy)
        {
            var searchContext = this.browser as ISearchContext;

            try
            {
                return searchContext.FindElements(searchBy);
            }
            catch (InvalidSelectorException ex)
            {
                Logger.WriteWarning(ex);
            }

            return new List<IWebElement>();
        }

        /// <summary>
        /// Get a Cookie with its name. Note that Selenium only allows looking for cookies that apply to the current document in the browser.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <returns>A cookie if found, or null.</returns>
        public virtual System.Net.Cookie GetCookie(string name)
        {
            return ConvertCookie(this.browser.Manage().Cookies.GetCookieNamed(name));
        }

        /// <summary>
        /// Adds a cookie to the browser cookie collection.
        /// </summary>
        /// <exception cref="WebDriverException">
        ///     Thrown when a Web Driver error condition occurs.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="name">
        ///     The cookie name.
        /// </param>
        /// <param name="value">
        ///     The cookie value.
        /// </param>
        /// <param name="domain">
        ///     The cookie domain.
        /// </param>
        /// <param name="path">
        ///     The cookie path.
        /// </param>
        /// <param name="expiry">
        ///     The cookie expiry date.
        /// </param>
        public virtual void AddCookie(string name, string value, string domain, string path, DateTime expiry)
        {
            new WebDriverExceptionRetry(this.actionState)
                .Execute(
                    MethodBase.GetCurrentMethod().Name,
                    this.CreateContext(),
                    () =>
                    {
                        Uri thisUrl;
                        if (Uri.TryCreate(this.browser.Url, UriKind.Absolute, out thisUrl))
                        {
                            // Selenium does not accept cookies for another domain
                            if (thisUrl.Host.Contains(domain))
                            {
                                var cookie = new OpenQA.Selenium.Cookie(name, value, domain, path, expiry);

                                try
                                {
                                    this.browser.Manage().Cookies.AddCookie(cookie);
                                }
                                catch (WebDriverException exception)
                                {
                                    // this is not uncommon, retry will not help so we ignore
                                    if (!exception.Message.Contains("You may only set cookies on html documents"))
                                    {
                                        Logger.WriteWarning(
                                            "AddCookie() failed with name: {0} value: {1} domain: {2} path: {3} expiry: {4}",
                                            cookie.Name,
                                            cookie.Value,
                                            cookie.Domain,
                                            cookie.Path,
                                            cookie.Expiry);

                                        // invalid domain, retry will not help so we ignore after logging
                                        if (!exception.Message.Contains("invalid cookie domain"))
                                        {
                                            throw;
                                        }
                                    }
                                }
                                catch (InvalidOperationException exception)
                                {
                                    // this is not uncommon, ignore
                                    if (!exception.Message.Contains("Access to 'cookie' is denied for this document") &&
                                        !exception.Message.Contains("Failed to set the 'cookie'"))
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    });
        }

        /// <summary>
        /// Gets a value indicating whether the browser is presenting an alert dialog.
        /// </summary>
        /// <remarks>
        /// If present, the dialog will be dismissed.
        /// </remarks>
        /// <param name="text">
        ///     The text.
        /// </param>
        /// <returns>
        /// True if a dialog was visible, otherwise false.
        /// </returns>
        public virtual bool DismissedIfAlertDisplayed(out string text)
        {
            string message = null;

            var visible = new WebDriverExceptionRetry(this.actionState)
                .Execute(
                    MethodBase.GetCurrentMethod().Name,
                    this.CreateContext(),
                    () => this.DismissIfAlertDisplayedImpl(out message));

            text = message;
            return visible;
        }

        /// <summary>
        /// Navigates the browser to a given URL.
        /// </summary>
        /// <param name="url">
        ///     The given URL.
        /// </param>
        public virtual void NavigateTo(string url)
        {
            this.alertMessageDisplayed.Clear();
            this.lastNavigatedLocation = url;
            new WebDriverExceptionRetry(this.actionState).Execute(
                MethodBase.GetCurrentMethod().Name,
                this.CreateContext(), 
                () => this.browser.Navigate().GoToUrl(url));
        }

        /// <summary>
        /// Deletes all cookies from the browser cache.
        /// </summary>
        public virtual void DeleteAllCookies()
        {
            new WebDriverExceptionRetry(this.actionState).Execute(
                MethodBase.GetCurrentMethod().Name,
                this.CreateContext(), 
                () => this.browser.Manage().Cookies.DeleteAllCookies());
        }

        /// <summary>
        /// Delete a cookie from the "cookie jar".
        /// </summary>
        /// <param name="cookie">A cookie to be deleted.</param>
        public virtual void DeleteCookie(System.Net.Cookie cookie)
        {
            OpenQA.Selenium.Cookie seleniumCookie = ConvertCookie(cookie);
            new WebDriverExceptionRetry(this.actionState).Execute(
                MethodBase.GetCurrentMethod().Name,
                this.CreateContext(), 
                () => this.browser.Manage().Cookies.DeleteCookie(seleniumCookie));
        }

        /// <summary>
        /// Closes the browser process and its associated Selenium driver process (where appropriate).
        /// </summary>
        public virtual void Quit()
        {
            try
            {
                new WebDriverExceptionRetry(this.actionState).Execute(
                    MethodBase.GetCurrentMethod().Name,
                    this.CreateContext(),
                    () => this.browser.Quit());
            }
            catch (Win32Exception)
            {
                // occasionally Selenium will throw an "Access Denied" exception while
                // trying to kill a process that's already in a "terminating" state. Ignore.
            }
            catch (InvalidOperationException)
            {
                // Cannot process request because the process (16124) has exited. Ignore
            }
        }

        /// <summary>
        /// Waits for the page to signal it has finished loading.
        /// </summary>
        /// <param name="timeoutMilleseconds">
        ///     The maximum time to wait for the request to complete.
        /// </param>
        /// <returns>
        /// True if the page loaded within the given timeout, False if the timeout was exceeded.
        /// </returns>
        public virtual bool WaitForPageLoad(int timeoutMilleseconds)
        {
            var script = this.browser as IJavaScriptExecutor;

            try
            {
                return new WebDriverExceptionRetry(this.actionState)
                    .Execute(
                        MethodBase.GetCurrentMethod().Name,
                        this.CreateContext() + " " + this.lastNavigatedLocation,
                        () => this.WaitForPageLoadImplementation(script, timeoutMilleseconds));
            }
            catch (Exception ex)
            {
                // webdriver exception retry throws generic exception,
                // for this case when we wait for a page to load we should catch these exception and return true of false
                Logger.WriteWarning(ex);
            }

            return false;
        }

        /// <summary>
        /// Convert an OpenQA.Selenium.Cookie instance to a System.Net.Cookie one.
        /// </summary>
        /// <param name="cookie">An instance of System.Net.Cookie</param>
        /// <returns>An instance of type OpenQA.Selenium.Cookie.</returns>
        private static OpenQA.Selenium.Cookie ConvertCookie(System.Net.Cookie cookie)
        {
            return cookie == null
                ? null
                : new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Expires);
        }

        /// <summary>
        /// Convert an System.Net.Cookie instance to a System.Net.Cookie one.
        /// </summary>
        /// <param name="cookie">An instance of OpenQA.Selenium.Cookie</param>
        /// <returns>An instance of type System.Net.Cookie.</returns>
        private static System.Net.Cookie ConvertCookie(OpenQA.Selenium.Cookie cookie)
        {
            return cookie == null
                ? null
                : new System.Net.Cookie
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    Path = cookie.Path,
                    Expires = cookie.Expiry.GetValueOrDefault()
                };
        }

        /// <summary>
        /// Wait for page load implementation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="script">
        ///     The script.
        /// </param>
        /// <param name="timeoutMilleseconds">
        ///     The maximum time to wait for the request to complete.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private bool WaitForPageLoadImplementation(IJavaScriptExecutor script, int timeoutMilleseconds)
        {
            bool response = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (script == null)
            {
                throw new InvalidOperationException("Browser does not support script execution");
            }

            try
            {
                // see http://msdn.microsoft.com/en-us/library/ie/ms534361(v=vs.85).aspx
                var result = script.ExecuteScript("return document.readyState");

                // result may occasionally be null
                string state = result?.ToString() ?? string.Empty;

                // NOTE: This will wait for the document to pull in related resources and run all load-time scripts,
                // but will not wait for any AJAX requests created inline by the page. The browser does not expose
                // any details about outstanding AJAX requests.
                //
                // In the future we can consider adding support for the jquery $.active property if we find enough
                // sites that use jquery
                if (state.Equals("complete") || state.Equals("loaded"))
                {
                    response = true;
                }

                var delay = (int)(timeoutMilleseconds - stopwatch.Elapsed.TotalMilliseconds);
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("waiting for doc.body failed") ||
                    exception.Message.Contains("JavaScript error"))
                {
                    // Firefox not an HTML result, possibly XML
                    return true;
                }

                throw;
            }
            catch (NoSuchElementException)
            {
                // Not an HTML page
            }

            return response;
        }

        /// <summary>
        /// Dismiss if alert displayed implementation.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private bool DismissIfAlertDisplayedImpl(out string message)
        {
            message = null;

            try
            {
                // This will throw if no alert is present
                var alert = this.browser.SwitchTo().Alert();

                // the following code will run if an alert is active
                message = alert.Text;
                alert.Dismiss();
                this.alertMessageDisplayed.Add(message);
                alert.Dismiss();

                Logger.WriteInfo(
                    "Alert appears to be visible for browser: {0} text: '{1}' url: {2}",
                    this.BrowserType,
                    message,
                    this.lastNavigatedLocation);

                return true;
            }
            catch (NoAlertPresentException)
            {
            }

            return false;
        }

        /// <summary>
        /// Creates the context.
        /// </summary>
        /// <returns>
        /// The new context.
        /// </returns>
        private string CreateContext()
        {
            return $"{this.BrowserType} (PID {this.ProcessId}) ";
        }
    }
}