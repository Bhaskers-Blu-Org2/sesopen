//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    /// <summary>
    /// An URL parameter.
    /// </summary>
    public sealed class UrlParameter
    {
        /// <summary>
        /// Gets or sets the Parameter value.
        /// </summary>
        /// <value>
        /// The parameter value.
        /// </value>
        public string ParameterValue { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ParameterValue;
        }
    }
}