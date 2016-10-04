//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine.Interfaces
{
    using System.Collections.Concurrent;
    using System.Net;
    using Browser;
    using Payloads;
    using PluginBase;

    /// <summary>
    /// The context interface. Provides access to web request and run all the detectors.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Gets a dictionary containing the set of found vulnerabilities.
        /// Key can be anything to uniquely identify the vulnerability.
        /// </summary>
        ConcurrentDictionary<string, Vulnerability> Vulnerabilities { get; }

        /// <summary>
        /// Gets the current cookies. After you send the request store cumulative cookies per domain.
        /// </summary>
        ConcurrentDictionary<string, CookieCollection> CurrentCookies { get; }

        /// <summary>
        /// Gets or sets the payloads.
        /// </summary>
        /// <value>
        /// The payloads.
        /// </value>
        IPayloads Payloads { get; set; }

        /// <summary>
        /// The acquire browser.
        /// </summary>
        /// <param name="browserType">
        /// The browser type.
        /// </param>
        /// <returns>
        /// The <see cref="BrowserAbstract"/>.
        /// </returns>
        BrowserAbstract AcquireBrowser(BrowserType browserType);

        /// <summary>
        /// The release browser.
        /// </summary>
        /// <param name="browser">
        /// The browser.
        /// </param>
        void ReleaseBrowser(BrowserAbstract browser);

        /// <summary>
        /// The send request.
        /// </summary>
        /// <param name="contextSendRequestParameter">
        /// The context send request parameter.
        /// </param>
        /// <returns>
        /// The <see cref="WebRequestContext"/>.
        /// </returns>
        WebRequestContext SendRequest(ContextSendRequestParameter contextSendRequestParameter);

        /// <summary>
        /// The run detectors.
        /// </summary>
        /// <param name="httpWebResponseHolder">
        /// The http web response holder.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="plugin">
        /// The plugin.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        /// <param name="testParameter">
        /// The tested parameter.
        /// </param>
        /// <param name="testedValue">
        /// The tested value.
        /// </param>
        void RunDetectors(
            HttpWebResponseHolder httpWebResponseHolder,
            ITarget target,
            string plugin,
            TestCase testCase,
            string testParameter,
            string testedValue);
    }
}