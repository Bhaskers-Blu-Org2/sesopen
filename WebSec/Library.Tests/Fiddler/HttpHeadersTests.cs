//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Fiddler
{
    using System;
    using System.Collections.Generic;
    using Library.Fiddler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) a HTTP headers tests.
    /// </summary>
    [TestClass]
    public class HttpHeadersTests
    {
        /// <summary>
        /// (Unit Test Method) exists and equals empty list.
        /// </summary>
        [TestMethod]
        public void ExistsAndEqualsEmptyList()
        {
            // an empty collection
            var items = new HttpHeaders();
            items.ExistsAndEquals("key", "value").ShouldBeFalse();
        }

        /// <summary>
        /// (Unit Test Method) exists and equals positive cases.
        /// </summary>
        [TestMethod]
        public void ExistsAndEqualsPositiveCases()
        {
            var items = new HttpHeaders
            {
                new KeyValuePair<string, string>("item1", "value1"),
                new KeyValuePair<string, string>("item2", "value2")
            };

            // positive cases, case insensitive
            items.ExistsAndEquals("item1", "value1").ShouldBeTrue();
            items.ExistsAndEquals("ITEM1", "value1").ShouldBeTrue();
            items.ExistsAndEquals("item1", "VALUE1").ShouldBeTrue();
            items.ExistsAndEquals("item", "value1").ShouldBeFalse();

            // negative case, non matching value
            items.ExistsAndEquals("item1", "some other value").ShouldBeFalse();

            // negative case, non matching key
            items.ExistsAndEquals("unknown key", "value1").ShouldBeFalse();

            // negative cases, spread of null and empty values
            items.ExistsAndEquals(string.Empty, string.Empty).ShouldBeFalse();
            items.ExistsAndEquals(null, string.Empty).ShouldBeFalse();
            items.ExistsAndEquals(string.Empty, null).ShouldBeFalse();
            items.ExistsAndEquals(null, null).ShouldBeFalse();
        }

        /// <summary>
        /// (Unit Test Method) converts this object to a HTTP header string empty list.
        /// </summary>
        [TestMethod]
        public void ToHttpHeaderStringEmptyList()
        {
            // an empty collection
            var items = new HttpHeaders();
            items.ToHttpHeaderString().ShouldEqual(string.Empty);
        }

        /// <summary>
        /// (Unit Test Method) converts this object to a HTTP header string positive cases.
        /// </summary>
        [TestMethod]
        public void ToHttpHeaderStringPositiveCases()
        {
            var items = new HttpHeaders
            {
                new KeyValuePair<string, string>("item1", "value1"),
                new KeyValuePair<string, string>("item2", string.Empty),
                new KeyValuePair<string, string>("item3", null),
                new KeyValuePair<string, string>("ITEM4", "value2")
            };

            items.ToHttpHeaderString().ShouldEqual(
                @"item1: value1" + Environment.NewLine +
                "item2: " + Environment.NewLine +
                "item3: " + Environment.NewLine +
                "ITEM4: value2" + Environment.NewLine);
        }
    }
}