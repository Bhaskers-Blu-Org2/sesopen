//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using Common.TestInfrastructure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// (Unit Test Class) a test setup.
    /// </summary>
    [TestClass]
    [DeploymentItem("chromedriver.exe", "")]
    [DeploymentItem("Payloads", "Payloads")]
    [DeploymentItem("Settings", "Settings")]
    public static class TestSetup
    {
        /// <summary>
        /// Assembly initialize.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            AssertFailureExceptionFactory.ConfigureForMicrosoftTest();
            TestSetupHelpers.CleanSeleniumDriver();
            TestSetupHelpers.StartIisExpress();
        }
    }
}