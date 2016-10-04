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
    /// The access control allow methods other than GET and POST.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "Access Control Allow Methods Detector",
        Description = "Access Control Allow Methods Detector")]
    public sealed class AccessControlAllowMethodsDetector : PluginBaseAbstract
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
            string accessControlAllowMethods = response.Headers["Access-Control-Allow-Methods"];

            if (!string.IsNullOrWhiteSpace(accessControlAllowMethods) &&
                accessControlAllowMethods.ContainsAnyOi(new[] { "put", "delete", "options" }))
            {
                Vulnerabilities.Enqueue(new Vulnerability
                {
                    TestPlugin = GetType().Name + " (via " + plugIn + ")",
                    Title = "Access-Control-Allow-Methods allows methods other than GET and POST",
                    Level = (int)VulnerabilityLevelEnum.Low,
                    Evidence = accessControlAllowMethods,
                    HttpResponse = response
                });
            }
        }
    }
}