//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    /// <summary>
    /// An unsafe page that has a password input.
    /// </summary>
    public partial class vuln_autocomplete_password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = Request.ContentType;

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