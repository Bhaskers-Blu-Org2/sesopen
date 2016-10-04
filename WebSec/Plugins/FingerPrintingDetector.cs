//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;
   
    /// <summary>
    /// A finger printing detector. Looks for misconfiguration, port exposure and sensitive tokens.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector,
        Name = "Fingerprint Detector",
        Description = "Looks for headers that expose information about the server platform.")]
    public sealed class FingerPrintingDetector : PluginBaseAbstract
    {
        /// <summary>
        /// The token substring.
        /// </summary>
        private const string TokenSubstring = "token";

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
            if (string.IsNullOrEmpty(response?.ResponseContent) || response.Headers == null)
            {
                return;
            }

            bool isHttp = responseTarget.Uri.OriginalString.StartsWith(
                "http://",
                StringComparison.InvariantCultureIgnoreCase);

            for (var i = 0; i < response.Headers.Count; i++)
            {
                string key;
                try
                {
                    key = response.Headers.GetKey(i);
                }
                catch
                {
                    continue;
                }

                // don't consider empty header value
                var value = response.Headers[i];
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                
                this.CheckContentTypeOptionsMisconfiguration(key, value, testParameter, testValue, response, testCase);
                this.CheckXssProtectionMisconfiguration(key, value, testParameter, testValue, response, testCase);
                this.CheckExposedTokens(isHttp, key, value, testParameter, testValue, response, testCase);
                this.CheckPossiblePortsExposure(key, value, testParameter, testValue, response, testCase);
            }
        }

        /// <summary>
        /// The check exposed tokens.
        /// </summary>
        /// <param name="isHttp">
        /// The is http.
        /// </param>
        /// <param name="headerName">
        /// The header name.
        /// </param>
        /// <param name="headerValue">
        /// The header value.
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
        private void CheckExposedTokens(
            bool isHttp,
            string headerName,
            string headerValue,
            string testParameter,
            string testValue,
            HttpWebResponseHolder response,
            TestCase testCase)
        {
            // check if we expose token headers over http
            if (isHttp && headerName.IndexOf(TokenSubstring, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                this.AddVulnerability(
                    "Possible exposed secret over non secure protocol.", 
                    testParameter,
                    testValue,
                    headerName + "=" + headerValue, 
                    response, 
                    testCase, 
                    VulnerabilityLevelEnum.Info);
            }
        }

        /// <summary>
        /// The check xss protection misconfiguration.
        /// </summary>
        /// <param name="headerName">
        /// The header name.
        /// </param>
        /// <param name="headerValue">
        /// The header value.
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
        private void CheckXssProtectionMisconfiguration(
            string headerName,
            string headerValue,
            string testParameter,
            string testValue,
            HttpWebResponseHolder response,
            TestCase testCase)
        {
            if (headerName.IndexOf("x-xss-protection", StringComparison.InvariantCultureIgnoreCase) > -1
                && headerValue.IndexOf("0", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                this.AddVulnerability(
                    this.Name, 
                    testParameter, 
                    testValue, 
                    headerName + "=" + headerValue,
                    response, 
                    testCase, 
                    VulnerabilityLevelEnum.Medium);
            }
        }

        /// <summary>
        /// The check content type options misconfiguration.
        /// </summary>
        /// <param name="headerName">
        /// The header name.
        /// </param>
        /// <param name="headerValue">
        /// The header value.
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
        private void CheckContentTypeOptionsMisconfiguration(
            string headerName,
            string headerValue,
            string testParameter,
            string testValue,
            HttpWebResponseHolder response,
            TestCase testCase)
        {
            if (headerName.IndexOf("x-content-type-options", StringComparison.InvariantCultureIgnoreCase) > -1
                && headerValue.IndexOf("nosniff", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                this.AddVulnerability(
                    this.Name, 
                    testParameter,
                    testValue,
                    $"x-content-type-options={headerValue}", 
                    response, 
                    testCase,
                    VulnerabilityLevelEnum.Low);
            }
        }

        /// <summary>
        /// The check possible ports exposure.
        /// </summary>
        /// <param name="headerName">
        /// The header name.
        /// </param>
        /// <param name="headerValue">
        /// The header value.
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
        private void CheckPossiblePortsExposure(
            string headerName,
            string headerValue,
            string testParameter,
            string testValue,
            HttpWebResponseHolder response,
            TestCase testCase)
        {
            try
            {
                var uri = new Uri(headerValue);

                if (!uri.IsDefaultPort)
                {
                    this.AddVulnerability(
                        "Possible ports exposed.",
                        testParameter, 
                        testValue,
                        $"{headerName}={headerValue}",
                        response, 
                        testCase,
                        VulnerabilityLevelEnum.Low);
                }
            }
            catch (UriFormatException)
            {
            }
        }
    }
}