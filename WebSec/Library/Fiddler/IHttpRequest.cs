//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    /// <summary>
    /// Describes an immutable object that represents an HTTP request.
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the request header details for this message.
        /// </summary>
        HttpHeaders RequestHeaders { get; }

        /// <summary>
        /// Gets the HTTP request "Host" header value.
        /// </summary>
        string RequestHost { get; }

        /// <summary>
        /// Gets the HTTP method value.
        /// </summary>
        string RequestHttpMethod { get; }

        /// <summary>
        /// Gets the HTTP version value.
        /// </summary>
        string RequestHttpVersion { get; }

        /// <summary>
        /// Gets the HTTP request content body.
        /// </summary>
        string RequestContent { get; }

        /// <summary>
        /// Gets the MIME type for the HTTP request content body.
        /// </summary>
        string RequestContentType { get; }

        /// <summary>
        /// Gets the HTTP "Cookie" request header.
        /// </summary>
        string RequestCookies { get; }

        /// <summary>
        /// Gets the User Agent header value.
        /// </summary>
        string RequestUserAgent { get; }

        /// <summary>
        /// Gets the full URL for this request.
        /// </summary>
        string RequestUrl { get; }
    }
}