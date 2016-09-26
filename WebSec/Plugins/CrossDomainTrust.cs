//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using System.Xml.Linq;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Logger;
    using Library.PluginBase;

    /// <summary>
    /// The cross domain trust test.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Flash Cross Domain Trust",
        Description = "Looks for cross domain trust issues in Adobe Flash policy files.")]
    public sealed class CrossDomainTrust : PluginBaseAbstract
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
            var newUrlCrossDomain = TestTarget.Uri.GetLeftPart(UriPartial.Authority) + "/" + "crossdomain.xml";

            WebRequestContext webRequestContext =
                Context.SendRequest(new ContextSendRequestParameter { Url = newUrlCrossDomain, AllowRedirect = false });

            HttpWebResponseHolder responseDomain = webRequestContext.ResponseHolder;

            if (string.IsNullOrEmpty(responseDomain.ResponseContent) || responseDomain.StatusCode != HttpStatusCode.OK)
            {
                Context.ReleaseBrowser(webRequestContext.Browser);
                return;
            }

            try
            {
                var root = XElement.Parse(responseDomain.ResponseContent);

                // Check CrossDomain.xml should not contain a global wild-card character.
                var badDomains =
                    root.Descendants()
                        .Where(e => e.Attribute("domain") != null && e.Attribute("domain").Value == "*");
                foreach (var e in badDomains)
                {
                    this.AddVulnerability(
                        "Trust of all domains",
                        string.Empty,
                        string.Empty,
                        e.ToString(),
                        responseDomain,
                        null,
                        VulnerabilityLevelEnum.High);
                }

                // Check if CrossDomain.xml contains the secured=true attribute
                var insecure =
                    root.Descendants()
                        .Where(
                            e =>
                                e.Attribute("secure") != null &&
                                e.Attribute("secure")
                                    .Value.Equals("false", StringComparison.InvariantCultureIgnoreCase));

                foreach (var e in insecure)
                {
                    this.AddVulnerability(
                        "Insecure trust with a domain",
                        string.Empty,
                        string.Empty,
                        e.ToString(),
                        responseDomain,
                        null,
                        VulnerabilityLevelEnum.High);
                }
            }
            catch (XmlException ex)
            {
                Logger.WriteWarning(ex);
            }
            finally
            {
                Context.ReleaseBrowser(webRequestContext.Browser);
            }
        }
    }
}