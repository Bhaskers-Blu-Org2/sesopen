//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    /// <summary>
    /// An unsafe page that has a password input.
    /// </summary>
    public partial class AutocompletePassword : System.Web.UI.Page
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
            Response.Clear();
            Response.ContentType = Request.ContentType;

            Response.Headers.Add("test", "value");

            Response.Write("<html>");
            Response.Write("<input type=\"button\"/>");
            Response.Write("<input type=\"password\" autocomplete=\"true\"/>");
            Response.Write("<input type=\"text\"/>");
            Response.Write("<done>Done!</done>");
            Response.Write("</html>");

            Response.End();
        }
    }
}