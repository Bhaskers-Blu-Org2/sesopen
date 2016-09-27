//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common
{
    using System;

    /// <summary>
    /// The websec worker constants.
    /// </summary>
    public sealed class Constants
    {
        /// <summary>
        /// Html tag.
        /// </summary>
        public const string HtmlTag = "<html";

        /// <summary>
        /// The HTML document tag.
        /// </summary>
        public const string IsHtmlDoc = "<!DOCTYPE HTML";

        /// <summary>
        /// The browser wait for page load in milliseconds.
        /// </summary>
        public const int BrowserWaitForPageLoadInMilliseconds = 1000;

        /// <summary>
        /// The vulnerabilities site port.
        /// </summary>
        public const int VulnerabilitiesSitePort = 60655;

        /// <summary>
        /// The vulnerabilities site ssl port.
        /// </summary>
        public const int VulnerabilitiesSiteSslPort = 44300;

        /// <summary>
        /// The fiddler port.
        /// </summary>
        public const int FiddlerPort = 8879;

        /// <summary>
        /// Initializes static members of the WebSec.Common.Constants class.
        /// </summary>
        static Constants()
        {
            VulnerabilitiesAddress = "http://localhost:{0}/".FormatIc(VulnerabilitiesSitePort);
            VulnerabilitiesAddressSsl = "https://localhost:{0}/".FormatIc(VulnerabilitiesSiteSslPort);
        }

        /// <summary>
        /// Gets the vulnerabilities address.
        /// </summary>
        /// <value>
        /// The vulnerabilities address.
        /// </value>
        public static string VulnerabilitiesAddress { get; private set; } 

        /// <summary>
        /// Gets the vulnerabilities address ssl.
        /// </summary>
        /// <value>
        /// The vulnerabilities address ssl.
        /// </value>
        public static string VulnerabilitiesAddressSsl { get; private set; } 
    }
}