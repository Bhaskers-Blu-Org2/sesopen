//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using Common;
    using Common.TestInfrastructure;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Library.Fiddler;

    /// <summary>
    /// A test cleanup.
    /// </summary>
    internal static class TestCleanup
    {
        /// <summary>
        /// Cleanup fiddler and browser instances.
        /// </summary>
        internal static void Cleanup()
        {
            FiddlerProxy.Cleanup(Constants.FiddlerPort);
            ((BrowserManager)ObjectResolver.Resolve<IBrowserManager>()).Dispose();

            TestSetupHelpers.CleanSeleniumDriver();
        }
    }
}