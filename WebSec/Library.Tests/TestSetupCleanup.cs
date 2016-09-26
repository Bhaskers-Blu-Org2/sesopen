//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) a test setup cleanup.
    /// </summary>
    [TestClass]
    public static class TestSetupCleanup
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
        }
    }
}