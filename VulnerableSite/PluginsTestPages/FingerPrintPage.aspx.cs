//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;
    using System.Web.UI;

    /// <summary>
    /// The finger print page.
    /// </summary>
    public partial class FingerPrintPage : Page
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
            if (!string.IsNullOrEmpty(Request.Params["port"]))
            {
                Response.Clear();
                Response.AddHeader("random", "http://domain.com:12345");
                Response.Write("<html>");
                Response.Write("<dummyContent>dummyContent</dummyContent>");
                Response.Write("</html>");
                Response.End();

                return;
            }

            if (!string.IsNullOrEmpty(Request.Params["portNegative"]))
            {
                Response.Clear();
                Response.AddHeader("random", "http://domain.com");
                Response.Write("<html>");
                Response.Write("<dummyContent>dummyContent</dummyContent>");
                Response.Write("</html>");
                Response.End();

                return;
            }

            if (!string.IsNullOrEmpty(Request.Params["noheaders"]))
            {
                Response.Clear();
                Response.Write("<html>");
                Response.Write("<dummyContent>dummyContent</dummyContent>");
                Response.Write("</html>");
                Response.End();

                return;
            }

            // for this test we will deactivate the X-Powered-By header in web config and emit it only when wanted.
            Response.AddHeader("X-Powered-By", "ASP.NET");
            Response.AddHeader("X-AIS-AuthToken", "test user token id");
            Response.Write("<html>");
            Response.Write("<dummyContent>dummyContent</dummyContent>");
            Response.Write("</html>");
            Response.End();
        }
    }
}