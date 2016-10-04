//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;

    /// <content>
    /// A safe javascript.
    /// </content>
    public partial class XSafeJavascript : System.Web.UI.Page
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
            Response.Write("<html>");
            Response.Write("<dummyContent>dummyContent</dummyContent>");
            Response.Write("<script>alert(\"some javascript\");</script>");
            Response.Write("</html>");
            Response.End();
        }
    }
}