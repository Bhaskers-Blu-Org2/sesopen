//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using Common;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// A click jacking detector.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector, Name = "ClickJacking detector",
        Description = "Detects if a page is i-frame-able")]
    public sealed class ClickJacking : PluginBaseAbstract
    {
        /// <summary>
        /// The frame options.
        /// </summary>
        private const string FrameOptions = "x-frame-options";

        /// <summary>
        /// The connection.
        /// </summary>
        private const string Connection = "Connection";

        /// <summary>
        /// The fiddler gateway.
        /// </summary>
        private const string FiddlerGateway = "FiddlerGateway";

        /// <summary>
        /// Initializer for the test plugin - the test manager will invoke this at the start of a test
        /// run. Tests that override this method must be sure to call the base.Init function, to ensure
        /// that the base class has the context, target, and results objects created and available, and
        /// if using the default DoTests function would add test case objects to the test cases collection
        /// (which the default DoTests function will use).
        /// </summary>
        /// <param name="currentcontext">
        /// The current context.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.Init(IContext,ITarget)"/>
        public override void Init(IContext currentcontext, ITarget target)
        {
            base.Init(currentcontext, target);
        }

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
            //// quick check for html before invoking a DOM parse
            //// exit if headers are empty or is not http OK status
            if (!HtmlHelper.ContentIsHtml(response.ResponseContent)
                || response.StatusCode != HttpStatusCode.OK
                || response.Headers.IsNullOrEmpty())
            {
                return;
            }

            // if it's fiddler gateway and connection was closed then the request was not completed
            // and only 2 headers are available, one is the fiddler time stamp and the other is connection status.
            // this situation is equivalent with Headers.IsEmpty()
            if (response.Headers.AllKeys.Any(
                k => k.Equals(Connection, StringComparison.InvariantCultureIgnoreCase))
                &&
                response.Headers.AllKeys.Any(
                    k => k.Equals(FiddlerGateway, StringComparison.InvariantCultureIgnoreCase))
                && response.Headers.GetValues(Connection)
                    .Contains("close", StringComparer.InvariantCultureIgnoreCase))
            {
                return;
            }

            var vulnerabilityLevel = response.RequestAbsolutUri.Equals(
                response.ResponseUri.AbsoluteUri,
                StringComparison.InvariantCultureIgnoreCase)
                ? VulnerabilityLevelEnum.High
                : VulnerabilityLevelEnum.Medium;

            // no x-frame-options or Content-Security-Policy found
            if (response.Headers.AllKeys.All(
                k => k.IndexOf(FrameOptions, StringComparison.InvariantCultureIgnoreCase) == -1)
                &&
                response.Headers.AllKeys.All(
                    k => k.IndexOf("Content-Security-Policy", StringComparison.InvariantCultureIgnoreCase) == -1))
            {
                this.AddVulnerability(
                    "No custom headers to protect this target for being i-framed were found.",
                    testParameter,
                    testValue,
                    "Custom headers, suitable for protection, x-frame-options and Content-Security-Policy are missing.",
                    response,
                    testCase,
                    vulnerabilityLevel);

                return;
            }

            // if there is x-frame-options but allows everything then the page can be i-framed
            if (
                response.Headers.AllKeys.Any(
                    k => k.IndexOf(FrameOptions, StringComparison.InvariantCultureIgnoreCase) > -1) &&
                response.Headers.GetValues(FrameOptions)
                    .Any(v => !v.Equals("Deny", StringComparison.InvariantCultureIgnoreCase)
                              && !v.Equals("SAMEORIGIN", StringComparison.InvariantCultureIgnoreCase)
                              && v.IndexOf(responseTarget.Uri.Host, StringComparison.InvariantCultureIgnoreCase) == -1))
            {
                this.AddVulnerability(
                    "Custom header with the wrong value found.",
                    testParameter,
                    testValue,
                    "Custom header x-frame-options, but the value domain is wrong.",
                    response,
                    testCase,
                    vulnerabilityLevel);
            }
        }
    }
}