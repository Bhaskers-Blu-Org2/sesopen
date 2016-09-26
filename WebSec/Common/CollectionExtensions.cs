//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace System
{
    using Collections.Generic;

    /// <summary>
    ///    Extension methods for <see cref = "ICollection{T}" />.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///    Supports add range functionality similar to <see cref = "List{T}" />.
        /// </summary>
        /// <typeparam name = "T">The type of elements in the list.</typeparam>
        /// <param name = "destination">The collection to which the source items are copied.</param>
        /// <param name = "source">The source collection which is copied.</param>
        /// <exception cref = "ArgumentNullException">
        ///    <paramref name = "source" /> is null.
        /// </exception>
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            if (ReferenceEquals(destination, null))
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (ReferenceEquals(source, null))
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var item in source)
            {
                destination.Add(item);
            }
        }
    }
}