//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.Validation
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// The lambda helper.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Helper to extract the name from a expression.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// The name from expression.
        /// </returns>
        /// <example>
        /// <code>
        /// LambdaHelper.GetNameFromLambda( () =&gt; variable );
        /// </code>
        /// </example>
        public static string GetNameFromLambda(LambdaExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                    return ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member.Name;

                default:
                    return ((MemberExpression)expression.Body).Member.Name;
            }
        }

        /// <summary>
        /// Helper to extract the name from a lambda.
        /// </summary>
        /// <param name="expression">
        /// The lambda expression.
        /// </param>
        /// <returns>
        /// The name from lambda.
        /// </returns>
        /// <example>
        /// <code>
        /// LambdaHelper.GetNameFromLambda( () =&gt; this.MyProperty);
        /// </code>
        /// </example>
        public static string GetNameFromLambda(Expression<Func<object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return GetNameFromLambda((LambdaExpression)expression);
        }
    }
}