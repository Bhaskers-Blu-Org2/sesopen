//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;

    /// <content>
    /// The XSS protection set to zero.
    /// </content>
    public partial class XXssProtectionSetToZero : System.Web.UI.Page
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
            Response.AddHeader("x-xss-protection", "0");
            Response.AddHeader("x-content-type-options", "nosniff");
            Response.Write("<html>");
            Response.Write("<dummyContent>dummyContent</dummyContent>");
            Response.Write("</html>");
            Response.End();
        }
    }
}