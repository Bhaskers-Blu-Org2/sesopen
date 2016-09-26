//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;

    /// <summary>
    /// The _500 error page.
    /// </summary>
    public partial class ServerError503 : System.Web.UI.Page
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
            if (string.IsNullOrEmpty(Request.Params["q"]))
            {
                Response.Clear();
                Response.AddHeader("X-Powered-By", "ASP.NET");
                Response.Write("<html>");

                for (int i = 0; i < 100; i++)
                {
                    Response.Write("<dummyContent>dummyContent</dummyContent>");
                }

                Response.Write("</html>");
                Response.End();

                return;
            }

            Response.Clear();
            Response.StatusCode = 503;
            Response.Write("<html>");
            Response.Write("This is a Bad Request, the page just crashed");
            Response.Write("</html>");
            Response.End();
        }
    }
}