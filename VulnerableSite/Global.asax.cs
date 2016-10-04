//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace VulnerableSite
{
    using System;
    using System.Web;

    /// <content>
    /// A global.
    /// </content>
    public partial class Global : HttpApplication
    {
        /// <summary>
        /// Event handler. Called by Application for start events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        public void Application_Start(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Event handler. Called by Application for end events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        public void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
        }

        /// <summary>
        /// Event handler. Called by Application for error events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        public void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        /// <summary>
        /// Application pre send request headers.
        /// </summary>
        protected void Application_PreSendRequestHeaders()
        {
            if (Request.RawUrl.Contains("PluginsTestPages/FingerPrintPage.aspx?noheaders"))
            {
                // The "Server" header cannot be removed via web.config, so might as well remove all fingerprint headers from one location
                Response.Headers.Remove("Server");
                Response.Headers.Remove("X-Powered-By");
                Response.Headers.Remove("X-AspNet-Version");
                Response.Headers.Remove("X-AspNetMvc-Version");
            }
        }
    }
}