//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The target interface. Wrap an Uri and add some helpers.
    /// </summary>
    public interface ITarget
    {
        /// <summary>
        /// Gets the URI.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Gets the URL base.
        /// </summary>
        /// <value>
        /// The URL base.
        /// </value>
        string UrlBase { get; }

        /// <summary>
        /// Gets options for controlling the operation.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IReadOnlyDictionary<string, UrlParameter> Params { get; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        string Content { get; set; }

        /// <summary>
        /// Gets a parameter.
        /// </summary>
        /// <param name="testparam">
        /// The test param.
        /// </param>
        /// <param name="testval">
        /// The testing val.
        /// </param>
        /// <returns>
        /// The parameter.
        /// </returns>
        string GetParam(string testparam, string testval);

        /// <summary>
        /// Gets all parameters.
        /// </summary>
        /// <param name="testval">
        /// The testing val.
        /// </param>
        /// <returns>
        /// all parameters.
        /// </returns>
        string GetAllParams(string testval);
    }
}