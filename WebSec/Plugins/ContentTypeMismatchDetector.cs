//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using Common;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;
    
    /// <summary>
    /// A content type mismatch detector.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Content-Type Mismatch Detector",
        Description = "Verifies the declared Content-Type header matches the delivered content type.")]
    public sealed class ContentTypeMismatchDetector : PluginBaseAbstract
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
            if (string.IsNullOrEmpty(response?.ResponseContent))
            {
                return;
            }

            string contentType = response.ResponseContentType;

            // test for empty content type
            if (string.IsNullOrWhiteSpace(contentType))
            {
                this.AddVulnerability(
                    "Content type header is missing.",
                    testParameter,
                    testValue,
                    "Content type header is missing.",
                    response,
                    testCase,
                    VulnerabilityLevelEnum.Low);
                return;
            }

            // test that we don't have utf7 char-set
            if (contentType.IndexOf("utf-7", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                this.AddVulnerability(
                    "Charset of the page should not be utf-7.",
                    testParameter,
                    testValue,
                    response.Headers.ToString(),
                    response,
                    testCase,
                    VulnerabilityLevelEnum.Low);
            }

            // test if the page is html and the content type is not text/html
            if (HtmlHelper.ContentIsHtml(response.ResponseContent) &&
                contentType.IndexOfOi("text/html") == -1 && contentType.IndexOfOi("application / xhtml + xml") == -1)
            {
                this.AddVulnerability(
                    "Html content type mismatched.",
                    testParameter,
                    testValue,
                    response.Headers.ToString(),
                    response,
                    testCase,
                    VulnerabilityLevelEnum.Low);
            }
        }
    }
}