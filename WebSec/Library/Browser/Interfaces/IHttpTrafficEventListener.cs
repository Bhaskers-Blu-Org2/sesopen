//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser.Interfaces
{
    using Fiddler;

    /// <summary>
    /// Define signature of http traffic handler.
    /// </summary>
    /// <param name="session">The http session.</param>
    public delegate void HttpTrafficEventHandler(FiddlerHttpSession session);

    /// <summary>
    /// Interface that defines http traffic listener event handlers.
    /// </summary>
    public interface IHttpTrafficEventListener
    {
        /// <summary>
        /// Before http request event handler.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly",
            Justification = "To keep consistent with FidderProxy event handler signature")]
        event HttpTrafficEventHandler BeforeRequest;

        /// <summary>
        /// Before http response event handler.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly",
            Justification = "To keep consistent with FidderProxy event handler signature")]
        event HttpTrafficEventHandler BeforeResponse;
    }
}