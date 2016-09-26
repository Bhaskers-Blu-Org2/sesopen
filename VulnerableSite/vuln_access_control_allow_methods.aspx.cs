//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    public partial class vuln_access_control_allow_methods : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Write("<!DOCTYPE html>");

            if (Request.Params["q"] != null)
            {
                Response.AddHeader("Access-Control-Allow-Methods", Request.Params["q"]);
            }

            Response.Write("<done>Done!</done>");
            Response.Write("</html>");

            Response.End();
        }
    }
}