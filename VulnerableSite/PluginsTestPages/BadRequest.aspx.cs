//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;
    using System.Web;

    /// <content>
    /// Returns bad request.
    /// </content>
    public partial class BadRequest : System.Web.UI.Page
    {
        /// <summary>
        /// Event handler. Called by Page for load events.
        /// </summary>
        /// <exception cref="HttpException">
        /// Thrown when a HTTP error condition occurs.
        /// </exception>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            throw new HttpException(400, "This is a Bad Request");
        }
    }
}