//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System.Text;
    using System.Text.RegularExpressions;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// A directory browsing detector. This shouldn't be active on the server side.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Directory Browsing Detector",
        Description = "Verifies whether directory browsing is enabled for a site.")]
    public sealed class DirectoryBrowsingDetector : PluginBaseAbstract
    {
        /// <summary>
        /// Matches for the tail end of a folder row e.g 6/17/2014 12:36 PM
        /// <A HREF="/API/">API</A>
        /// </summary>
        private const string FolderPattern = @"(AM|PM)\s*&lt;dir&gt;\s*\<A\s+HREF="".+?""\>.+?\</A\>";

        /// <summary>
        /// Matches for the tail end of a file row e.g. 5/12/2014  8:25 AM         2213
        /// <A HREF="/Web.config">Web.config</A>
        /// </summary>
        private const string FilePattern = @"(AM|PM)\s*\d+\s*\<A\s+HREF="".+?""\>.+?\</A\>";

        /// <summary>
        /// Inspects the page response content.
        /// </summary>
        /// <param name="requestTarget">
        /// The request Target.
        /// </param>
        /// <param name="responseTarget">
        /// The response Target.
        /// </param>
        /// <param name="response">
        /// The current Response.
        /// </param>
        /// <param name="plugIn">
        /// The plug In.
        /// </param>
        /// <param name="testCase">
        /// The test Case.
        /// </param>
        /// <param name="testParameter">
        /// The tested Parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested Val.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.InspectResponse(ITarget,ITarget,HttpWebResponseHolder,string,TestCase,string,string)"/>
        public override void InspectResponse(
            ITarget requestTarget,
            ITarget responseTarget,
            HttpWebResponseHolder response,
            string plugIn,
            TestCase testCase,
            string testParameter,
            string testValue)
        {
            var evidence = new StringBuilder();

            foreach (Match e in Regex.Matches(response.ResponseContent, FolderPattern, RegexOptions.IgnoreCase))
            {
                evidence.AppendLine(e.Value);
            }

            foreach (Match e in Regex.Matches(response.ResponseContent, FilePattern, RegexOptions.IgnoreCase))
            {
                evidence.AppendLine(e.Value);
            }

            if (evidence.Length > 0)
            {
                this.AddVulnerability(
                    "Information Leakage - Directory Browsing Is Enabled",
                    testParameter,
                    testValue,
                    evidence.ToString(),
                    response,
                    testCase,
                    VulnerabilityLevelEnum.High);
            }
        }
    }
}