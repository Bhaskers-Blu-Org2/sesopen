//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    using global::Fiddler;

    /// <summary>
    /// Base cache type.
    /// </summary>
    internal abstract class FiddlerProxyCache
    {
        /// <summary>
        /// Performs caching operations on a Fiddler session before it sent to the server.
        /// </summary>
        /// <param name="session">The Fiddler session</param>
        public abstract void ProcessRequest(Session session);

        /// <summary>
        /// Performs caching operations on a Fiddler session before it is sent to the caller.
        /// </summary>
        /// <param name="session">The Fiddler session</param>
        public abstract void ProcessResponse(Session session);
    }
}