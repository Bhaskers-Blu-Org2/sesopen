//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;
    using System.Threading;
    using System.Web;

    /// <summary>
    /// Simple page that waits for a period of time.
    /// </summary>
    public partial class DelayContentType : System.Web.UI.Page
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

            Response.Cookies.Add(new HttpCookie("cookie1", this.Request["t"]));
            Response.Cookies.Add(new HttpCookie("cookie2", "hello"));
            Response.ContentType = this.Request["t"];
            Response.Write(this.Request["t"]);
            Response.End();
        }
    }
}