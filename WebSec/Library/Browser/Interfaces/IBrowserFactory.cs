//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser.Interfaces
{
    /// <summary>
    /// Defines a factory that handles creation of browser instances.
    /// </summary>
    public interface IBrowserFactory
    {
        /// <summary>
        /// Creates a new browser instance.
        /// </summary>
        /// <param name="browserType">The type of browser expected.</param>
        /// <returns>A Task that will create an instance of a browser.</returns>
        BrowserAbstract Create(BrowserType browserType);
    }
}