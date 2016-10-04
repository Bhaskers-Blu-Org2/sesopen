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
    /// The cache control meta detector.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Cache Control Detector in Meta tags",
        Description = "Cache Control Detector in Meta tags")]
    public sealed class CacheControlMetaDetector : PluginBaseAbstract
    {
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

            if (response.RequestAbsolutUri.IndexOf("https://", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                // Check for cache-control violation in HTML meta elements.
                HtmlNodeCollection metaElements = dom.DocumentNode.SelectNodes("//meta");

                if (metaElements != null)
                {
                    foreach (HtmlNode metaElement in metaElements)
                    {
                        string httpEquiv = metaElement.GetAttributeValue("http-equiv", string.Empty).Trim();

                        if (httpEquiv.Equals("cache-control", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string content = metaElement.GetAttributeValue("content", string.Empty).Trim();

                           if (content.IndexOfOi("no-store") == -1
                                && content.IndexOfOi("no-cache") == -1
                                && content.IndexOfOi("max-age") == -1
                                && content.IndexOfOi("private") == -1)
                            {
                                this.AddVulnerability(
                                    "HTTPS pages should not be cached",
                                    testParameter,
                                    testValue,
                                    metaElement.OuterHtml,
                                    response,
                                    testCase,
                                    VulnerabilityLevelEnum.Low);

                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}