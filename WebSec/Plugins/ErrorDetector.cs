//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// This detectors looks for internal server errors (server side runtime errors), or .Net exceptions manifested in the page.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector,
        Name = "ErrorDetector",
        Description = "Detects server side runtime errors on a page.")]
    public sealed class ErrorDetector : PluginBaseAbstract
    {
        /// <summary>
        /// The status codes.
        /// </summary>
        private static readonly HttpStatusCode[] StatusCodes =
        {
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.InternalServerError
        };

        /// <summary>
        /// The dot net exceptions.
        /// </summary>
        private static readonly string[] DotNetExceptions =
        {
            "System.Exception",
            "System.IndexOutOfRangeException",
            "System.ArgumentException",
            "System.InvalidOperationException"
        };

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
            string evidence = CheckResponseStatus(response);
            if (!string.IsNullOrEmpty(evidence))
            {
                this.AddVulnerability(
                    "Server Error",
                    testParameter,
                    testValue,
                    evidence,
                    response,
                    testCase,
                    VulnerabilityLevelEnum.Medium);
            }

            // check .net exceptions, in case the response is empty than return
            if (string.IsNullOrEmpty(response.ResponseContent))
            {
                return;
            }

            evidence = CheckDotNetError(response);
            if (!string.IsNullOrEmpty(evidence))
            {
                this.AddVulnerability(
                    ".Net Exception",
                    testParameter,
                    testValue,
                    evidence,
                    response,
                    testCase,
                    VulnerabilityLevelEnum.Medium);
            }
        }

        /// <summary>
        /// Check response status.
        /// </summary>
        /// <param name="currentresponse">
        ///     The current response.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        private static string CheckResponseStatus(HttpWebResponseHolder currentresponse)
        {
            if (StatusCodes.Contains(currentresponse.StatusCode))
            {
                return "HTTP Status: " + currentresponse.StatusCode;
            }

            return string.Empty;
        }

        /// <summary>
        /// Check dot net error.
        /// </summary>
        /// <param name="currentresponse">
        ///     The current response.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        private static string CheckDotNetError(HttpWebResponseHolder currentresponse)
        {
            foreach (var dotNetException in DotNetExceptions
                .Where(
                    dotNetException =>
                        currentresponse.ResponseContent.IndexOf(
                            dotNetException,
                            StringComparison.InvariantCultureIgnoreCase) > -1))
            {
                return dotNetException;
            }

            return string.Empty;
        }
    }
}