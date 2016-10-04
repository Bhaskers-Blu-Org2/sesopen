//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Logger;
    using Library.PluginBase;

    /// <summary>
    /// An information leakage detector. Looks for Ip/Path exposure,
    ///  and exchange secured cookies over unsecured protocols(http).
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(
        TestBaseType = TestBaseType.Detector,
        Name = "Information Leakage Detector",
        Description = "Checks for IP address or internal file path leakages.")]
    public sealed class InformationLeakageDetector : PluginBaseAbstract
    {
        /// <summary>
        /// The fiddler error page.
        /// </summary>
        private const string FiddlerErrorPage = "[Fiddler]";

        /// <summary>
        /// The minimum folder name length.
        /// </summary>
        private const short MinimumFolderNameLength = 4;

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
            // if this is a fiddler error page just return, nothing to test for
            if (IsFiddlerErrorPage(response.ResponseContent))
            {
                Logger.WriteError("Fiddler error {0}", response.ResponseContent);
                return;
            }

            // ipv4 and ipv6 match
            var addressMatch = Regex.Matches(
                response.ResponseContent,
                @"(127\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})|(192\.168\.[0-9]{1,3}\.[0-9]{1,3})|(10\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})|(172\.1[6-9]\.[0-9]{1,3}\.[0-9]{1,3})|(172\.2[0-9]\.[0-9]{1,3}\.[0-9]{1,3})|(^172\.3[0-1]\.[0-9]{1,3}\.[0-9]{1,3})",
                RegexOptions.IgnoreCase);

            if (addressMatch.Count > 0)
            {
                var evidence = new StringBuilder();
                foreach (Match match in addressMatch)
                {
                    // the class IPAdress allows addresses like "::" to pass as valid ip, protect against these cases
                    if (!CheckIpValidity(match.Value))
                    {
                        continue;
                    }

                    // The regex patterns are a rough comparison, let the IPAddress parser make the final call
                    IPAddress address;
                    if (IPAddress.TryParse(match.Value, out address))
                    {
                        evidence.AppendLine(match.Value);
                    }
                }

                if (evidence.Length > 0)
                {
                    this.AddVulnerability(
                        "Information Leakage - IP Address Disclosure",
                        testParameter,
                        testValue,
                        evidence.ToString(),
                        response,
                        testCase,
                        VulnerabilityLevelEnum.Low);
                }
            }

            var filePathMatch = Regex.Matches(
                response.ResponseContent,
                @"([A-Z]{1}:\\[A-Z0-9\\\.]{0,})|(\\\\[A-Z0-9_-]{1,}[A-Z0-9\\\.]{0,})",
                RegexOptions.IgnoreCase); // UNC

            if (filePathMatch.Count > 0)
            {
                var evidence = new StringBuilder();
                foreach (Match match in filePathMatch)
                {
                    // check UNC path validity
                    if (!CheckPathValidity(match.Value))
                    {
                        continue;
                    }

                    evidence.AppendLine(match.Value);
                }

                if (evidence.Length > 0)
                {
                    this.AddVulnerability(
                        "Information Leakage - File Path Disclosure",
                        testParameter,
                        testValue,
                        evidence.ToString(),
                        response,
                        testCase,
                        VulnerabilityLevelEnum.Low);
                }
            }

            // check for exchange secure cookies over http
            this.CheckSecureCookiesOverHttp(responseTarget, testParameter, testValue, response, testCase);

            this.CheckAuthInformationDisclosure(testParameter, testValue, response, testCase);
        }

        /// <summary>
        /// The check path validity.
        /// </summary>
        /// <param name="evidence">
        /// The evidence.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool CheckPathValidity(string evidence)
        {
            try
            {
                var uri = new Uri(evidence);

                var tokens = evidence.Split(new[] { ':', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.All(t => t.Length < MinimumFolderNameLength))
                {
                    return false;
                }

                // if it's a network path then look to start with a machine name and ignore if the name length is one or two chars
                if (evidence.StartsWith(@"\\") && tokens[0].Length < MinimumFolderNameLength)
                {
                    return false;
                }

                // true if is an UNC path or an absolute file path
                return uri.IsUnc || (uri.IsAbsoluteUri && uri.IsFile);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The is fiddler error page.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool IsFiddlerErrorPage(string response)
        {
            if (response.IndexOfOi(FiddlerErrorPage) > -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The check ip validity.
        /// </summary>
        /// <param name="possibleIp">
        /// The possible ip.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool CheckIpValidity(string possibleIp)
        {
            // check if starts with ::. in which case is not an ip
            if (possibleIp.StartsWith("::", StringComparison.InvariantCultureIgnoreCase)
                || possibleIp.StartsWith("0", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // add more cases here
            return true;
        }

        /// <summary>
        /// The check secure cookies over http.
        /// </summary>
        /// <param name="responseTarget">
        /// The response target.
        /// </param>
        /// <param name="testParameter">
        /// The tested parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested val.
        /// </param>
        /// <param name="response">
        /// The current response.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        private void CheckSecureCookiesOverHttp(
            ITarget responseTarget,
            string testParameter,
            string testValue,
            HttpWebResponseHolder response,
            TestCase testCase)
        {
            bool isHttp = responseTarget.Uri.OriginalString.StartsWith(
                "http://",
                StringComparison.InvariantCultureIgnoreCase);

            if (!isHttp || response.Cookies == null)
            {
                return;
            }

            foreach (Cookie cookie in response.Cookies.Cast<Cookie>().Where(cookie => cookie.Secure))
            {
                this.AddVulnerability(
                    "Information Leakage - Secured cookie exchanged over non-secure http.",
                    testParameter,
                    testValue,
                    cookie.Name,
                    response,
                    testCase,
                    VulnerabilityLevelEnum.Info);
            }
        }

         /// <summary>
        /// Check authentication information disclosure.
        /// </summary>
        /// <param name="testParameter">
        /// The tested Parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested Val.
        /// </param>
        /// <param name="response">
        /// The current Response.
        /// </param>
        /// <param name="testCase">
        /// The test Case.
        /// </param>
        private void CheckAuthInformationDisclosure(
            string testParameter,
            string testValue,
            HttpWebResponseHolder response,
            TestCase testCase)
        {
            if (string.IsNullOrEmpty(response.ResponseContent))
            {
                return;
            }

            if (response.ResponseContent.IndexOfOi("username") > -1)
            {
                 this.AddVulnerability(
                        "Possible user name information disclosure found.",
                        testParameter, 
                        testValue,
                        string.Empty,
                        response, 
                        testCase,
                        VulnerabilityLevelEnum.Low);
            }

             if (response.ResponseContent.IndexOfOi("password") > -1)
            {
                 this.AddVulnerability(
                        "Possible password information disclosure found.",
                        testParameter, 
                        testValue,
                        string.Empty,
                        response, 
                        testCase,
                        VulnerabilityLevelEnum.Medium);
            }
        }
    }
}