//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Payloads
{
    using System.Collections.Generic;
    using Common.Validation;

    /// <summary>
    /// Wrapper for the payloads(attacks signatures). This class cannot be inherited.
    /// </summary>
    public sealed class Payloads : IPayloads
    {
        /// <summary>
        /// The payloads data loader.
        /// </summary>
        private readonly PayloadsDataLoader payloadsDataLoader;

        /// <summary>
        /// Initializes a new instance of the WebSec.Library.Payloads.Payloads class.
        /// </summary>
        /// <param name="payloadsDataFolderPath">
        /// Full pathname of the payloads data folder file.
        /// </param>
        public Payloads(string payloadsDataFolderPath)
        {
            payloadsDataLoader = new PayloadsDataLoader(payloadsDataFolderPath);
        }

        /// <summary>
        /// Loads the payloads.
        /// </summary>
        /// <param name="payloadName">
        /// Name of the payload.
        /// </param>
        /// <returns>
        /// An enumerable of payloads.
        /// </returns>
        public IEnumerable<string> LoadPayloads(string payloadName)
        {
            Require.NotNullOrEmpty(() => payloadName);

            return payloadsDataLoader.Payloads[payloadName];
        }
    }
}