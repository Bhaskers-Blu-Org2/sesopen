//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System.Net;
    using Browser;

    /// <summary>
    /// The send request context parameter.
    /// </summary>
    public class ContextSendRequestParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextSendRequestParameter" /> class.
        /// </summary>
        public ContextSendRequestParameter()
        {
            this.AllowRedirect = true;
            this.BrowserType = BrowserType.Chrome;
        }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Cookies.
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether allow redirect.
        /// </summary>
        public bool AllowRedirect { get; set; }

        /// <summary>
        /// Gets or sets the plugin name.
        /// </summary>
        public string PluginName { get; set; }

        /// <summary>
        /// Gets or sets the Parameter name fuzzed.
        /// </summary>
        public string ParameterNameFuzzed { get; set; }

        /// <summary>
        /// Gets or sets the Post message.
        /// </summary>
        public string PostMessage { get; set; }

        /// <summary>
        /// Gets or sets the Custom content type.
        /// </summary>
        public string CustomContentType { get; set; }

        /// <summary>
        /// Gets or sets the browser type.
        /// </summary>
        public BrowserType BrowserType { get; set; }
    }
}