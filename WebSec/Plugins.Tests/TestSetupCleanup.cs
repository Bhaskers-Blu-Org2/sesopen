//-----------------------------------------------------------------------
// <copyright file="TestSetupCleanup.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace BackScatterUnitTests
{
    using CommonFx.Tests.Infrastructure;
    using Microsoft.Search.Security.Common;
    using Microsoft.Search.Security.Common.Platform;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public static class TestSetupCleanup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            AssertFailureExceptionFactory.ConfigureForMicrosoftTest();

            ObjectResolver.RegisterInstance<PlatformInitialization>(new NullPlatformInitialization());
        }
    }
}
