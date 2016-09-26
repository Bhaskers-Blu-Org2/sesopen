//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;
    using System.Threading;

    /// <summary>
    /// The sql injection vuln page.
    /// </summary>
    public partial class SqlInjectionVulnPage : System.Web.UI.Page
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
            if (!string.IsNullOrEmpty(Request.Params["delay"]))
            {
                Thread.Sleep(30000);
            }

            if (!string.IsNullOrEmpty(Request.Params["error"]))
            {
                Response.Write("<html>");
                Response.Write("<dummyContent>syntax error</dummyContent>");
                Response.Write("</html>");
                Response.End();
            }
        }
    }
}