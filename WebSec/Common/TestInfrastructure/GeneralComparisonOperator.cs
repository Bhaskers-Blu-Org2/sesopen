//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    /// <summary>
    /// Represents the general types of comparisons the 
    /// system can perform.
    /// </summary>
    public enum GeneralComparisonOperator
    {
        /// <summary>
        /// Represents a Reference Equality comparison.
        /// </summary>
        ReferenceEqual,

        /// <summary>
        /// Represents an Equal comparison.
        /// </summary>
        Equal,

        /// <summary>
        /// Represents a Not Equal comparison.
        /// </summary>
        NotEqual,

        /// <summary>
        /// Represents a Less Than comparison.
        /// </summary>
        LessThan,

        /// <summary>
        /// Represents a Less Than Or Equal comparison.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Represents a Greater Than comparison.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Represents a Greater Than Or Equal comparison.
        /// </summary>
        GreaterThanOrEqual
    }
}