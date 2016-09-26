//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace System
{
    /// <summary>
    ///    Provides extensions methods to the Object class.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        ///    Casts the specified obj.
        /// </summary>
        /// <typeparam name = "T">The target type of the cast.</typeparam>
        /// <param name = "inputValue">The object.</param>
        /// <returns>
        ///    The result of the cast.
        /// </returns>
        public static T CastTo<T>(this object inputValue)
        {
            return (T)inputValue;
        }

        /// <summary>
        ///    Evaluates type compatibility.
        /// </summary>
        /// <typeparam name = "T">
        ///    The type to evaluate against.
        /// </typeparam>
        /// <param name = "inputValue">
        ///    The object to evaluate compatibility for.
        /// </param>
        /// <returns>
        ///    True if the object is compatible otherwise false.
        /// </returns>
        public static bool Is<T>(this object inputValue)
        {
            return inputValue is T;
        }

        /// <summary>
        ///    Performs an as operation on the supplied object.
        /// </summary>
        /// <typeparam name = "T">The target type of the as operation.</typeparam>
        /// <param name = "inputValue">The object.</param>
        /// <returns>
        ///    The result of the as operation.
        /// </returns>
        public static T As<T>(this object inputValue) where T : class
        {
            return inputValue as T;
        }

        /// <summary>
        ///    Determines whether the specified object is null.
        /// </summary>
        /// <param name = "inputValue">The object.</param>
        /// <returns>
        ///    <c>true</c> if the specified object is null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNull([ValidatedNotNullAttribute] this object inputValue)
        {
            return ReferenceEquals(inputValue, null);
        }

        /// <summary>
        ///    Determines whether the specified object is not null.
        /// </summary>
        /// <param name = "inputValue">The object.</param>
        /// <returns>
        ///    <c>true</c> if the specified object is not null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNull([ValidatedNotNullAttribute] this object inputValue)
        {
            return !ReferenceEquals(inputValue, null);
        }
    }
}