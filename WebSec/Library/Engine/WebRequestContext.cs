//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using Browser;

    /// <summary>
    /// Web request context.
    /// </summary>
    public sealed class WebRequestContext
    {
        /// <summary>
        /// Gets or sets the response holder.
        /// </summary>
        public HttpWebResponseHolder ResponseHolder { get; set; }

        /// <summary>
        /// Gets or sets the browser.
        /// </summary>
        public BrowserAbstract Browser { get; set; }
    }
}