//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------
#pragma warning disable 0436

namespace VulnerableSite.PluginsTestPages
{
    using System;

    /// <content>
    /// An open redirect to bad places http.
    /// </content>
    public partial class XOpenRedirectToBadPlacesHTTP : System.Web.UI.Page
    {
        /// <summary>
        /// Event handler. Called by Page for load events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}