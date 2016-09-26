//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) a test setup cleanup.
    /// </summary>
    [TestClass]
    [DeploymentItem("chromedriver.exe", "")]
    [DeploymentItem("Payloads", "Payloads")]
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
            TestSetupHelpers.StartIisExpress();
        }
    }
}