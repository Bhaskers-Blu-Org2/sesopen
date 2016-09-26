//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;

    /// <summary>
    /// The click jacking test page.
    /// </summary>
    public partial class ClickjackingTestPage : System.Web.UI.Page
    {
        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write("<html>");
            Response.Write("<dummyContent>dummyContent</dummyContent>");
            Response.Write("</html>");

            if (this.Request["wrongProtection"] != null)
            {
                Response.ClearHeaders();
                Response.AddHeader("x-frame-options", "ALLOW-FROM http://otherdomain.com");
                Response.End();
            }

            if (this.Request["protection"] != null)
            {
                Response.ClearHeaders();
                Response.AddHeader("x-frame-options", "DENY");
                Response.End();
            }
        }
    }
}