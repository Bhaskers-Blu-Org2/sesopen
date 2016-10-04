//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;

    /// <summary>
    /// This factory is responsible for the creation of browser instances.
    /// </summary>
    /// <seealso cref="T:Microsoft.Search.Security.Common.BrowserPool.Interfaces.IBrowserFactory"/>
    public sealed class DefaultBrowserFactory : IBrowserFactory
    {
        /// <inheritdoc/>
        public BrowserAbstract Create(BrowserType browserType)
        {
            switch (browserType)
            {
                case BrowserType.Chrome:
                {
                    return CreateChromeBrowserInstance();
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null);
            }
        }

        /// <summary>
        /// The create chrome browser instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     InvalidOperationException exception.
        /// </exception>
        /// <returns>
        /// The <see cref="BrowserAbstract"/>.
        /// </returns>
        private static BrowserAbstract CreateChromeBrowserInstance()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-xss-auditor");
            options.AddArgument("--no-experiments");
            options.AddArgument("--disable-translate");
            options.AddArgument("--disable-plugins");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--no-default-browser-check");
            options.AddArgument("--clear-token-service");
            options.AddArgument("--disable-default-apps");
            options.AddArgument("--enable-logging");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--allow-file-access-from-files");
            options.AddArgument("--enable-file-cookies");

            var driver = new ChromeDriver(options);

            var capabilities =
                driver.CastTo<RemoteWebDriver>().Capabilities.GetCapability("chrome") as IDictionary<string, object>;

            if (capabilities == null)
            {
                throw new InvalidOperationException("Unable to get capabilities for Chrome.");
            }

            var browser = new ChromeBrowser(driver, capabilities["userDataDir"] as string);

            return browser;
        }
    }
}