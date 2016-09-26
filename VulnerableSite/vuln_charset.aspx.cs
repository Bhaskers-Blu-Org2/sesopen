//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    /// <summary>
    /// A page that mimics the requested content-type.
    /// </summary>
    public partial class vuln_charset : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = Request.ContentType;

            Response.Write("<html>");

            if (Response.ContentType.IndexOf("utf-7", StringComparison.OrdinalIgnoreCase) != -1 &&
                Request.Params["q"] != null)
            {
                Response.Write("<div>" + Request.Params["q"] + "</div>");
            }

            Response.Write("<done>Done!</done>");
            Response.Write("</html>");

            Response.End();
        }
    }
}