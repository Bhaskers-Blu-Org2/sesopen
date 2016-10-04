//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Text.RegularExpressions;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;
    
    /// <summary>
    /// The access control has wildcard for domain not in the white list.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Access Control Allow Origin Detector",
        Description = "Access Control Allow Origin Detector")]
    public sealed class AccessControlAllowOriginDetector : PluginBaseAbstract
    {
        /// <summary>
        /// The white list domains.
        /// </summary>
        private string[] whiteListDomains = { "([./-]*)bing.com" };

        /// <summary>
        /// The inspect response.
        /// </summary>
        /// <param name="requestTarget">
        /// The request target.
        /// </param>
        /// <param name="responseTarget">
        /// The response target.
        /// </param>
        /// <param name="response">
        /// The current response.
        /// </param>
        /// <param name="plugIn">
        /// The plug in.
        /// </param>
        /// <param name="testCase">
        /// The test case.
        /// </param>
        /// <param name="testParameter">
        /// The tested parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested val.
        /// </param>
        public override void InspectResponse(
            ITarget requestTarget,
            ITarget responseTarget,
            HttpWebResponseHolder response,
            string plugIn,
            TestCase testCase,
            string testParameter,
            string testValue)
        {
            string accessControlAllowOrigin = response.Headers["Access-Control-Allow-Origin"];

            if (!string.IsNullOrWhiteSpace(accessControlAllowOrigin) &&
                accessControlAllowOrigin.IndexOf("*", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                bool matchFound = false;

                foreach (string allowedDomain in this.whiteListDomains)
                {
                    if (Regex.Match(accessControlAllowOrigin, allowedDomain).Success)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    Vulnerabilities.Enqueue(new Vulnerability
                    {
                        TestPlugin = GetType().Name + " (via " + plugIn + ")",
                        Title = "Access-Control-Allow-Origin has wildcard for domain not in WebSec white list",
                        Level = (int)VulnerabilityLevelEnum.Low,
                        Evidence = accessControlAllowOrigin,
                        HttpResponse = response
                    });
                }
            }
        }
    }
}