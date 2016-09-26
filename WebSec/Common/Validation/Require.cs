//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.Validation
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;

    /// <summary>
    /// A Require Contract for Designs.  Specifies that the executed contracts
    /// (specified methods) describe contracts that are Required.
    /// </summary>
    public static class Require
    {
        /// <summary>
        /// The programming error in invocation of require.
        /// </summary>
        private const string ProgrammingErrorInInvocationOfRequire = "Programming error in the invocation of Require.";

        /// <summary>
        /// The programming error in invocation of require on non member expression.
        /// </summary>
        private const string ProgrammingErrorInInvocationOfRequireOnNonMemberExpression =
            ProgrammingErrorInInvocationOfRequire
            + "  Non-member expression detected.  (Are you requiring on a constant?)";

        /// <summary>
        /// The programming error in invocation of require on static.
        /// </summary>
        private const string ProgrammingErrorInInvocationOfRequireOnStatic =
            ProgrammingErrorInInvocationOfRequire + "  Require is not for use on statics.";

        /// <summary>
        /// Specifies that a value must equal another given value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the values being compared.
        /// </typeparam>
        /// <param name="expression">
        /// A lambda expression whose execution yields the value being compared.
        /// </param>
        /// <param name="comparand">
        /// The value being compared to.
        /// </param>
        public static void EqualTo<T>(Expression<Func<IEquatable<T>>> expression, T comparand)
        {
            try
            {
                // comparand is allowed to be null.
                NotNull(() => expression);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequire, ae);
            }

            // When the value of the parameter does not equal the comparand, throw an ArgumentException.
            var actualValue = expression.Compile().Invoke();
            if (!actualValue.Equals(comparand))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, "The value must equal {0}.", comparand));
            }
        }

        /// <summary>
        /// Specifies that a value must be greater than or equal to the given minimum.
        /// </summary>
        /// <param name="expression">
        /// A Lambda expressions to be executed when evaluating the
        /// value of an argument, or local or member variable.
        /// </param>
        /// <param name="minimum">
        /// The minimum allowed value.
        /// </param>
        /// <typeparam name="T">
        /// The type of the comparable.
        /// </typeparam>
        public static void GreaterThanOrEqualTo<T>(Expression<Func<IComparable<T>>> expression, T minimum)
        {
            try
            {
                NotNull(() => expression);
                NotNull(() => minimum);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequire, ae);
            }

            // when the value of the parameter is less than the minimum, throw an ArgumentOutOfRangeException.
            var actualValue = expression.Compile().Invoke();

            if ((actualValue == null) || actualValue.CompareTo(minimum) < 0)
            {
                var uexp = (UnaryExpression)expression.Body;
                var mexp = (MemberExpression)uexp.Operand;
                string name = mexp.Member.Name;
                string msg = string.Format(
                    CultureInfo.CurrentCulture, "The value must be greater than or equal to {0}.", minimum);
                throw new ArgumentOutOfRangeException(name, actualValue, msg);
            }
        }

        /// <summary>
        /// Specifies that a string value must not be null or empty.
        /// </summary>
        /// <param name="expression">
        /// A Lambda expression to be executed when evaluating the value
        /// of a local or member variable.  This expression must be a string
        /// and the result must not be Null or Empty.
        /// </param>
        public static void NotNullOrEmpty(Expression<Func<string>> expression)
        {
            try
            {
                NotNull(() => expression);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequire, ae);
            }

            // Get the name of the parameter.
            string name;
            try
            {
                name = ExpressionHelper.GetNameFromLambda(expression);
            }
            catch (InvalidCastException ice)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequireOnNonMemberExpression, ice);
            }

            // when the value of the parameter is a null value or empty string, determine the
            // name of the parameter and throw an ArgumentNullException
            string value = ExecuteExpression(name, expression);
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(ExpressionHelper.GetNameFromLambda(expression));
            }
        }

        /// <summary>
        /// Specifies that a value must not be null.
        /// </summary>
        /// <param name="expression">
        /// A Lambda expression to be executed when evaluating the
        /// value of a local or member variable.
        /// </param>
        public static void NotNull(Expression<Func<object>> expression)
        {
            if (expression == null)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequire);
            }

            // Get the name of the parameter.
            string name;
            try
            {
                name = ExpressionHelper.GetNameFromLambda(expression);
            }
            catch (InvalidCastException ice)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequireOnNonMemberExpression, ice);
            }

            // when the value of the parameter is a null reference, determine the
            // name of the parameter and throw an ArgumentNullException
            object value = ExecuteExpression(name, expression);
            if (value == null)
            {
                throw new ArgumentNullException(ExpressionHelper.GetNameFromLambda((LambdaExpression)expression));
            }
        }

        /// <summary>
        /// Executes the lambda operation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// A lambda expression whose execution yields the value being compared.
        /// </param>
        /// <returns>
        /// An object.
        /// </returns>
        private static object ExecuteLambda(string name, LambdaExpression expression)
        {
            Expression current = expression;
            while (current != null && current.NodeType != ExpressionType.Constant)
            {
                switch (current.NodeType)
                {
                    case ExpressionType.Lambda:
                        current = ((LambdaExpression)current).Body;
                        break;

                    case ExpressionType.MemberAccess:
                        current = ((MemberExpression)current).Expression;
                        break;

                    case ExpressionType.Convert:
                        current = ((UnaryExpression)current).Operand;
                        break;
                }
            }

            if (current == null)
            {
                throw new InvalidOperationException(ProgrammingErrorInInvocationOfRequireOnStatic);
            }

            object value = ((ConstantExpression)current).Value;
            object retval = null;
            if (value != null && value.GetType().Name.Contains("<>c__DisplayClass"))
            {
                var fields = value.GetType().GetFields();
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.Name == name)
                    {
                        retval = fieldInfo.GetValue(value);
                        break;
                    }
                }
            }

            return retval;
        }

        /// <summary>
        /// Executes the expression operation.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// A Lambda expression to be executed when evaluating the value of a local or member variable.
        /// </param>
        /// <returns>
        /// An object.
        /// </returns>
        private static object ExecuteExpression(string name, Expression<Func<object>> expression)
        {
            return ExecuteLambda(name, expression);
        }

        /// <summary>
        /// Executes the expression operation.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// A Lambda expression to be executed when evaluating the value of a local or member variable.
        /// </param>
        /// <returns>
        /// An object.
        /// </returns>
        private static string ExecuteExpression(string name, Expression<Func<string>> expression)
        {
            return (string)ExecuteLambda(name, expression);
        }
    }
}