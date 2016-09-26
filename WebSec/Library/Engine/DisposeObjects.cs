//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using Browser;
    using Browser.Interfaces;
    using Common;

    /// <summary>
    /// Dispose objects.
    /// </summary>
    public static class DisposeObjects
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public static void Dispose()
        {
            ((BrowserManager)ObjectResolver.Resolve<IBrowserManager>()).Dispose();
        }
    }
}