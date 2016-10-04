//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    /// <summary>
    /// Simple page that responds with XML content.
    /// </summary>
    public partial class XmlResponse : System.Web.UI.Page
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
            Response.ContentType = "text/xml";
            Response.Charset = "utf-8";
            Response.Write("<root>content</root>");
            Response.End();
        }
    }
}