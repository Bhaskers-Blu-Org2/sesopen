//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Fiddler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Library.Fiddler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) a fiddler tests.
    /// </summary>
    [TestClass]
    public class FiddlerTests
    {
        /// <summary>
        /// (Unit Test Method) simple HTTP request object well formed.
        /// </summary>
        [TestMethod]
        public void SimpleHttpRequestObjectWellFormed()
        {
            IHttpRequest request = new SimpleHttpRequest(
                "http://host1?a=1",
                "POST", 
                "HTTP/1.1",
                "content",
                new HttpHeaders
                {
                    new KeyValuePair<string, string>("User-Agent", "user agent value 1"),
                    new KeyValuePair<string, string>("Cookie", "key=value"),
                    new KeyValuePair<string, string>("Content-Type", "*/*"),
                    new KeyValuePair<string, string>("Host", "http://host1"),
                    new KeyValuePair<string, string>("header1", "value1"),
                    new KeyValuePair<string, string>("header2", string.Empty)
                });

            request.RequestContent.ShouldEqual("content");
            request.RequestContentType.ShouldEqual("*/*");
            request.RequestCookies.ShouldEqual("key=value");
            request.RequestHost.ShouldEqual("http://host1");
            request.RequestHttpMethod.ShouldEqual("POST");
            request.RequestHttpVersion.ShouldEqual("HTTP/1.1");
            request.RequestUrl.ShouldEqual("http://host1?a=1");
            request.RequestUserAgent.ShouldEqual("user agent value 1");
            request.RequestHeaders.Distinct().Count().ShouldEqual(6);
            request.RequestHeaders.Single(h => h.Key.Equals("header1")).Value.ShouldEqual("value1");
            request.RequestHeaders.Single(h => h.Key.Equals("header2")).Value.ShouldEqual(string.Empty);
        }

        /// <summary>
        /// (Unit Test Method) simple HTTP request object with an empty URL.
        /// </summary>
        [TestMethod]
        public void SimpleHttpRequestObjectWithAnEmptyUrl()
        {
            var exception = Catch.Exception(() => new SimpleHttpRequest(
                string.Empty,
                "POST", 
                "HTTP/1.1",
                null,
                new HttpHeaders
                {
                    new KeyValuePair<string, string>("header1", "value1")
                }));

            // It should throw the expected exception type
            exception.ShouldBeOfType<ArgumentNullException>();

            // It should specify the expected parameter name
            ((ArgumentNullException)exception).ParamName.ShouldEqual("url");
        }

        /// <summary>
        /// (Unit Test Method) simple HTTP request object with an empty method.
        /// </summary>
        [TestMethod]
        public void SimpleHttpRequestObjectWithAnEmptyMethod()
        {
            var exception = Catch.Exception(() => new SimpleHttpRequest(
                "http://host1?a=1", 
                string.Empty,
                "HTTP/1.1",
                null,
                new HttpHeaders
                {
                    new KeyValuePair<string, string>("header1", "value1")
                }));

            // It should throw the expected exception type
            exception.ShouldBeOfType<ArgumentNullException>();

            // It should specify the expected parameter name
            ((ArgumentNullException)exception).ParamName.ShouldEqual("httpMethod");
        }

        /// <summary>
        /// (Unit Test Method) simple HTTP request object with an empty version.
        /// </summary>
        [TestMethod]
        public void SimpleHttpRequestObjectWithAnEmptyVersion()
        {
            var exception = Catch.Exception(() => new SimpleHttpRequest(
                "http://host1?a=1", 
                "GET",
                string.Empty,
                null,
                new HttpHeaders
                {
                    new KeyValuePair<string, string>("header1", "value1")
                }));

            // It should throw the expected exception type
            exception.ShouldBeOfType<ArgumentNullException>();

            // It should specify the expected parameter name
            ((ArgumentNullException)exception).ParamName.ShouldEqual("httpVersion");
        }

        /// <summary>
        /// (Unit Test Method) simple HTTP request object with missing headers.
        /// </summary>
        [TestMethod]
        public void SimpleHttpRequestObjectWithMissingHeaders()
        {
            IHttpRequest request = new SimpleHttpRequest(
                "http://host1?a=1", 
                "POST", 
                "HTTP/1.1",
                null,
                new HttpHeaders());

            request.RequestContent.ShouldBeNull();
            request.RequestContentType.ShouldEqual(string.Empty);
            request.RequestCookies.ShouldEqual(string.Empty);
            request.RequestHost.ShouldEqual(string.Empty);
            request.RequestHttpMethod.ShouldEqual("POST");
            request.RequestHttpVersion.ShouldEqual("HTTP/1.1");
            request.RequestUrl.ShouldEqual("http://host1?a=1");
            request.RequestUserAgent.ShouldEqual(string.Empty);
            request.RequestHeaders.Count().ShouldEqual(0);
        }
    }
}