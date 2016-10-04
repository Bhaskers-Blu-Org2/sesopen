﻿//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// An open redirect tester. Check if any params are expose to domain redirection.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Open Redirect",
        Description = "Test for parameters that redirect to other sites")]
    public sealed class OpenRedirectTester : PluginBaseAbstract
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
            TestCases.Add(new TestCase
            {
                TestName = "Open Redirect",
                InjectionString = TestBaseHelper.LoadTestCase("OpenRedirectTester", Context).ToArray()
            });
        }

        /// <summary>
        /// Inside a test check to see if the match string gets found on the response page,regex used for
        /// matching if that is the case then log a vulnerability.
        /// </summary>
        /// <param name="webRequestContext">
        /// The web Request Context.
        /// </param>
        /// <param name="testcase">
        /// The test case.
        /// </param>
        /// <param name="testedParam">
        /// The tested parameter.
        /// </param>
        /// <param name="testValue">
        /// The tested Val.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.CheckForVuln(WebRequestContext,TestCase,string,string)"/>
        protected override void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testcase,
            string testedParam,
            string testValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;

            if (response.ResponseUri != null &&
                response.ResponseUri.Host.IndexOf(response.RequestHost, StringComparison.InvariantCultureIgnoreCase) ==
                -1 &&
                response.RequestHost.IndexOf(response.ResponseUri.Host, StringComparison.InvariantCultureIgnoreCase) ==
                -1 &&
                response.ResponseUri.Host.IndexOf("ikeahackers.net", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                this.AddVulnerability(
                    testcase.TestName,
                    testedParam,
                    testValue,
                    response.ResponseUri.Host,
                    response,
                    testcase,
                    VulnerabilityLevelEnum.High);
            }
        }
    }
}