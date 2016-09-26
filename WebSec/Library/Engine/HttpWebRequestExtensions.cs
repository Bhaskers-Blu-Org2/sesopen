//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System;
    using System.Text.RegularExpressions;
    using global::Fiddler;

    /// <summary>
    /// Extension class for HttpWebRequest.
    /// </summary>
    public static class HttpWebRequestExtensions
    {
        /// <summary>
        /// The set response fiddler headers.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        public static void SetResponseFiddlerHeaders(
            this HttpWebResponseHolder response,
            HTTPResponseHeaders headers)
        {
            foreach (HTTPHeaderItem httpResponseHeader in headers)
            {
                try
                {
                    // remove control char from header value
                    httpResponseHeader.Value = Regex.Replace(
                        httpResponseHeader.Value.Trim(),
                        @"[\u0000-\u001F]",
                        string.Empty);
                    response.Headers[httpResponseHeader.Name] = httpResponseHeader.Value;
                }
                catch (ArgumentException ex)
                {
                    Library.Logger.Logger.WriteWarning(
                        ex, 
                        "control char hasn't been removed , header value {0}",
                        httpResponseHeader.Value);
                }
            }
        }
    }
}