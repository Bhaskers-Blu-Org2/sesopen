//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace Worker
{
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// A class to receive parsed values
    /// </summary>
    internal class ArgumentOptions
    {
        /// <summary>
        /// Gets or sets the URL input.
        /// </summary>
        /// <value>
        /// The URL input.
        /// </value>
        [Option('u', "url", Required = true,
          HelpText = "A file containing urls or a single url. Each url should be on a different line.")]
        public string UrlInput { get; set; }

        /// <summary>
        /// Gets or sets the active plugins input file.
        /// </summary>
        /// <value>
        /// The active plugins input file.
        /// </value>
        [Option('p', "plugins", Required = false,
          HelpText = "Active plugins configuration file. For configuration format see documentation.")]
        public string ActivePluginsInputFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this object use full payloads.
        /// </summary>
        /// <value>
        /// true if use full payloads, false if not.
        /// </value>
        [Option('r', "payloads", DefaultValue = false, Required = false,
          HelpText = "Use WebSec full payloads. True if use it , false for using a much lighter version of them.")]
        public bool UseFullPayloads { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the verbose.
        /// </summary>
        /// <value>
        /// true if verbose, false if not.
        /// </value>
        [Option('v', "verbose", DefaultValue = true, Required = false,
          HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets the state of the last parser.
        /// </summary>
        /// <value>
        /// The last parser state.
        /// </value>
        [ParserState]
        public IParserState LastParserState { get; set; }

        /// <summary>
        /// Gets the usage.
        /// </summary>
        /// <returns>
        /// The usage.
        /// </returns>
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(
               this,
              current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}