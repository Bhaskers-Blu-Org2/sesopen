//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using global::Fiddler;

    /// <summary>
    /// An facade to simplify interaction with the Fiddler <see cref="Session"/> type.
    /// </summary>
    public class FiddlerHttpSession : IHttpRequest
    {
        /// <summary>
        /// The session.
        /// </summary>
        private readonly Session session;

        /// <summary>
        /// Initializes a new instance of the <see cref="FiddlerHttpSession"/> class.
        /// </summary>
        /// <param name="session">The Fiddler session associated with this object.</param>
        public FiddlerHttpSession(Session session)
        {
            this.session = session;
        }

        /// <summary>
        /// Gets the transmission sequence number for this session.
        /// </summary>
        public int SequenceNumber => this.session.id;

        /// <summary>
        /// Gets the response header details for this message.
        /// </summary>
        public HttpHeaders ResponseHeaders
        {
            get
            {
                var result = new HttpHeaders();

                foreach (HTTPHeaderItem item in this.session.oResponse.headers)
                {
                    result.Add(new KeyValuePair<string, string>(item.Name, item.Value));
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the MIME type for the HTTP response content body.
        /// </summary>
        public string ResponseContentType => this.session.oResponse.headers["Content-Type"] ?? string.Empty;

        /// <summary>
        /// Gets the HTTP version for the response.
        /// </summary>
        public string ResponseHttpVersion => this.session.oResponse.headers.HTTPVersion ?? string.Empty;

        /// <summary>
        /// Gets the HTTP code for this response.
        /// </summary>
        public int ResponseCode => this.session.oResponse.headers.HTTPResponseCode;

        /// <summary>
        /// Gets the HTTP response Cookie collection.
        /// </summary>
        public IEnumerable<string> ResponseCookies
        {
            get
            {
                var cookies = new List<string>();

                foreach (HTTPHeaderItem item in this.session.oResponse.headers)
                {
                    if (item.Name.EqualsOi("Set-Cookie"))
                    {
                        cookies.Add(item.Value);
                    }
                }

                return cookies;
            }
        }

        /// <inheritdoc/>
        public HttpHeaders RequestHeaders
        {
            get
            {
                var result = new HttpHeaders();

                foreach (HTTPHeaderItem item in this.session.oRequest.headers)
                {
                    result.Add(new KeyValuePair<string, string>(item.Name, item.Value));
                }

                return result;
            }
        }

        /// <inheritdoc/>
        public string RequestHttpMethod => this.session.oRequest.headers.HTTPMethod;

        /// <inheritdoc/>
        public string RequestHttpVersion => this.session.oRequest.headers.HTTPVersion;

        /// <inheritdoc/>
        public string RequestHost => this.session.oRequest.headers["Host"] ?? string.Empty;

        /// <inheritdoc/>
        public string RequestUrl => this.session.fullUrl;

        /// <inheritdoc/>
        public string RequestContent => this.session.GetRequestBodyAsString();

        /// <inheritdoc/>
        public string RequestContentType => this.session.oRequest.headers["Content-Type"] ?? string.Empty;

        /// <inheritdoc/>
        public string RequestCookies
        {
            get
            {
                StringBuilder buf = new StringBuilder();
                foreach (HTTPHeaderItem item in this.session.oRequest.headers)
                {
                    if (item.Name.EqualsOi("Cookie"))
                    {
                        if (buf.Length > 0)
                        {
                            buf.Append("; ");
                        }

                        buf.Append(item.Value);
                    }
                }

                return buf.ToString();
            }
        }

        /// <inheritdoc/>
        public string RequestUserAgent => this.session.oRequest.headers["User-Agent"] ?? string.Empty;

        /// <summary>
        /// Replace the "Cookie" header with new value.
        /// </summary>
        /// <param name="requestCookie">The string value that will be used as the "Cookie" header.</param>
        public void ReplaceRequestCookies(string requestCookie)
        {
            this.session.oRequest.headers.Remove("Cookie");
            if (!string.IsNullOrWhiteSpace(requestCookie))
            {
                this.session.oRequest.headers.Add("Cookie", requestCookie);
            }
        }
    }
}