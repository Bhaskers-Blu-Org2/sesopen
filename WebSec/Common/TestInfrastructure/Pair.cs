//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///    Default implementation of <see cref = "IPair{TFirst,TSecond}" />.
    /// </summary>
    /// <typeparam name = "TFirst">The first type.</typeparam>
    /// <typeparam name = "TSecond">The second type.</typeparam>
    public sealed class Pair<TFirst, TSecond> : IPair<TFirst, TSecond>
    {
        /// <summary>
        /// The first comparison.
        /// </summary>
        private readonly Comparison<TFirst> firstComparison;

        /// <summary>
        /// The first equality comparer.
        /// </summary>
        private readonly IEqualityComparer<TFirst> firstEqualityComparer;

        /// <summary>
        /// The second comparison.
        /// </summary>
        private readonly Comparison<TSecond> secondComparison;

        /// <summary>
        /// The second equality comparer.
        /// </summary>
        private readonly IEqualityComparer<TSecond> secondEqualityComparer;

        /// <summary>
        /// The first.
        /// </summary>
        private TFirst first;

        /// <summary>
        /// The second.
        /// </summary>
        private TSecond second;

        /// <summary>
        ///    Initializes a new instance of the <see cref = "Pair{TFirst, TSecond}" /> class.
        /// </summary>
        public Pair()
            : this(default(TFirst), null, Comparer<TFirst>.Default, default(TSecond), null, Comparer<TSecond>.Default)
        {
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref = "Pair{TFirst, TSecond}" /> class.
        /// </summary>
        /// <param name = "first">The first.</param>
        /// <param name = "second">The second.</param>
        public Pair(TFirst first, TSecond second)
            : this(first, null, Comparer<TFirst>.Default, second, null, Comparer<TSecond>.Default)
        {
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref = "Pair{TFirst, TSecond}" /> class.
        /// </summary>
        /// <param name = "first">The first.</param>
        /// <param name = "firstEqualityComparer">The first equality comparer.</param>
        /// <param name = "firstSortingComparer">The first sorting comparer.</param>
        /// <param name = "second">The second.</param>
        /// <param name = "secondEqualityComparer">The second equality comparer.</param>
        /// <param name = "secondSortingComparer">The second sorting comparer.</param>
        public Pair(
            TFirst first,
            IEqualityComparer<TFirst> firstEqualityComparer,
            IComparer<TFirst> firstSortingComparer,
            TSecond second,
            IEqualityComparer<TSecond> secondEqualityComparer,
            IComparer<TSecond> secondSortingComparer)
        {
            this.first = first;
            this.firstEqualityComparer = firstEqualityComparer ?? EqualityComparer<TFirst>.Default;
            IComparer<TFirst> firstComp = firstSortingComparer ?? Comparer<TFirst>.Default;
            firstComparison = firstComp.Compare;

            this.second = second;
            this.secondEqualityComparer = secondEqualityComparer ?? EqualityComparer<TSecond>.Default;
            IComparer<TSecond> secondComp = secondSortingComparer ?? Comparer<TSecond>.Default;
            secondComparison = secondComp.Compare;
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref = "Pair{TFirst, TSecond}" /> class.
        /// </summary>
        /// <param name = "first">The first.</param>
        /// <param name = "firstEqualityComparer">The first equality comparer.</param>
        /// <param name = "firstSortingComparison">The first sorting comparison.</param>
        /// <param name = "second">The second.</param>
        /// <param name = "secondEqualityComparer">The second equality comparer.</param>
        /// <param name = "secondSortingComparison">The second sorting comparison.</param>
        public Pair(
            TFirst first,
            IEqualityComparer<TFirst> firstEqualityComparer,
            Comparison<TFirst> firstSortingComparison,
            TSecond second,
            IEqualityComparer<TSecond> secondEqualityComparer,
            Comparison<TSecond> secondSortingComparison)
        {
            this.first = first;
            this.firstEqualityComparer = firstEqualityComparer ?? EqualityComparer<TFirst>.Default;
            firstComparison = firstSortingComparison ?? Comparer<TFirst>.Default.Compare;

            this.second = second;
            this.secondEqualityComparer = secondEqualityComparer ?? EqualityComparer<TSecond>.Default;
            secondComparison = secondSortingComparison ?? Comparer<TSecond>.Default.Compare;
        }

        /// <summary>
        ///    Gets or sets the first item in the pair.
        /// </summary>
        public TFirst First
        {
            get { return first; }
            set { first = value; }
        }

        /// <summary>
        ///    Gets or sets the second item in the pair.
        /// </summary>
        public TSecond Second
        {
            get { return second; }
            set { second = value; }
        }

        /// <summary>
        ///    Equality operator overload.
        /// </summary>
        /// <param name = "left">The left value.</param>
        /// <param name = "right">The right value.</param>
        /// <returns>
        ///    <c>true</c> when the instances are equal, otherwise <c>false</c>.
        /// </returns>
        public static bool operator ==(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            return Pair<TFirst, TSecond>.Equals(left, right);
        }

        /// <summary>
        ///    Greater-than operator overload.
        /// </summary>
        /// <param name = "left">The left value.</param>
        /// <param name = "right">The right value.</param>
        /// <returns>
        ///    <c>true</c> when the <paramref name = "left" /> is greater-than <paramref name = "right" />, otherwise <c>false</c>.
        /// </returns>
        public static bool operator >(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            if (ReferenceEquals(null, left))
            {
                return !ReferenceEquals(null, right);
            }

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        ///    Greater-than-or-equal-to operator overload.
        /// </summary>
        /// <param name = "left">The left value.</param>
        /// <param name = "right">The right value.</param>
        /// <returns>
        ///    <c>true</c> when the <paramref name = "left" /> is Greater-than-or-equal-to <paramref name = "right" />, otherwise <c>false</c>.
        /// </returns>
        public static bool operator >=(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            if (ReferenceEquals(null, left))
            {
                return !ReferenceEquals(null, right);
            }

            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        ///    Inequality operator overload.
        /// </summary>
        /// <param name = "left">The left value.</param>
        /// <param name = "right">The right value.</param>
        /// <returns>
        ///    <c>true</c> when the instances are not equal, otherwise <c>false</c>.
        /// </returns>
        public static bool operator !=(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            return !Pair<TFirst, TSecond>.Equals(left, right);
        }

        /// <summary>
        ///    Less-than operator overload.
        /// </summary>
        /// <param name = "left">The left value.</param>
        /// <param name = "right">The right value.</param>
        /// <returns>
        ///    <c>true</c> when the <paramref name = "left" /> is less-than <paramref name = "right" />, otherwise <c>false</c>.
        /// </returns>
        public static bool operator <(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            if (ReferenceEquals(null, left))
            {
                return !ReferenceEquals(null, right);
            }

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        ///    Less-than-or-equal-to operator overload.
        /// </summary>
        /// <param name = "left">The left value.</param>
        /// <param name = "right">The right value.</param>
        /// <returns>
        ///    <c>true</c> when the <paramref name = "left" /> is Less-than-or-equal-to <paramref name = "right" />, otherwise <c>false</c>.
        /// </returns>
        public static bool operator <=(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
        {
            if (ReferenceEquals(null, left))
            {
                return !ReferenceEquals(null, right);
            }

            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        ///    Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///    A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            TFirst firstClone = first;
            var firstAsCloneable = first as ICloneable;
            if (firstAsCloneable != null)
            {
                firstClone = (TFirst)firstAsCloneable.Clone();
            }

            TSecond secondClone = second;
            var secondAsCloneable = second as ICloneable;
            if (secondAsCloneable != null)
            {
                secondClone = (TSecond)secondAsCloneable.Clone();
            }

            return new Pair<TFirst, TSecond>(
                firstClone,
                firstEqualityComparer,
                firstComparison,
                secondClone,
                this.secondEqualityComparer,
                secondComparison);
        }

        /// <summary>
        ///    Compares the current object with another object of the same type.
        /// </summary>
        /// <param name = "other">An object to compare with this object.
        /// </param>
        /// <returns>
        ///    A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
        ///    <list type = "table">
        ///       <listheader>
        ///          <term>Value</term>
        ///          <description>Condition</description>
        ///       </listheader>
        ///       <item><term>Less than zero</term><description>This object is less than the <paramref name = "other" /> parameter.</description></item>
        ///       <item><term>Zero</term><description>This object is equal to <paramref name = "other" />.</description></item>
        ///       <item><term>Greater than zero</term><description>This object is greater than <paramref name = "other" />.</description></item>
        ///    </list>
        /// </returns>
        public int CompareTo(IPair<TFirst, TSecond> other)
        {
            if (other == null)
            {
                return 1;
            }

            int firstCompare = firstComparison(first, other.First);
            return firstCompare != 0 ? firstCompare : secondComparison(second, other.Second);
        }

        /// <summary>
        ///    Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///    <c>true</c> if the current object is equal to the <paramref name = "other" /> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name = "other">An object to compare with this object.
        /// </param>
        public bool Equals(IPair<TFirst, TSecond> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return firstEqualityComparer.Equals(First, other.First) &&
                   secondEqualityComparer.Equals(Second, other.Second);
        }

        /// <summary>
        ///    Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name = "obj">
        ///    An object to compare with this instance.
        /// </param>
        /// <returns>
        ///    A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
        ///    <list type = "table">
        ///       <listheader>
        ///          <term>Value</term>
        ///          <description>Condition</description>
        ///       </listheader>
        ///       <item><term>Less than zero</term><description>This object is less than the <paramref name = "obj" /> parameter.</description></item>
        ///       <item><term>Zero</term><description>This object is equal to <paramref name = "obj" />.</description></item>
        ///       <item><term>Greater than zero</term><description>This object is greater than <paramref name = "obj" />.</description></item>
        ///    </list>
        /// </returns>
        int IComparable.CompareTo(object obj)
        {
            return this.CompareTo(obj as IPair<TFirst, TSecond>);
        }

        /// <summary>
        ///    Determines whether the specified <see cref = "object" /> is equal to the current <see cref = "object" />.
        /// </summary>
        /// <returns>
        ///    True if the specified <see cref = "object" /> is equal to the current <see cref = "object" />; otherwise, false.
        /// </returns>
        /// <param name = "obj">The <see cref = "object" /> to compare with the current <see cref = "object" />.
        /// </param>
        /// <exception cref = "NullReferenceException">The <paramref name = "obj" /> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as IPair<TFirst, TSecond>);
        }

        /// <summary>
        ///    Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///    A hash code for the current <see cref = "Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.first.GetHashCode() * 397) ^ this.second.GetHashCode();
            }
        }
    }
}