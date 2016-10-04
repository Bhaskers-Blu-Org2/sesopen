//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common
{
    using System;

    /// <summary>
    /// An HTML helper.
    /// </summary>
    public static class HtmlHelper
    {
        /// <summary>
        /// Inspects a string and decides if the content is likely to be an Html document.
        /// </summary>
        /// <param name="content">The content to inspect.</param>
        /// <returns>True if the content is Html, otherwise false.</returns>
        public static bool ContentIsHtml(string content)
        {
            return content.StartsWith(Constants.HtmlTag, StringComparison.InvariantCultureIgnoreCase)
                   || content.IndexOf(Constants.IsHtmlDoc, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}