//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;
    using WebSec.Common.Validation;

    /// <summary>
    /// (Unit Test Class) a require tests.
    /// </summary>
    [TestClass]
    public class RequireTests
    {
        /// <summary>
        /// (Unit Test Method) iterator should not throw argument exception when comparand is equal.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowArgumentExceptionOnWhenComparandIsEqual()
        {
            const int Comparand = 1;
            int target = Comparand;
            target++;
            target--;
            Require.EqualTo(() => target, Comparand);
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw argument exception when comparand is not equal.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowArgumentExceptionOnWhenComparandIsNotEqual()
        {
            const int Comparand = 1;
            int target = Comparand;
            target++;
            typeof(ArgumentException).ShouldBeThrownBy(() => Require.EqualTo(() => target, Comparand));
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw invalid operation exception a null expression.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowInvalidOperationExceptionOnANullExpression()
        {
            string comparand = Guid.NewGuid().ToString();
            typeof(InvalidOperationException).ShouldBeThrownBy(() => Require.EqualTo(null, comparand));
        }

        /// <summary>
        /// (Unit Test Method) iterator should not throw when target equals minimum.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowWhenTargetEqualsMinimum()
        {
            const int Minimum = 1;
            int target = Minimum;
            target--;
            target++;
            Require.GreaterThanOrEqualTo(() => target, Minimum);
        }

        /// <summary>
        /// (Unit Test Method) iterator should not throw when target is greater than minimum.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowWhenTargetIsGreaterThanMinimum()
        {
            const int Minimum = 1;
            int target = Minimum;
            target++;
            Require.GreaterThanOrEqualTo(() => target, Minimum);
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw argument out of range exception when target is less
        /// than minimum.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowArgumentOutOfRangeExceptionWhenTargetIsLessThanMinimum()
        {
            const int Minimum = 1;
            int target = Minimum;
            target--;
            typeof(ArgumentOutOfRangeException).ShouldBeThrownBy(() => Require.GreaterThanOrEqualTo(() => target, Minimum));
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw invalid operation exception a null minimum.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowInvalidOperationExceptionOnANullMinimum()
        {
            string target = Guid.NewGuid().ToString();
            typeof(InvalidOperationException).ShouldBeThrownBy(() => Require.GreaterThanOrEqualTo(() => target, null));
        }

        /// <summary>
        /// (Unit Test Method) iterator should not throw on not null expression value.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowOnNotNullExpressionValue()
        {
            string target = string.Empty;
            Require.NotNull(() => target);
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw argument null exception null expression value.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowArgumentNullExceptionOnNullExpressionValue()
        {
            string target = null;
            typeof(ArgumentNullException).ShouldBeThrownBy(() => Require.NotNull(() => target));
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw invalid operation exception null expression
        /// argument.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowInvalidOperationExceptionOnNullExpressionArgument()
        {
            typeof(InvalidOperationException).ShouldBeThrownBy(() => Require.NotNull(null));
        }

        /// <summary>
        /// (Unit Test Method) iterator should not throw on a non null empty enumerable.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowOnANonNullEmptyEnumerable()
        {
            IEnumerable<string> target = new List<string>();
            Require.NotNull(() => target);
        }

        /// <summary>
        /// (Unit Test Method) iterator should not throw on a non null enumerable containing no nulls.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowOnANonNullEnumerableContainingNoNulls()
        {
            IEnumerable<string> target = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            Require.NotNull(() => target);
        }

        /// <summary>
        /// (Unit Test Method) iterator should not throw on not null not empty expression value.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldNotThrowOnNotNullNotEmptyExpressionValue()
        {
            string target = Guid.NewGuid().ToString();
            Require.NotNullOrEmpty(() => target);
        }

        /// <summary>
        /// (Unit Test Method) iterator should throw argument exception empty expression value.
        /// </summary>
        [TestMethod]
        [TestCategory("CheckIn")]
        public void ItShouldThrowArgumentExceptionOnEmptyExpressionValue()
        {
            string target = string.Empty;
            typeof(ArgumentNullException).ShouldBeThrownBy(() => Require.NotNullOrEmpty(() => target));
        }
    }
}