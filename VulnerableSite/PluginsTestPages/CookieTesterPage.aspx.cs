//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite.PluginsTestPages
{
    using System;
    using System.Web;

    /// <summary>
    /// The cookie tester page.
    /// </summary>
    public partial class CookieTesterPage : System.Web.UI.Page
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
            if (!string.IsNullOrEmpty(Request.Params["secure"]))
            {
                Response.Clear();
                Response.ClearHeaders();
                var cookie = new HttpCookie("SecuredCookie", "SecuredCookieValue") { Secure = true };
                Response.SetCookie(cookie);
                Response.Write("<html>");
                Response.Write("<dummyContent>dummyContent</dummyContent>");
                Response.Write("</html>");
                Response.End();
                return;
            }

            Response.Clear();
            Response.ClearHeaders();
            Response.SetCookie(new HttpCookie("testcookiekey", "testcookievalue"));
            Response.Write("<html>");
            Response.Write("<dummyContent>dummyContent</dummyContent>");
            Response.Write("</html>");
            Response.End();
        }
    }
}