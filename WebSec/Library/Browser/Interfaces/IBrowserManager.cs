//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a factory that handles the browser pool.
    /// </summary>
    public interface IBrowserManager
    {
        /// <summary>
        /// Gets the collection of browser types that are available in the pool.
        /// </summary>
        /// <value>
        /// A list of types of the available browsers.
        /// </value>
        IEnumerable<BrowserType> AvailableBrowserTypes { get; }

        /// <summary>
        /// Gets the count of browsers available for work.
        /// </summary>
        /// <value>
        /// The available browsers.
        /// </value>
        int AvailableBrowsers { get; }

        /// <summary>
        /// Returns a browser instance to the browser pool.
        /// </summary>
        /// <param name="browser">
        ///     The browser to return.
        /// </param>
        void ReleaseBrowser(BrowserAbstract browser);

        /// <summary>
        /// Gets the count of browsers available for work by browser type.
        /// </summary>
        /// <param name="browserType">
        ///     browser type.
        /// </param>
        /// <returns>
        /// The available browsers by type.
        /// </returns>
        int GetAvailableBrowsersByType(BrowserType browserType);

        /// <summary>
        /// Ensures that each browser pool has the expected number of instances.
        /// </summary>
        void CreateBrowserInstances();

        /// <summary>
        /// Fetches a browser instance from the browser pool.
        /// </summary>
        /// <param name="browserType">
        ///     The variety of browser to return.
        /// </param>
        /// <returns>
        /// A Task that supplies browser instance, or null if no browsers are available within the
        /// timeout period.
        /// </returns>
        BrowserAbstract AcquireBrowser(BrowserType browserType);
    }
}