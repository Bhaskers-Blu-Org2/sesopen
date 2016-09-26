//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// A silverlight cross domain trust.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Silverlight Cross Domain Trust",
        Description = "Looks for cross domain trust issues in Silverlight policy files.")]
    public sealed class SilverlightCrossDomainTrust : PluginBaseAbstract
    {
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
        /// Executes the tests.
        /// </summary>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.DoTests()"/>
        public override void DoTests()
        {
            var newUrlCrossDomain = TestTarget.Uri.GetLeftPart(UriPartial.Authority) + "/" + "clientaccesspolicy.xml";

            var webRequestContext =
                Context.SendRequest(new ContextSendRequestParameter { Url = newUrlCrossDomain, AllowRedirect = false });

            try
            {
                HttpWebResponseHolder responseDomain = webRequestContext.ResponseHolder;

                if (string.IsNullOrEmpty(responseDomain.ResponseContent) ||
                    responseDomain.StatusCode != HttpStatusCode.OK)
                {
                    Context.ReleaseBrowser(webRequestContext.Browser);
                    return;
                }

                var root = XElement.Parse(responseDomain.ResponseContent);

                // Check CrossDomain.xml should not contain a global wildcard character.
                var badDomains =
                    root.Descendants().Where(e => e.Attribute("uri") != null && e.Attribute("uri").Value == "*");

                foreach (var e in badDomains)
                {
                    this.AddVulnerability(
                        "Trust of all domains",
                        string.Empty,
                        string.Empty,
                        e.ToString(),
                        responseDomain,
                        null,
                        VulnerabilityLevelEnum.Medium);
                }
            }
            finally
            {
                Context.ReleaseBrowser(webRequestContext.Browser);
            }
        }
    }
}