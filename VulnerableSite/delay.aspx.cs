//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;
    using System.Threading;

    /// <summary>
    /// Simple page that waits for a period of time.
    /// </summary>
    public partial class Delay : System.Web.UI.Page
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
            int delay;
            if (int.TryParse(this.Request["a"], out delay))
            {
                Thread.Sleep(delay * 1000);
            }

            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Charset = "utf-8";
            Response.Write("<done>Done!</done>");
            Response.End();
        }
    }
}