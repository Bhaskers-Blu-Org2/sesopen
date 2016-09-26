//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace System
{
    using Globalization;

    /// <summary>
    ///    Provides extensions methods to the String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///    Parses a string into a double using the Invariant Culture.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to be parsed.
        /// </param>
        /// <returns>
        ///    A double value resulting from the string parsing.
        /// </returns>
        public static double ToDoubleIc(this string thisValue)
        {
            return double.Parse(thisValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///    Parses a string into a double using the Invariant Culture.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to be parsed.
        /// </param>
        /// <returns>
        ///    A double value resulting from the string parsing.
        /// </returns>
        public static long ToInt64Ic(this string thisValue)
        {
            return long.Parse(thisValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///    Compares the ordinal insensitive.
        /// </summary>
        /// <param name = "thisValue">The string.</param>
        /// <param name = "with">The comparison string.</param>
        /// <returns> The result. </returns>
        public static int CompareOi(this string thisValue, string with)
        {
            return string.Compare(thisValue, with, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///    Compares two strings for equality using Ordinal Insensitive comparison.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to compare.
        /// </param>
        /// <param name = "with">
        ///    The string to compare it with.
        /// </param>
        /// <returns>
        ///    True if the strings are the same otherwise false.
        /// </returns>
        public static bool EqualsOi(this string thisValue, string with)
        {
            return CompareOi(thisValue, with) == 0;
        }

        /// <summary>
        /// Returns true if any string in search is found in thisValue.
        /// </summary>
        /// <param name="thisValue">
        /// The string to search within.
        /// </param>
        /// <param name="search">
        /// The array of strings to search for.
        /// </param>
        /// <returns>returns true of contains otherwise false</returns>
        public static bool ContainsAnyOi(this string thisValue, string[] search)
        {
            foreach (string needle in search)
            {
                if (thisValue.IndexOfOi(needle) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///    Returns the IndexOf one string within another using Ordinal 
        ///    Case Insensitive search.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to search within.
        /// </param>
        /// <param name = "search">
        ///    The string to search for.
        /// </param>
        /// <param name="startIndex">start index to search from</param>
        /// <returns>
        ///    The location of the search string within the string to search or
        ///    -1 if the string is not contained.
        /// </returns>
        public static int IndexOfOi(this string thisValue, string search, int startIndex)
        {
            if (ReferenceEquals(thisValue, null))
            {
                throw new ArgumentNullException(nameof(thisValue));
            }

            return thisValue.IndexOf(search, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///    Returns the IndexOf one string within another using Ordinal 
        ///    Case Insensitive search.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to search within.
        /// </param>
        /// <param name = "search">
        ///    The string to search for.
        /// </param>
        /// <returns>
        ///    The location of the search string within the string to search or
        ///    -1 if the string is not contained.
        /// </returns>
        public static int IndexOfOi(this string thisValue, string search)
        {
            if (ReferenceEquals(thisValue, null))
            {
                throw new ArgumentNullException("thisValue");
            }

            return thisValue.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///    Returns true if ends with or false otherwise
        ///    Case Insensitive search.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to search within.
        /// </param>
        /// <param name = "search">
        ///    The string to search for.
        /// </param>
        /// <returns>
        ///    true if ends with or false otherwise
        /// </returns>
        public static bool EndsWithOi(this string thisValue, string search)
        {
            if (ReferenceEquals(thisValue, null))
            {
                throw new ArgumentNullException(nameof(thisValue));
            }

            return thisValue.EndsWith(search, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///    Returns true if starts with or false otherwise
        ///    Case Insensitive search.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to search within.
        /// </param>
        /// <param name = "search">
        ///    The string to search for.
        /// </param>
        /// <returns>
        ///    true if starts with or false otherwise
        /// </returns>
        public static bool StartsWithOi(this string thisValue, string search)
        {
            if (ReferenceEquals(thisValue, null))
            {
                throw new ArgumentNullException(nameof(thisValue));
            }

            return thisValue.StartsWith(search, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///    Formats a string using standard CLR string formatting rules for
        ///    the Invariant Culture.
        /// </summary>
        /// <param name = "thisValue">
        ///    The string to be used as the base format pattern.
        /// </param>
        /// <param name = "args">
        ///    The arguments to supply when formatting the string.
        /// </param>
        /// <returns>
        ///    The string formatted using the Invariant Culture.
        /// </returns>
        public static string FormatIc(this string thisValue, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                // arguments might contain JSON which will crash the string formatter, ensure they are escaped.
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is string)
                    {
                        args[i] = args[i].CastTo<string>().Replace("{", "{{").Replace("}", "}}");
                    }
                }

                return string.Format(CultureInfo.InvariantCulture, thisValue, args);
            }

            return thisValue;
        }
    }
}