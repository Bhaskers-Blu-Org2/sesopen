//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// Tests for XSS behavior.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Client-side DOM XSS Test",
        Description = "Detects XSS vulnerabilities within client side script.")]
    public class DomXssTest : PluginBaseAbstract
    {
        /// <inheritdoc/>
        public override void Init(IContext context, ITarget target)
        {
            base.Init(context, target);

            TestCases.AddRange(TestBaseHelper.XssTestCases(target, context));
        }

        /// <inheritdoc/>
        protected override void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testcase,
            string testedParam,
            string testValue)
        {
            if (webRequestContext.Browser.AlertMessageDisplayed.Contains(TestBaseHelper.AttackSignature))
            {
                Vulnerabilities.Enqueue(new Vulnerability
                {
                    Title = testcase.TestName,
                    Level = (int)VulnerabilityLevelEnum.High,
                    TestedParam = testedParam,
                    TestedVal = testValue,
                    HttpResponse = webRequestContext.ResponseHolder,
                    Evidence = $"Found by {webRequestContext.Browser.BrowserType}",
                    MatchString = testcase.MatchString,
                    TestPlugin = GetType().Name
                });
            }
        }
    }
}