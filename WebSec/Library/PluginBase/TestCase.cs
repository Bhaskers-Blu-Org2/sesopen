//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.PluginBase
{
    using System;
    using System.Net;

    /// <summary>
    /// The test case.
    /// </summary>
    public class TestCase
    {
        /// <summary>
        /// Initializes a new instance of the TestCase class.
        /// </summary>
        public TestCase()
        {
            this.InjectionString = new string[] { };
            this.ContentType = null;
            this.SaveRequestCookies = true;
        }

        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        /// <value>
        /// The name of the test.
        /// </value>
        public string TestName { get; set; }

        /// <summary>
        /// Gets or sets the injection string.
        /// </summary>
        /// <value>
        /// The injection string.
        /// </value>
        public string[] InjectionString { get; set; }

        /// <summary>
        /// Gets or sets the match string.
        /// </summary>
        /// <value>
        /// The match string.
        /// </value>
        public string MatchString { get; set; }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the injection cookie call back.
        /// </summary>
        /// <value>
        /// The injection cookie call back.
        /// </value>
        public Func<CookieCollection, CookieCollection> InjectionCookieCallBack { get; set; }

        /// <summary>
        /// Gets or sets the type of the filter response content.
        /// </summary>
        /// <value>
        /// The type of the filter response content.
        /// </value>
        public string[] FilterResponseContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request cookies should be saved.
        /// </summary>
        /// <value>
        /// true if save request cookies, false if not.
        /// </value>
        public bool SaveRequestCookies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fuzz also all parameters at the same time.
        /// </summary>
        /// <value>
        /// true if fuzz also all parameters at the same time, false if not.
        /// </value>
        public bool FuzzAlsoAllParamsAtTheSameTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fuzz only numeric param.
        /// </summary>
        public bool FuzzOnlyNumericParam { get; set; }
    }
}