//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace System
{
    using Collections;
    using Collections.Generic;
    using Linq;

    /// <summary>
    ///    Extension methods to Enumerable Types.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///    Determines whether the specified enumerable is empty.
        /// </summary>
        /// <param name = "thisValue">The enumerable.</param>
        /// <returns>
        ///    <c>true</c> if the specified enumerable is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(this IEnumerable thisValue)
        {
            return !thisValue.IsNull() && !thisValue.Cast<object>().Any();
        }

        /// <summary>
        ///    Determines whether the specified enumerable is empty.
        /// </summary>
        /// <param name = "thisValue">The enumerable.</param>
        /// <returns>
        ///    <c>true</c> if the specified enumerable is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEmpty(this IEnumerable thisValue)
        {
            return thisValue.IsNull() || thisValue.Cast<object>().Count() != 0;
        }

        /// <summary>
        ///    Determines whether the specified enumerable is not null or empty.
        /// </summary>
        /// <param name = "thisValue">The enumerable.</param>
        /// <returns>
        ///    <c>true</c> if the specified enumerable is not null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNullOrEmpty(this IEnumerable thisValue)
        {
            return thisValue.IsNotNull() && thisValue.Cast<object>().Count() != 0;
        }

        /// <summary>
        ///    Determines whether the specified enumerable is null or empty.
        /// </summary>
        /// <param name = "thisValue">The enumerable.</param>
        /// <returns>
        ///    <c>true</c> if the specified enumerable is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this IEnumerable thisValue)
        {
            return thisValue.IsNull() || !thisValue.Cast<object>().Any();
        }

        /// <summary>
        /// Returns a subset of a <see cref="IList"/>.
        /// </summary>
        /// <typeparam name="T">Generic of T.</typeparam>
        /// <param name="list">List to obtain the subset of.</param>
        /// <param name="start">Starting index position.</param>
        /// <param name="end">Ending index position.</param>
        /// <returns>A IEnumerable of the subset of the list.</returns>
        /// <remarks>
        /// This extension is more effective then the Skip().Take() method since Skip(N)
        /// iterate N times to skip N elements.
        /// This method does not check boundaries by design.  It is the caller responsibility.
        /// </remarks>
        public static IEnumerable<T> Subset<T>(this IList<T> list, int start, int end)
        {
            for (var i = start; i <= end; ++i)
            {
                yield return list[i];
            }
        }
    }
}