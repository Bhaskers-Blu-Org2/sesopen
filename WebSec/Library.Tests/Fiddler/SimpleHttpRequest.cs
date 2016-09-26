//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Fiddler
{
    using System;
    using System.Linq;
    using Library.Fiddler;
    using WebSec.Common.Validation;

    /// <summary>
    /// Represents an HTTP request.
    /// </summary>
    public class SimpleHttpRequest : IHttpRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleHttpRequest"/> class.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="httpMethod">
        /// The http Method.
        /// </param>
        /// <param name="httpVersion">
        /// The http Version.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        public SimpleHttpRequest(
            string url,
            string httpMethod,
            string httpVersion,
            string content,
            HttpHeaders headers)
        {
            Require.NotNullOrEmpty(() => url);
            Require.NotNullOrEmpty(() => httpMethod);
            Require.NotNullOrEmpty(() => httpVersion);
            Require.NotNull(() => headers);

            this.RequestUrl = url;
            this.RequestHttpMethod = httpMethod;
            this.RequestHttpVersion = httpVersion;
            this.RequestContent = content;
            this.RequestHeaders = headers;
            this.RequestHost = this.GetHeaderValue("Host");
            this.RequestContentType = this.GetHeaderValue("Content-Type");
            this.RequestCookies = this.GetHeaderValue("Cookie");
            this.RequestUserAgent = this.GetHeaderValue("User-Agent");
        }

        /// <inheritdoc/>
        public HttpHeaders RequestHeaders { get; protected set; }

        /// <inheritdoc/>
        public string RequestHost { get; protected set; }

        /// <inheritdoc/>
        public string RequestHttpMethod { get; protected set; }

        /// <inheritdoc/>
        public string RequestHttpVersion { get; protected set; }

        /// <inheritdoc/>
        public string RequestContent { get; protected set; }

        /// <inheritdoc/>
        public string RequestContentType { get; protected set; }

        /// <inheritdoc/>
        public string RequestCookies { get; protected set; }

        /// <inheritdoc/>
        public string RequestUserAgent { get; protected set; }

        /// <inheritdoc/>
        public string RequestUrl { get; protected set; }

        /// <summary>
        /// The get header value.
        /// </summary>
        /// <param name="headerKey">
        /// The header key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetHeaderValue(string headerKey)
        {
            var header =
                this.RequestHeaders.FirstOrDefault(
                    h => h.Key.Equals(headerKey, StringComparison.InvariantCultureIgnoreCase));

            return header.Value ?? string.Empty;
        }
    }
}