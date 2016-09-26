//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common
{
    using System;

    /// <summary>
    /// Provides extensions methods to the DatTime class.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// The days in year.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int DaysInYear(this DateTime value)
        {
            return DateTime.IsLeapYear(value.Year) ? 366 : 365;
        }
    }
}