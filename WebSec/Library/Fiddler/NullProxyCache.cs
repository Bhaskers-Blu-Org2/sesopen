//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    using global::Fiddler;

    /// <summary>
    /// Null cache implementation.
    /// </summary>
    internal class NullProxyCache : FiddlerProxyCache
    {
        /// <inheritdoc/>
        public override void ProcessRequest(Session session)
        {
            // does nothing.
        }

        /// <inheritdoc/>
        public override void ProcessResponse(Session session)
        {
            // does nothing.
        }
    }
}