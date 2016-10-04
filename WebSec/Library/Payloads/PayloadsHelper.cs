//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Payloads
{
    using System;
    using System.IO;

    /// <summary>
    /// The payloads helper.
    /// </summary>
    internal static class PayloadsHelper
    {
        /// <summary>
        /// The payload file extension.
        /// </summary>
        private static readonly string PayloadFileExtension = "payload";

        /// <summary>
        /// Payload file name.
        /// </summary>
        /// <param name="pluginName">
        /// Name of the plugin.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        internal static string PayloadFileName(string pluginName)
        {
            return $"{pluginName}.{PayloadFileExtension}";
        }

        /// <summary>
        /// Payload plugin name.
        /// </summary>
        /// <param name="payloadFileName">
        /// Filename of the payload file.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        internal static string PayloadPluginName(string payloadFileName)
        {
            int idx = payloadFileName.IndexOfOi($".{PayloadFileExtension}");

            if (idx == -1)
            {
                return payloadFileName;
            }

            return payloadFileName.Substring(0, idx);
        }

        /// <summary>
        /// Query if 'pluginName' is payload data generated.
        /// </summary>
        /// <param name="payloadsDataFolderPath">
        /// Full pathname of the payloads data folder file.
        /// </param>
        /// <param name="pluginName">
        /// Name of the plugin.
        /// </param>
        /// <returns>
        /// true if payload data generated, false if not.
        /// </returns>
        internal static bool IsPayloadDataGenerated(string payloadsDataFolderPath, string pluginName)
        {
            return File.Exists(Path.Combine(payloadsDataFolderPath, PayloadFileName(pluginName)));
        }
    }
}