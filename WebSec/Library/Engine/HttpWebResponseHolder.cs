//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System;
    using System.Net;

    /// <summary>
    /// Holder for an http request
    /// </summary>
    public sealed class HttpWebResponseHolder
    {
        /// <summary>
        /// Initializes a new instance of the HttpWebResponseHolder class.
        /// </summary>
        public HttpWebResponseHolder()
        {
            this.ResponseContent = string.Empty;
            this.HttpError = string.Empty;
            this.RequestHeaders = string.Empty;
            this.RequestAbsolutUri = string.Empty;
            this.Headers = new WebHeaderCollection();
        }

        /// <summary>
        /// Gets or sets the http error.
        /// </summary>
        public string HttpError { get; set; }

        /// <summary>
        /// Gets or sets the response date time.
        /// </summary>
        public DateTime ResponseDateTime { get; set; }

        /// <summary>
        /// Gets or sets the response content.
        /// </summary>
        public string ResponseContent { get; set; }

        /// <summary>
        /// Gets or sets the latency.
        /// </summary>
        public TimeSpan Latency { get; set; }

        /// <summary>
        /// Gets or sets the response uri.
        /// </summary>
        public Uri ResponseUri { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public WebHeaderCollection Headers { get; set; }

        /// <summary>
        /// Gets or sets the cookies.
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Gets or sets the response content type.
        /// </summary>
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Gets or sets the request absolute uri.
        /// </summary>
        public string RequestAbsolutUri { get; set; }

        /// <summary>
        /// Gets or sets the request host.
        /// </summary>
        public string RequestHost { get; set; }

        /// <summary>
        /// Gets or sets the request user agent.
        /// </summary>
        public string RequestUserAgent { get; set; }

        /// <summary>
        /// Gets or sets the request headers.
        /// </summary>
        public string RequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the browser page title.
        /// </summary>
        public string BrowserPageTitle { get; set; }
    }
}