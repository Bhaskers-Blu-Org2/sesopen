//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    /// <summary>
    /// A page that allows session splitting.
    /// </summary>
    public class vuln_session_splitting : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "text/html";
            Response.Charset = "utf-8";

            if (!string.IsNullOrWhiteSpace(Request.Params["q"]))
            {
                Response.AddHeader("to", Request.Params["q"]);
            }

            Response.Write("<html>");
            Response.Write("<done>Done!</done>");
            Response.Write("</html>");

            Response.End();
        }
    }
}