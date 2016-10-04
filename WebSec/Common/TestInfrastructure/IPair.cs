//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    using System;

    /// <summary>
    ///    Denotes a pair of values.
    /// </summary>
    /// <typeparam name = "TFirst">The type of the first value.</typeparam>
    /// <typeparam name = "TSecond">The type of the second value.</typeparam>
    public interface IPair<TFirst, TSecond>
        : ICloneable, IComparable, IComparable<IPair<TFirst, TSecond>>, IEquatable<IPair<TFirst, TSecond>>
    {
        /// <summary>
        ///    Gets or sets the first item in the pair.
        /// </summary>
        TFirst First { get; set; }

        /// <summary>
        ///    Gets or sets the second item in the pair.
        /// </summary>
        TSecond Second { get; set; }
    }
}