//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using Common.Validation;
    using OpenQA.Selenium.Chrome;

    /// <summary>
    /// Chrome browser implementation.
    /// </summary>
    public sealed class ChromeBrowser : BrowserAbstract
    {
        /// <summary>
        /// The window title format.
        /// </summary>
        private const string WindowTitleFormat = "{0} - Google Chrome";

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChromeBrowser" /> class.
        /// </summary>
        /// <param name="browser">The Selenium driver to wrap.</param>
        /// <param name="profilePath">The path to the browser profile.</param>
        public ChromeBrowser(ChromeDriver browser, string profilePath)
            : base(browser, WindowTitleFormat)
        {
            Require.NotNull(() => browser);
            Require.NotNullOrEmpty(() => profilePath);

            this.ProfilePath = profilePath;
        }

        /// <inheritdoc/>
        public override BrowserType BrowserType => BrowserType.Chrome;

        /// <inheritdoc/>
        public override string DefaultPage => "data:,";

        /// <inheritdoc/>
        public string ProfilePath { get; }
    }
}