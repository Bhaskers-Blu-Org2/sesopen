//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Payloads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Logger;

    /// <summary>
    /// Payloads data loader.
    /// </summary>
    internal sealed class PayloadsDataLoader
    {
        /// <summary>
        /// Full pathname of the payloads data folder file.
        /// </summary>
        private readonly string payloadsDataFolderPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadsDataLoader"/> class.
        /// </summary>
        /// <param name="payloadsDataFolderPath">
        /// Full pathname of the payloads data folder file.
        /// </param>
        internal PayloadsDataLoader(string payloadsDataFolderPath)
        {
            this.payloadsDataFolderPath = payloadsDataFolderPath;
            this.Payloads = new Dictionary<string, IEnumerable<string>>();

            // initialize load payloads
            this.LoadPayloads();
        }

        /// <summary>
        /// Gets the payloads.
        /// </summary>
        /// <value>
        /// The payloads.
        /// </value>
        internal IDictionary<string, IEnumerable<string>> Payloads { get; }

        /// <summary>
        /// Loads the payloads.
        /// </summary>
        internal void LoadPayloads()
        {
            IEnumerable<string> payloadFiles = Directory.EnumerateFiles(this.payloadsDataFolderPath, "*.payload");

            foreach (var payloadFileName in payloadFiles)
            {
                this.ReadPayload(payloadFileName);
            }
        }

        /// <summary>
        /// Read the payload described by payloadFileName.
        /// </summary>
        /// <param name="payloadFileName">
        /// Filename of the payload file.
        /// </param>
        private void ReadPayload(string payloadFileName)
        {
            string payloadFileNameContent = File.ReadAllText(payloadFileName);

            Logger.WriteInfo(
                "Payload file name {0} has been loaded.",
                payloadFileName);

            IEnumerable<string> values = payloadFileNameContent.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !l.StartsWith(";"));

            this.Payloads[PayloadsHelper.PayloadPluginName(new FileInfo(payloadFileName).Name)] = values;
        }
    }
}