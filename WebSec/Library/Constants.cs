//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library
{
    /// <summary>
    /// The websec constants.
    /// </summary>
    public sealed class Constants
    {
        /// <summary>
        /// The all params.
        /// </summary>
        public const string AllParams = "All params";

        /// <summary>
        /// Html tag.
        /// </summary>
        public const string HtmlTag = "<html>";

        /// <summary>
        /// The HTML document tag.
        /// </summary>
        public const string IsHtmlDoc = "<!DOCTYPE HTML";

        /// <summary>
        /// The post parameter value.
        /// </summary>
        public const string PostParameterValue = "postparametervalue";

        /// <summary>
        /// The x path input text type.
        /// </summary>
        public const string XPathInputTextType = "//input[@type='text']";

        /// <summary>
        /// The fiddler error page.
        /// </summary>
        public const string FiddlerErrorPage = "[Fiddler]";

        /// <summary>
        /// Pathname of the payloads quick data folder.
        /// </summary>
        public const string PayloadsQuickDataFolder = "Payloads\\Quick";

        /// <summary>
        /// Pathname of the payloads full data folder.
        /// </summary>
        public const string PayloadsFullDataFolder = "Payloads\\Full";

        /// <summary>
        /// Path name of the active plugins file.
        /// </summary>
        public const string ActivePluginsFilePath = @"Settings\ActivePlugins.cfg";

        /// <summary>
        /// Name of the plugin DLL.
        /// </summary>
        public const string PluginDllName = "WebSec.Plugins.dll";

        /// <summary>
        /// The fiddler response session key.
        /// </summary>
        public const string FiddlerResponseSessionKey = "{0}{1}";
    }
}