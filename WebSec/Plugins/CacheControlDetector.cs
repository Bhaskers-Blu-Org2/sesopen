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
    /// The cache control detector.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Cache Control Detector",
        Description = "Verifies HTTPS pages are not cached")]
    public class CacheControlDetector : PluginBaseAbstract
    {
        /// <summary>
        /// The cache control.
        /// </summary>
        private const string CacheControl = "Cache-Control";

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
            if (response.RequestAbsolutUri.IndexOfOi("https://") != -1
                && response.Headers.IsNotEmpty())
            {
                // Check for cache-control violation in response header.
                string cacheControl = response.Headers[CacheControl];

                if (!string.IsNullOrWhiteSpace(cacheControl))
                {
                    // http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.2
                    if (cacheControl.IndexOfOi("no-store") == -1)
                    {
                        this.AddVulnerability(
                            "HTTPS pages should not be cached",
                            testParameter,
                            testValue,
                            CacheControl + ": " + cacheControl,
                            response,
                            testCase,
                            VulnerabilityLevelEnum.Low);
                    }
                }
            }
        }
    }
}