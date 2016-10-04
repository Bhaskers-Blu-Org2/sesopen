//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using HtmlAgilityPack;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// Detects exchange password over unsecured protocol HTTP.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Password Over Https Vuln Detector",
        Description = "Password Over Https Vuln Detector")]
    public class PasswordOverHttpsDetector : PluginBaseAbstract
    {
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
            var dom = new HtmlDocument();

            try
            {
                dom.LoadHtml(response.ResponseContent);
            }
            catch (Exception)
            {
                // ignore parsing errors - we don't know which errors are possible here.
                return;
            }

            HtmlNodeCollection inputElements = dom.DocumentNode.SelectNodes("//input");

            if (inputElements != null)
            {
                foreach (HtmlNode node in inputElements)
                {
                    string inputType = node.GetAttributeValue("type", "text");

                    if (inputType.Equals("password", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Pages with password fields should be served over HTTPS.
                        if (
                            response.RequestAbsolutUri.IndexOf(
                                "https://",
                                StringComparison.InvariantCultureIgnoreCase) == -1)
                        {
                            this.AddVulnerability(
                                "Password fields should only be served over HTTPS",
                                testParameter,
                                testValue,
                                response.RequestAbsolutUri,
                                response,
                                testCase,
                                VulnerabilityLevelEnum.Low);
                        }
                    }
                }
            }
        }
    }
}