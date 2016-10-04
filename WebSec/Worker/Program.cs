//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace Worker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using WebSec.Library.Engine;
    using WebSec.Library.Logger;

    /// <summary>
    /// Main class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry-point for this application.
        /// </summary>
        /// <param name="args">
        /// Array of command-line argument strings.
        /// </param>
        internal static void Main(string[] args)
        {
            var argumentOptions = new ArgumentOptions();

            if (!CommandLine.Parser.Default.ParseArguments(args, argumentOptions))
            {
                Console.WriteLine($"Invalid command line arguments! Usage : {argumentOptions.GetUsage()}");
                return;
            }

            var urls = new List<string>();

            if (File.Exists(argumentOptions.UrlInput))
            {
                urls.AddRange(
                    File.ReadAllText(argumentOptions.UrlInput)
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                urls.Add(argumentOptions.UrlInput);
            }

            IEnumerable<Vulnerability> vulnerabilities = Engine.StartEngine(
                urls,
                LoadActivePlugins(argumentOptions.ActivePluginsInputFile),
                argumentOptions.UseFullPayloads,
                argumentOptions.Verbose);

            LogVulnerabilities(vulnerabilities);
        }

        /// <summary>
        /// Loads active plugins.
        /// </summary>
        /// <param name="activePluginsInputFile">
        /// The active plugins input file.
        /// </param>
        /// <returns>
        /// The active plugins.
        /// </returns>
        private static IReadOnlyDictionary<string, bool> LoadActivePlugins(string activePluginsInputFile)
        {
            if (string.IsNullOrEmpty(activePluginsInputFile) || !File.Exists(activePluginsInputFile))
            {
                return new Dictionary<string, bool>();
            }

            var ret = new Dictionary<string, bool>();

            string[] activePlugins = File.ReadAllText(activePluginsInputFile)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var activePlugin in activePlugins)
            {
                string[] pluingState = activePlugin.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (pluingState.Length != 2)
                {
                    continue;
                }

                ret[pluingState[0]] = bool.Parse(pluingState[1]);
            }

            return ret;
        }

        /// <summary>
        /// Logs the vulnerabilities.
        /// </summary>
        /// <param name="vulnerabilities">
        /// The vulnerabilities.
        /// </param>
        private static void LogVulnerabilities(IEnumerable<Vulnerability> vulnerabilities)
        {
            Logger.WriteInfo("-----------------------------------------");
            Logger.WriteInfo("Found vulnerabilities");

            foreach (Vulnerability vulnerability in vulnerabilities)
            {
                Logger.WriteInfo(
                    $"{vulnerability.Title}\t{vulnerability.TestPlugin}\t{vulnerability.HttpResponse.RequestAbsolutUri}");
            }
        }
    }
}