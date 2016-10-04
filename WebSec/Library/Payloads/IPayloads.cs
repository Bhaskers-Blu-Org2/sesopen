//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Payloads
{
    using System.Collections.Generic;

    /// <summary>
    /// Payloads interface.
    /// </summary>
    public interface IPayloads
    {
        /// <summary>
        /// Loads the payloads.
        /// </summary>
        /// <param name="payloadName">
        /// Name of the payload.
        /// </param>
        /// <returns>
        /// An enumerable of payloads.
        /// </returns>
        IEnumerable<string> LoadPayloads(string payloadName);
    }
}