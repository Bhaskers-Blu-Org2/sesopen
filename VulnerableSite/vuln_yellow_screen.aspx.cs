//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;

    public partial class vuln_yellow_screen : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            throw new InvalidOperationException("bang");
        }
    }
}