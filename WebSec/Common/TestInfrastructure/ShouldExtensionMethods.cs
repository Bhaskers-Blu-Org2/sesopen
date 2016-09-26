//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Validation;

    /// <summary>
    ///    Assertion extension methods.
    /// </summary>
    /// <remarks>
    ///    Originally borrowed from Machine.Specifications, licensed under MS-PL.
    /// </remarks>
    public static class ShouldExtensionMethods
    {
        /// <summary>
        ///    Attempts to convert two objects to compatible types.
        /// </summary>
        /// <param name = "left">
        ///    The left object.
        /// </param>
        /// <param name = "right">
        ///    The right object.
        /// </param>
        /// <returns>
        ///    A pair representing the converted objects (or null if no conversion could
        ///    be made).
        /// </returns>
        public static IPair<object, object> EquateObjects(object left, object right)
        {
            Require.NotNull(() => left);
            Require.NotNull(() => right);

            Type leftType = left.GetType();
            Type rightType = right.GetType();

            // If both types are the same, nothing to do, just return the
            // pair of objects as they were presented.
            if (leftType == rightType)
            {
                return new Pair<object, object>(left, right);
            }

            // If both types are numeric, use special handling rules for
            // equating numeric types.
            if (leftType.IsNumeric() && rightType.IsNumeric())
            {
                return EquateNumericObjects(left, right);
            }

            // if left is numeric and right is a string, try to
            // parse the string as a numeric.
            if (leftType.IsNumeric() && rightType == typeof(string))
            {
                string rightString = right.ToString();
                if (rightString.Contains('.'))
                {
                    return EquateNumericObjects(left, rightString.ToDoubleIc());
                }

                return EquateNumericObjects(left, rightString.ToInt64Ic());
            }

            // if the right is numeric and the left is a string, try to
            // parse the left as a string.
            if (leftType == typeof(string) && rightType.IsNumeric())
            {
                string leftString = left.ToString();
                if (leftString.Contains('.'))
                {
                    return EquateNumericObjects(leftString.ToDoubleIc(), right);
                }

                return EquateNumericObjects(leftString.ToInt64Ic(), right);
            }

            // The only "simi" primitives left are String, Char, DateTime, TimeSpan, & Guid

            // Otherwise we can not equate the types, simply return null.
            return null;
        }

        /// <summary>
        /// General compare.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// <typeparam name="T">
        /// Generic type parameter.
        /// </typeparam>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public static bool GeneralCompare<T>(T left, T right, GeneralComparisonOperator comparisonOperator)
        {
            // First handle Reference equality because it requires no special rules.
            if (comparisonOperator == GeneralComparisonOperator.ReferenceEqual)
            {
                return ReferenceEquals(left, right);
            }

            // First let the "conversion" system try to get us compatible types.
            IPair<object, object> equated = EquateObjects(left, right);
            object leftEquated = equated.First;
            object rightEquated = equated.Second;
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            // If we could not find compatible types, then we can not do the comparison.
            if (equated.IsNull())
            {
                throw new InvalidOperationException(
                    "Unable to equate the types ({0}) and ({1})".FormatIc(leftType.FullName, rightType.FullName));
            }

            // Numeric comparisons are "special cased" let the numeric comparison functionality handle it.
            if (leftType.IsNumeric() && rightType.IsNumeric())
            {
                return NumericCompare(leftEquated, rightEquated, comparisonOperator);
            }

            if (leftType.Is<DateTime>() && rightType.Is<DateTime>())
            {
                return DateTimeCompare(
                    leftEquated.CastTo<DateTime>(), 
                    rightEquated.CastTo<DateTime>(),
                    comparisonOperator);
            }

            // Strings are easy, lets knock them out
            if (leftType == typeof(string) && rightType == typeof(string))
            {
                return StringCompare(left.ToString(), right.ToString(), comparisonOperator);
            }

            // The rest can only be compared for equality, so use existing SafeCompare
            if (comparisonOperator == GeneralComparisonOperator.Equal ||
                comparisonOperator == GeneralComparisonOperator.NotEqual)
            {
                return comparisonOperator == GeneralComparisonOperator.Equal
                    ? SafeEquals(left, right)
                    : !SafeEquals(left, right);
            }

            throw new InvalidOperationException(
                "Unable to compare: ({0}) {2} ({1})".FormatIc(
                    leftType.FullName, 
                    rightType.FullName,
                    comparisonOperator.ToString()));
        }

        /// <summary>
        ///    Asserts the given value equal the expected value.
        /// </summary>
        /// <typeparam name = "T">The type of objects to compare.</typeparam>
        /// <param name = "actual">The given value.</param>
        /// <param name = "expected">The expected value.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static T ShouldEqual<T>(this T actual, T expected)
        {
            if (!GeneralCompare(actual, expected, GeneralComparisonOperator.Equal))
            {
                throw NewException("Should equal {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given value not equal the disallowed value.
        /// </summary>
        /// <typeparam name = "T">The type of objects to compare.</typeparam>
        /// <param name = "actual">The given value.</param>
        /// <param name = "expected">The disallowed value.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotEqual<T>(this T actual, T expected)
        {
            if (!GeneralCompare(actual, expected, GeneralComparisonOperator.NotEqual))
            {
                throw NewException("Should not equal {0} but does: {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object is less than the comparable object.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "expected">The comparable object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeLessThan(this object actual, object expected)
        {
            if (actual == null)
            {
                throw NewException("Should be less than {0} but is [null]", expected);
            }

            if (!GeneralCompare(actual, expected, GeneralComparisonOperator.LessThan))
            {
                throw NewException("Should be less than {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object is less than or equal to the comparable object.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "expected">The comparable object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeLessThanOrEqualTo(this object actual, object expected)
        {
            if (actual == null && expected != null)
            {
                throw NewException("Should be less than or equal to {0} but is [null]", expected);
            }

            if (!GeneralCompare(actual, expected, GeneralComparisonOperator.LessThanOrEqual))
            {
                throw NewException("Should be less than or equal to {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object is greater than the comparable object.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "expected">The comparable object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeGreaterThan(this object actual, object expected)
        {
            if (actual == null)
            {
                throw NewException("Should be greater than {0} but is [null]", expected);
            }

            if (!GeneralCompare(actual, expected, GeneralComparisonOperator.GreaterThan))
            {
                throw NewException("Should be greater than {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object is greater than or equal to the comparable object.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "expected">The comparable object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeGreaterThanOrEqualTo(this object actual, object expected)
        {
            if (actual == null && expected != null)
            {
                throw NewException("Should be greater than or equal to {0} but is [null]", expected);
            }

            if (!GeneralCompare(actual, expected, GeneralComparisonOperator.GreaterThanOrEqual))
            {
                throw NewException("Should be greater than or equal to {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object should not be null.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotBeNull(this object actual)
        {
            if (actual == null)
            {
                throw NewException("Should be [not null] but is [null]");
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given value should be null.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeNull(this object actual)
        {
            if (actual != null)
            {
                throw NewException("Should be [null] but is {0}", actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given collection should contain zero elements.
        /// </summary>
        /// <param name = "actual">
        ///    The given collection.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldBeEmpty(this IEnumerable actual)
        {
            if (!actual.IsEmpty())
            {
                throw NewException("Should be empty but contains:\n" + actual.Cast<object>().EachToUsefulString());
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should contain at least one element.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldNotBeEmpty(this IEnumerable actual)
        {
            if (!actual.IsNotEmpty())
            {
                throw NewException("Should not be empty but is");
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given collection should be null or empty.
        /// </summary>
        /// <param name = "actual">
        ///    The given collection.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldBeNullOrEmpty(this IEnumerable actual)
        {
            if (actual.IsNotNullOrEmpty())
            {
                throw NewException("Should be null or empty but is {0}", actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given collection should not be null or empty.
        /// </summary>
        /// <param name = "actual">
        ///    The given collection.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldNotBeNullOrEmpty(this IEnumerable actual)
        {
            if (actual.IsNullOrEmpty())
            {
                throw NewException("Should not be null or empty but is");
            }

            return actual;
        }

        // REFACTOR: I'm still working on everything below this line.  (MWP believes this REFACTOR NOTE was left by TISTOCKS).

        /// <summary>
        ///    Asserts that the given value should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <param name = "expected">
        ///    The expected value.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static float ShouldBeCloseTo(this float actual, float expected)
        {
            return actual.ShouldBeCloseTo(expected, 0.0000001f);
        }

        /// <summary>
        ///    Asserts that the given value should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <param name = "expected">
        ///    The expected value.
        /// </param>
        /// <param name = "tolerance">
        ///    The deviation tolerance.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static float ShouldBeCloseTo(this float actual, float expected, float tolerance)
        {
            if (Math.Abs(actual - expected) > tolerance)
            {
                throw NewException("Should be within {0} of {1} but is {2}", tolerance, expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given value should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <param name = "expected">
        ///    The expected value.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static double ShouldBeCloseTo(this double actual, double expected)
        {
            return actual.ShouldBeCloseTo(expected, 0.0000001d);
        }

        /// <summary>
        ///    Asserts that the given value should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <param name = "expected">
        ///    The expected value.
        /// </param>
        /// <param name = "tolerance">
        ///    The deviation tolerance.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static double ShouldBeCloseTo(this double actual, double expected, double tolerance)
        {
            if (Math.Abs(actual - expected) > tolerance)
            {
                throw NewException("Should be within {0} of {1} but is {2}", tolerance, expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given value should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <param name = "expected">
        ///    The expected value.
        /// </param>
        /// <param name = "tolerance">
        ///    The deviation tolerance.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static double ShouldBeCloseTo(this long actual, long expected, int tolerance)
        {
            if (Math.Abs(actual - expected) > tolerance)
            {
                throw NewException("Should be within {0} of {1} but is {2}", tolerance, expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given value should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given value.
        /// </param>
        /// <param name = "expected">
        ///    The expected value.
        /// </param>
        /// <param name = "tolerance">
        ///    The deviation tolerance.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static double ShouldBeCloseTo(this int actual, int expected, int tolerance)
        {
            if (Math.Abs(actual - expected) > tolerance)
            {
                throw NewException("Should be within {0} of {1} but is {2}", tolerance, expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts that the given DateTime should be approximately equal to the expected date.
        /// </summary>
        /// <param name = "actual">
        ///    The given DateTime.
        /// </param>
        /// <param name = "expected">
        ///    The expected DateTime.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static DateTime ShouldBeCloseTo(this DateTime actual, DateTime expected)
        {
            return ShouldBeCloseTo(actual, expected, new TimeSpan(0, 5, 10));
        }

        /// <summary>
        ///    Asserts that the given DateTime should be approximately equal to the expected value.
        /// </summary>
        /// <param name = "actual">
        ///    The given DateTime.
        /// </param>
        /// <param name = "expected">
        ///    The expected DateTime.
        /// </param>
        /// <param name = "tolerance">
        ///    The deviation tolerance.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static DateTime ShouldBeCloseTo(this DateTime actual, DateTime expected, TimeSpan tolerance)
        {
            if (actual.Kind == DateTimeKind.Unspecified && expected.Kind != DateTimeKind.Unspecified)
            {
                // NEIN: throwing ArgumentException, create a specific exception.
                throw new ArgumentException(
                    "DateTimeKind.Unspecified is only supported when both comparands are unspecified.", nameof(actual));
            }

            if (actual.Kind != DateTimeKind.Unspecified && expected.Kind == DateTimeKind.Unspecified)
            {
                // NEIN: throwing ArgumentException, create a specific exception.
                throw new ArgumentException(
                    "DateTimeKind.Unspecified is only supported when both comparands are unspecified.", nameof(expected));
            }

            if (Math.Abs(actual.ToUniversalTime().Ticks - expected.ToUniversalTime().Ticks) > tolerance.Ticks)
            {
                throw NewException("Should be within {0} of {1} but is {2}", tolerance, expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given string should be equal (case- and culture-insensitive) to the expected string.
        /// </summary>
        /// <param name = "actual">The given string.</param>
        /// <param name = "expected">The expected string.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldBeEqualOrdinalIgnoreCase(this string actual, string expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw NewException("Should be equal ignoring case to {0} but is [null]", expected);
            }

            if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                throw NewException("Should be equal ignoring case to {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the condition should be <c>false</c>.
        /// </summary>
        /// <param name = "actual">The condition.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static bool ShouldBeFalse(this bool actual)
        {
            if (actual)
            {
                throw NewException("Should be [false] but is [true]");
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object is of the specified type.
        /// </summary>
        /// <param name = "actual">
        ///    The given object.
        /// </param>
        /// <param name = "expected">
        ///    The expected type.
        /// </param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeOfType(this object actual, Type expected)
        {
            Require.NotNull(() => expected);

            if (actual == null)
            {
                throw NewException("Should be of type {0} but is [null]", expected.GetType());
            }

            if (!expected.IsAssignableFrom(actual.GetType()))
            {
                throw NewException("Should be of type {0} but is of type {1}", expected, actual.GetType());
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object is of the specified type.
        /// </summary>
        /// <param name = "actual">
        ///    The given object.
        /// </param>
        /// <typeparam name = "T">The expected type.</typeparam>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeOfType<T>(this object actual)
        {
            actual.ShouldBeOfType(typeof(T));
            return actual;
        }

        /// <summary>
        ///    Asserts the given value should be delimited by the provided starting and ending delimiters.
        /// </summary>
        /// <param name = "actual">The given value.</param>
        /// <param name = "expectedStartDelimiter">The starting delimiter.</param>
        /// <param name = "expectedEndDelimiter">The ending delimiter.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldBeSurroundedWith(
            this string actual, 
            string expectedStartDelimiter,
            string expectedEndDelimiter)
        {
            actual.ShouldStartWith(expectedStartDelimiter);
            actual.ShouldEndWith(expectedEndDelimiter);
            return actual;
        }

        /// <summary>
        ///    Asserts the given value should be delimited by the provided delimiter.
        /// </summary>
        /// <param name = "actual">The given value.</param>
        /// <param name = "expectedDelimiter">The delimiter.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldBeSurroundedWith(this string actual, string expectedDelimiter)
        {
            return ShouldBeSurroundedWith(actual, expectedDelimiter, expectedDelimiter);
        }

        /// <summary>
        ///    Asserts the given value have reference equality with the expected value.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "expected">The expected object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldBeTheSameAs(this object actual, object expected)
        {
            if (!ReferenceEquals(actual, expected))
            {
                throw NewException("Should be the same as {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the delegate should throw an exception of the expected type.
        /// </summary>
        /// <param name = "exceptionType">The expected type of exception.</param>
        /// <param name = "method">The delegate.</param>
        /// <returns>
        ///    The thrown exception.
        /// </returns>
        public static Exception ShouldBeThrownBy(this Type exceptionType, Action method)
        {
            Exception exception = Catch.Exception(method);

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType(exceptionType);
            return exception;
        }

        /// <summary>
        ///    Asserts the condition should be <c>true</c>.
        /// </summary>
        /// <param name = "actual">The condition.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static bool ShouldBeTrue(this bool actual)
        {
            if (!actual)
            {
                throw NewException("Should be [true] but is [false]");
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return actual;

            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        /// <summary>
        ///    Asserts the given collection should be a subset of the provided collection.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "superset">The collection that should contain the given collection.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldBeSubsetOf(this IEnumerable actual, IEnumerable superset)
        {
            IEnumerable<object> actualList = ConvertEnumerableToList(actual);
            IEnumerable<object> supersetList = ConvertEnumerableToList(superset);

            actualList.ShouldBeSubsetOf(supersetList);

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should be a proper subset of the provided superset collection.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "superset">The collection that should contain the given collection.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldBeProperSubsetOf(this IEnumerable actual, IEnumerable superset)
        {
            actual.ShouldBeSubsetOf(superset);
            IEnumerable<object> actualList = ConvertEnumerableToList(actual);
            IEnumerable<object> supersetList = ConvertEnumerableToList(superset);

            actualList.Count().ShouldNotEqual(supersetList.Count());

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should be a superset of the provided collection.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "subset">The collection that should be a subset of the given collection.</param>
        /// <typeparam name = "T">The type of elements.</typeparam>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable<T> ShouldBeSupersetOf<T>(this IEnumerable<T> actual, IEnumerable<T> subset)
        {
            var comparer = new AssertEqualityComparer<T>();

            List<T> missingItems = subset.Where(item => !actual.Contains(item, comparer)).ToList();

            if (missingItems.Any())
            {
                throw NewException(
                    "Should be a superset of: {0} \r\nsuperset collection: {1}\r\nhas missing items: {2}",
                    subset.EachToUsefulString(),
                    actual.EachToUsefulString(),
                    missingItems.EachToUsefulString());
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should be a superset of the provided items.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "subset">The collection that should be a subset of the given collection.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldBeSupersetOf(this IEnumerable actual, IEnumerable subset)
        {
            IEnumerable<object> actualList = ConvertEnumerableToList(actual);
            IEnumerable<object> subsetList = ConvertEnumerableToList(subset);

            actualList.ShouldBeSupersetOf(subsetList);

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should be a proper subset of the provided superset collection.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "subset">The collection that should contain the given collection.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldBeProperSupersetOf(this IEnumerable actual, IEnumerable subset)
        {
            actual.ShouldBeSupersetOf(subset);
            IEnumerable<object> actualList = ConvertEnumerableToList(actual);
            IEnumerable<object> subsetList = ConvertEnumerableToList(subset);

            actualList.Count().ShouldNotEqual(subsetList.Count());

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should contain the provided items.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "superset">The collection that should contain the given collection.</param>
        /// <typeparam name = "T">The type of elements.</typeparam>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable<T> ShouldBeSubsetOf<T>(this IEnumerable<T> actual, IEnumerable<T> superset)
        {
            var comparer = new AssertEqualityComparer<T>();

            List<T> missingItems = actual.Where(item => !superset.Contains(item, comparer)).ToList();

            if (missingItems.Any())
            {
                throw NewException(
                    "Should be a subset of: {0} \r\nsubset collection: {1}\r\nhas missing items: {2}",
                    superset.EachToUsefulString(),
                    actual.EachToUsefulString(),
                    missingItems.EachToUsefulString());
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given string should contain the provided substring.
        /// </summary>
        /// <param name = "actual">The given string.</param>
        /// <param name = "expected">The required substring.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldContain(this string actual, string expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw NewException("Should contain {0} but is [null]", expected);
            }

            if (!actual.Contains(expected))
            {
                throw NewException("Should contain {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given string should not contain the provided substring.
        /// </summary>
        /// <param name = "actual">The given string.</param>
        /// <param name = "expected">The required substring.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldNotContain(this string actual, string expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw NewException("Should contain {0} but is [null]", expected);
            }

            if (actual.Contains(expected))
            {
                throw NewException("Should contain {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given exception contains the required message.
        /// </summary>
        /// <param name = "actual">The given exception.</param>
        /// <param name = "expected">The required message.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static Exception ShouldContainErrorMessage(this Exception actual, string expected)
        {
            Require.NotNull(() => actual);

            actual.ShouldNotBeNull();
            actual.Message.ShouldContain(expected);
            return actual;
        }

        /// <summary>
        ///    Asserts the given collection contain the required item.
        /// </summary>
        /// <typeparam name = "T">The type of elements.</typeparam>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "expected">The required item.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable<T> ShouldContain<T>(this IEnumerable<T> actual, T expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw NewException("Should contain {0} but is [null]", expected);
            }

            if (!actual.Contains<T>(expected))
            {
                throw NewException("Should contain {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection only contain the required items.
        /// </summary>
        /// <typeparam name = "T">The type of elements.</typeparam>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "items">The required item.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable<T> ShouldContainOnly<T>(this IEnumerable<T> actual, params T[] items)
        {
            return actual.ShouldContainOnly((IEnumerable<T>)items);
        }

        /// <summary>
        ///    Asserts the given collection only contain the required items.
        /// </summary>
        /// <typeparam name = "T">The type of elements.</typeparam>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "items">The required item.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable<T> ShouldContainOnly<T>(this IEnumerable<T> actual, IEnumerable<T> items)
        {
            Require.NotNull(() => actual);
            Require.NotNull(() => items);

            var source = new List<T>(actual);
            var missingItems = new List<T>();
            var comparer = new AssertEqualityComparer<T>();

            foreach (T item in items)
            {
                if (!source.Contains(item, comparer))
                {
                    missingItems.Add(item);
                }
                else
                {
                    source.Remove(item);
                }
            }

            if (missingItems.Any() || source.Any())
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Should contain only: {0} \r\nentire collection: {1}",
                    items.EachToUsefulString(),
                    actual.EachToUsefulString());

                if (missingItems.Any())
                {
                    message += "\ndoes not contain: " + missingItems.EachToUsefulString();
                }

                if (source.Any())
                {
                    message += "\ndoes contain but shouldn't: " + source.EachToUsefulString();
                }

                throw NewException(message);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given string end with the expected substring.
        /// </summary>
        /// <param name = "actual">The given value.</param>
        /// <param name = "expected">The expected substring.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldEndWith(this string actual, string expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException("expected");
            }

            if (actual == null)
            {
                throw NewException("Should end with {0} but is [null]", expected);
            }

            if (!actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase))
            {
                throw NewException("Should end with {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given value not be of the provided type.
        /// </summary>
        /// <param name = "actual">The given value.</param>
        /// <param name = "type">The provided type.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotBeOfType(this object actual, Type type)
        {
            Require.NotNull(() => type);
            Require.NotNull(() => actual);

            Type actualType = actual.GetType();
            if (type.IsAssignableFrom(actualType))
            {
                throw NewException("Should not be of type {0} but is of type {1}", type, actualType);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given value not be of the provided type.
        /// </summary>
        /// <param name = "actual">The given value.</param>
        /// <param name = "types">The provided type.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotBeOfType(this object actual, IEnumerable<Type> types)
        {
            Require.NotNull(() => actual);

            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            Type actualType = actual.GetType();
            foreach (Type t in types.Where(t => t.IsAssignableFrom(actualType)))
            {
                throw NewException("Should not be of type {0} but is of type {1}", t, actualType);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object not have reference equality with the reference object.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "reference">The disallowed object.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotBeTheSameAs(this object actual, object reference)
        {
            if (ReferenceEquals(actual, reference))
            {
                throw NewException("Should not be the same as {0} but is {1}", reference, actual);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given object not have reference equality with any of the reference objects.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "references">The disallowed objects.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotBeTheSameAs(this object actual, params object[] references)
        {
            IEnumerable<object> referenceList = references;
            return actual.ShouldNotBeTheSameAs(referenceList);
        }

        /// <summary>
        ///    Asserts the given object not have reference equality with any of the reference objects.
        /// </summary>
        /// <param name = "actual">The given object.</param>
        /// <param name = "references">The disallowed objects.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static object ShouldNotBeTheSameAs(this object actual, IEnumerable<object> references)
        {
            Require.NotNull(() => references);
            foreach (object item in references)
            {
                actual.ShouldBeTheSameAs(item);
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should not contain any of the provided items.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "items">The disallowed items.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable ShouldNotContain(this IEnumerable actual, params object[] items)
        {
            IEnumerable<object> actualList = actual.Cast<object>();
            IEnumerable<object> expectedList = items;

            actualList.ShouldNotContain(expectedList);
            return actual;
        }

        /// <summary>
        ///    Asserts the given collection should not contain any of the provided items.
        /// </summary>
        /// <param name = "actual">The given collection.</param>
        /// <param name = "items">The disallowed items.</param>
        /// <typeparam name = "T">The type of elements.</typeparam>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static IEnumerable<T> ShouldNotContain<T>(this IEnumerable<T> actual, params T[] items)
        {
            var comparer = new AssertEqualityComparer<T>();

            List<T> contains = items.Where(item => actual.Contains(item, comparer)).ToList();

            if (contains.Any())
            {
                throw NewException(
                    "Should not contain: {0} \r\nentire collection: {1}\r\ndoes contain: {2}",
                    items.EachToUsefulString(),
                    actual.EachToUsefulString(),
                    contains.EachToUsefulString());
            }

            return actual;
        }

        /// <summary>
        ///    Asserts the given string start with the expected substring.
        /// </summary>
        /// <param name = "actual">The given value.</param>
        /// <param name = "expected">The expected substring.</param>
        /// <returns>
        ///    The <paramref name = "actual" /> argument value.
        /// </returns>
        public static string ShouldStartWith(this string actual, string expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException("expected");
            }

            if (actual == null)
            {
                throw NewException("Should start with {0} but is [null]", expected);
            }

            if (!actual.StartsWithOi(expected))
            {
                throw NewException("Should start with {0} but is {1}", expected, actual);
            }

            return actual;
        }

        /// <summary>
        /// Enumerates should match sequence in this collection.
        /// </summary>
        /// <exception cref="NewException">
        /// Thrown when a New error condition occurs.
        /// </exception>
        /// <typeparam name="T">
        /// Generic type parameter.
        /// </typeparam>
        /// <param name="actual">
        /// The given collection.
        /// </param>
        /// <param name="expected">
        /// The required item.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process should match sequence in this
        /// collection.
        /// </returns>
        public static IEnumerable<T> ShouldMatchSequence<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var matches =
                expected.Zip(actual, (e, a) => GeneralCompare(a, e, GeneralComparisonOperator.Equal)).All(m => m);

            if (!matches || actual.Count() != expected.Count())
            {
                throw NewException(
                    "Should match sequences:  expected: {0}\r\nactual: {1}\r\n",
                    expected.EachToUsefulString(),
                    actual.EachToUsefulString());
            }

            return actual;
        }

        /// <summary>
        /// Decimal compare.
        /// </summary>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool DecimalCompare(decimal left, decimal right, GeneralComparisonOperator comparisonOperator)
        {
            // switch on the comparison operator and perform the comparison.
            switch (comparisonOperator)
            {
                case GeneralComparisonOperator.Equal:
                    return left == right;

                case GeneralComparisonOperator.GreaterThan:
                    return left > right;

                case GeneralComparisonOperator.GreaterThanOrEqual:
                    return left >= right;

                case GeneralComparisonOperator.LessThan:
                    return left < right;

                case GeneralComparisonOperator.LessThanOrEqual:
                    return left <= right;

                default:
                    return left != right;
            }
        }

        /// <summary>
        /// Double compare.
        /// </summary>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool DoubleCompare(double left, double right, GeneralComparisonOperator comparisonOperator)
        {
            // switch on the comparison operator and perform the comparison.
            switch (comparisonOperator)
            {
                case GeneralComparisonOperator.Equal:
                    return Math.Abs(left - right) < float.Epsilon;

                case GeneralComparisonOperator.GreaterThan:
                    return left > right;

                case GeneralComparisonOperator.GreaterThanOrEqual:
                    return left >= right;

                case GeneralComparisonOperator.LessThan:
                    return left < right;

                case GeneralComparisonOperator.LessThanOrEqual:
                    return left <= right;

                default:
                    return Math.Abs(left - right) > float.Epsilon;
            }
        }

        /// <summary>
        /// Long compare.
        /// </summary>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool LongCompare(long left, long right, GeneralComparisonOperator comparisonOperator)
        {
            // switch on the comparison operator and perform the comparison.
            switch (comparisonOperator)
            {
                case GeneralComparisonOperator.Equal:
                    return left == right;

                case GeneralComparisonOperator.GreaterThan:
                    return left > right;

                case GeneralComparisonOperator.GreaterThanOrEqual:
                    return left >= right;

                case GeneralComparisonOperator.LessThan:
                    return left < right;

                case GeneralComparisonOperator.LessThanOrEqual:
                    return left <= right;

                default:
                    return left != right;
            }
        }

        /// <summary>
        /// Ulong compare.
        /// </summary>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool UlongCompare(ulong left, ulong right, GeneralComparisonOperator comparisonOperator)
        {
            // switch on the comparison operator and perform the comparison.
            switch (comparisonOperator)
            {
                case GeneralComparisonOperator.Equal:
                    {
                        return left == right;
                    }

                case GeneralComparisonOperator.GreaterThan:
                    {
                        return left > right;
                    }

                case GeneralComparisonOperator.GreaterThanOrEqual:
                    {
                        return left >= right;
                    }

                case GeneralComparisonOperator.LessThan:
                    {
                        return left < right;
                    }

                case GeneralComparisonOperator.LessThanOrEqual:
                    {
                        return left <= right;
                    }

                default:
                    {
                        return left != right;
                    }
            }
        }

        /// <summary>
        /// String compare.
        /// </summary>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool StringCompare(string left, string right, GeneralComparisonOperator comparisonOperator)
        {
            int comp = string.Compare(left, right, StringComparison.Ordinal);

            // switch on the comparison operator and perform the comparison.)
            switch (comparisonOperator)
            {
                case GeneralComparisonOperator.Equal:
                    {
                        return comp == 0;
                    }

                case GeneralComparisonOperator.GreaterThan:
                    {
                        return comp > 0;
                    }

                case GeneralComparisonOperator.GreaterThanOrEqual:
                    {
                        return comp >= 0;
                    }

                case GeneralComparisonOperator.LessThan:
                    {
                        return comp < 0;
                    }

                case GeneralComparisonOperator.LessThanOrEqual:
                    {
                        return comp <= 0;
                    }

                default:
                    {
                        return comp != 0;
                    }
            }
        }

        /// <summary>
        /// Date time compare.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool DateTimeCompare(DateTime left, DateTime right, GeneralComparisonOperator comparisonOperator)
        {
            // the FCL assumes local time when converting unspecified date times.  Active Directory uses
            // unspecified times holding UTC values.  Force the engineers to be diligent with regard to DateTime.
            if (left.Kind == DateTimeKind.Unspecified && right.Kind != DateTimeKind.Unspecified)
            {
                // NEIN: throwing ArgumentException, create a specific exception.
                throw new ArgumentException(
                    "DateTimeKind.Unspecified is only supported when both comparands are unspecified.", nameof(left));
            }

            if (left.Kind != DateTimeKind.Unspecified && right.Kind == DateTimeKind.Unspecified)
            {
                // NEIN: throwing ArgumentException, create a specific exception.
                throw new ArgumentException(
                    "DateTimeKind.Unspecified is only supported when both comparands are unspecified.", nameof(right));
            }

            left = left.ToUniversalTime();
            right = right.ToUniversalTime();

            // switch on the comparison operator and perform the comparison.
            switch (comparisonOperator)
            {
                case GeneralComparisonOperator.Equal:
                    {
                        return left == right;
                    }

                case GeneralComparisonOperator.GreaterThan:
                    {
                        return left > right;
                    }

                case GeneralComparisonOperator.GreaterThanOrEqual:
                    {
                        return left >= right;
                    }

                case GeneralComparisonOperator.LessThan:
                    {
                        return left < right;
                    }

                case GeneralComparisonOperator.LessThanOrEqual:
                    {
                        return left <= right;
                    }

                default:
                    {
                        return left != right;
                    }
            }
        }

        /// <summary>
        /// Numeric compare.
        /// </summary>
        /// <param name="left">
        /// The left object.
        /// </param>
        /// <param name="right">
        /// The right object.
        /// </param>
        /// <param name="comparisonOperator">
        /// The type of comparison to perform.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool NumericCompare(object left, object right, GeneralComparisonOperator comparisonOperator)
        {
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            // There is no Reference Equality as that would have been handle by the caller, so we only have to
            // deal with mathematical comparisons.

            // First let's deal with decimals (which is a special case floating point).
            if (leftType == typeof(decimal) && rightType == typeof(decimal))
            {
                return DecimalCompare(left.CastTo<decimal>(), right.CastTo<decimal>(), comparisonOperator);
            }

            // Next let's deal with any FloatingPoint (other than decimals) as they require no additional special rules.
            if (leftType.IsFloatingPoint() || rightType.IsFloatingPoint())
            {
                // Safely cast both as decimal and do a Floating Point comparison.
                return DoubleCompare(
                    Convert.ToDouble(left, CultureInfo.InvariantCulture),
                    Convert.ToDouble(right, CultureInfo.InvariantCulture),
                    comparisonOperator);
            }

            // Next let's deal with integer types when neither are a ulong.
            if (leftType != typeof(ulong) && rightType != typeof(ulong))
            {
                return LongCompare(
                    Convert.ToInt64(left, CultureInfo.InvariantCulture),
                    Convert.ToInt64(right, CultureInfo.InvariantCulture), 
                    comparisonOperator);
            }

            // If both types are ulong, we can simply compare the values
            if (leftType == typeof(ulong) && rightType == typeof(ulong))
            {
                return UlongCompare(
                    Convert.ToUInt64(left, CultureInfo.InvariantCulture),
                    Convert.ToUInt64(right, CultureInfo.InvariantCulture), 
                    comparisonOperator);
            }

            // We only need to test one for ulong because the other can't be (we've handled the case where they both are).
            bool leftIsUlong = left.Is<ulong>();

            // Okay now the special case rules for ulong types.  We already know that both are not
            // ulong types (as that has already been handled).  The casting rules will only result with
            // a ulong value if the value of the ulong was greater than long.MaxValue (otherwise it would
            // have simply been turned into a long).  Thus most of the comparisons are fairly simple)
            switch (comparisonOperator)
            {
                // They can not be equal because one is greater than long.MaxValue and the other is not.
                case GeneralComparisonOperator.Equal:
                    return false;

                // left is greater than (also greater than or equal) right if left was the ulong, otherwise it is not.
                case GeneralComparisonOperator.GreaterThan:
                case GeneralComparisonOperator.GreaterThanOrEqual:
                    return leftIsUlong;

                case GeneralComparisonOperator.LessThan:
                case GeneralComparisonOperator.LessThanOrEqual:
                    return !leftIsUlong;

                // Only not equal remains, they must be not equal because one is greater than
                // long.MaxValue and the other is not.
                default:
                    return true;
            }
        }

        /// <summary>
        /// Equate integer objects.
        /// </summary>
        /// <param name="left">
        /// The left object.
        /// </param>
        /// <param name="right">
        /// The right object.
        /// </param>
        /// <returns>
        /// An IPair&lt;object,object&gt;
        /// </returns>
        private static IPair<object, object> EquateIntegerObjects(object left, object right)
        {
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            // If neither type is a ulong (or it is a ulong but is less that the value of long.MaxValue),
            // then it is safe to cast both to long and all comparisons operations will work correctly.
            if (((leftType != typeof(ulong)) || (leftType == typeof(ulong) && left.CastTo<ulong>() <= long.MaxValue)) &&
                ((rightType != typeof(ulong)) ||
                 (rightType == typeof(ulong) && right.CastTo<ulong>() <= long.MaxValue)))
            {
                return new Pair<object, object>(
                    Convert.ToInt64(left, CultureInfo.InvariantCulture),
                    Convert.ToInt64(right, CultureInfo.InvariantCulture));
            }

            // If the left object is a ulong, leave it as a ulong (the comparison operation
            // has special cast handling for ulong types.
            if (leftType == typeof(ulong))
            {
                return new Pair<object, object>(left, Convert.ToInt64(right, CultureInfo.InvariantCulture));
            }

            // Otherwise the right object must be the ulong (because this function is never
            // called if they are both the same type), so we can safely cast the left to
            // long and keep the right as a ulong (again, the comparison operation has
            // special handling for ulong types).
            return new Pair<object, object>(Convert.ToInt64(left, CultureInfo.InvariantCulture), right);
        }

        /// <summary>
        /// Equate numeric objects.
        /// </summary>
        /// <param name="left">
        /// The left object.
        /// </param>
        /// <param name="right">
        /// The right object.
        /// </param>
        /// <returns>
        /// An IPair&lt;object,object&gt;
        /// </returns>
        private static IPair<object, object> EquateNumericObjects(object left, object right)
        {
            Type leftType = left.GetType();
            Type rightType = right.GetType();

            // If either type is a Boolean value transform both types to a Boolean
            if (leftType == typeof(bool) || rightType == typeof(bool))
            {
                return new Pair<object, object>(
                    Convert.ToBoolean(left, CultureInfo.InvariantCulture),
                    Convert.ToBoolean(right, CultureInfo.InvariantCulture));
            }

            // Okay, here's the deal (from MSDN) on decimals, they have greater precision (after
            // the point) but a smaller range (before the decimal point).  Consequently,
            // double.MaxValue and float.MaxValue (or double.MinValue and float.MinValue) can not fit
            // into a decimal value.  As a result, we *have* to preference range over precision.
            // So we therefore first try to cast to double first, then to single, then finally decimal.

            // This hurts for decimal comparisons where precision is critical, but in that case, the
            // answer *has* be the calling test case should ensure that both are a decimal first, then
            // do the comparison.  (As we have established, we only get to this point if both types
            // are not exactly the same going into the comparison).

            // If either type is a double, the result is both cast to double.
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return new Pair<object, object>(
                    Convert.ToDouble(left, CultureInfo.InvariantCulture),
                    Convert.ToDouble(right, CultureInfo.InvariantCulture));
            }

            // If either type is a float, the result is both cast to a float.
            if (leftType == typeof(float) || rightType == typeof(float))
            {
                return new Pair<object, object>(
                    Convert.ToSingle(left, CultureInfo.InvariantCulture),
                    Convert.ToSingle(right, CultureInfo.InvariantCulture));
            }

            // If either type is a decimal, the result is both cast to decimal.
            if (leftType == typeof(decimal) || rightType == typeof(decimal))
            {
                return new Pair<object, object>(
                    Convert.ToDecimal(left, CultureInfo.InvariantCulture),
                    Convert.ToDecimal(right, CultureInfo.InvariantCulture));
            }

            // Otherwise, they must both be integers because this function is only called
            // if they were both numeric types to begin with.  Thus, let the integer equation
            // logic handle the problem.
            return EquateIntegerObjects(left, right);
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that each to useful string.
        /// </summary>
        /// <typeparam name="T">
        /// Generic type parameter.
        /// </typeparam>
        /// <param name="enumerable">
        /// The enumerable to act on.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        private static string EachToUsefulString<T>(this IEnumerable<T> enumerable)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.Append(string.Join(",\n", enumerable.Select(x => x.ToUsefulString().Tab()).Take(10).ToArray()));
            if (enumerable.Count() > 10)
            {
                if (enumerable.Count() > 11)
                {
                    sb.AppendLine(string.Format(
                        CultureInfo.InvariantCulture, 
                        ",\n  ...({0} more elements)",
                        enumerable.Count() - 10));
                }
                else
                {
                    sb.AppendLine(",\n" + enumerable.Last().ToUsefulString().Tab());
                }
            }
            else
            {
                sb.AppendLine();
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="parameters">
        /// A variable-length parameters list containing parameters.
        /// </param>
        /// <returns>
        /// An Exception.
        /// </returns>
        private static Exception NewException(string message, params object[] parameters)
        {
            if (parameters.Any())
            {
                return
                    AssertFailureExceptionFactory.CreateException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            message,
                            parameters.Select(x => x.ToUsefulString()).Cast<object>().ToArray()));
            }

            return AssertFailureExceptionFactory.CreateException(message);
        }

        /// <summary>
        /// A T extension method that safe equals.
        /// </summary>
        /// <typeparam name="T">
        /// Generic type parameter.
        /// </typeparam>
        /// <param name="left">
        /// The left value for the comparison.
        /// </param>
        /// <param name="right">
        /// The right value for the comparison.
        /// </param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool SafeEquals<T>(this T left, T right)
        {
            var comparer = new AssertEqualityComparer<T>();

            return comparer.Equals(left, right);
        }

        /// <summary>
        /// A string extension method that tabs the given string.
        /// </summary>
        /// <param name="value">
        /// The value to act on.
        /// </param>
        /// <returns>
        /// A string.
        /// </returns>
        private static string Tab(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string[] split = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            sb.Append("  " + split[0]);
            foreach (string part in split.Skip(1))
            {
                sb.AppendLine();
                sb.Append("  " + part);
            }

            return sb.ToString();
        }

        /// <summary>
        /// An object extension method that converts an obj to an useful string.
        /// </summary>
        /// <param name="obj">
        /// The obj to act on.
        /// </param>
        /// <returns>
        /// obj as a string.
        /// </returns>
        private static string ToUsefulString(this object obj)
        {
            string str;
            if (obj == null)
            {
                return "[null]";
            }

            if (obj is string)
            {
                str = (string)obj;

                return "\"" + str.Replace("\n", "\\n") + "\"";
            }

            if (obj.GetType().IsValueType)
            {
                return "[" + obj + "]";
            }

            var objAsEnumerable = obj as IEnumerable;
            if (objAsEnumerable != null)
            {
                IEnumerable<object> enumerable = objAsEnumerable.Cast<object>();

                return obj.GetType() + ":\n" + enumerable.EachToUsefulString();
            }

            str = obj.ToString();

            if (string.IsNullOrEmpty(str))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}:[]", obj.GetType());
            }

            str = str.Trim();

            if (str.Contains("\n"))
            {
                return string.Format(CultureInfo.InvariantCulture, "{1}:\r\n[\r\n{0}\r\n]", str.Tab(), obj.GetType());
            }

            if (obj.GetType().ToString() == str)
            {
                return obj.GetType().ToString();
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}:[{1}]", obj.GetType(), str);
        }

        /// <summary>
        /// Enumerates convert enumerable to list in this collection.
        /// </summary>
        /// <param name="inputEnumerable">
        /// The input enumerable.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process convert enumerable to list in this
        /// collection.
        /// </returns>
        private static IEnumerable<object> ConvertEnumerableToList(IEnumerable inputEnumerable)
        {
            return inputEnumerable.Is<IEnumerable>() && !inputEnumerable.Is<string>()
                ? inputEnumerable.Cast<object>()
                : new[] { inputEnumerable };
        }
    }
}