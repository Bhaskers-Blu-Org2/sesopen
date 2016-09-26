//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;

    /// <summary>
    /// The template injection test page.
    /// </summary>
    public partial class TemplateInjectionTestPage : System.Web.UI.Page
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
            if (!string.IsNullOrEmpty(Request.Params["simulateTemplateEngine"]))
            {
                Response.Clear();
                Response.Write("<html>");
                Response.Write("<dummyContent>Hello 49</dummyContent>");
                Response.Write("</html>");
                Response.End();

                return;
            }

            Response.Clear();
            Response.Write("<html>");
            Response.Write("<dummyContent>TemplateInjectionTestPage</dummyContent>");
            Response.Write("</html>");
            Response.End();
        }
    }
}