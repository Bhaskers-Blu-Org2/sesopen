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
    /// The browser helper.
    /// </summary>
    public static class BrowserHelper
    {
        /// <summary>
        /// Releases the browser described by webRequestContext.
        /// </summary>
        /// <param name="webRequestContext">
        /// The web request context.
        /// </param>
        public static void ReleaseBrowser(WebRequestContext webRequestContext)
        {
            if (webRequestContext.Browser != null)
            {
                ReleaseBrowser(webRequestContext.Browser);
                webRequestContext.Browser = null;
            }
        }

        /// <summary>
        /// The release browser.
        /// </summary>
        /// <param name="browser">
        /// The browser.
        /// </param>
        public static void ReleaseBrowser(BrowserAbstract browser)
        {
            if (browser != null)
            {
                var browserManager = ObjectResolver.Resolve<IBrowserManager>();
                browserManager.ReleaseBrowser(browser);
            }
        }
    }
}