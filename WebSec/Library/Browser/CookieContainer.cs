//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// This class is similar to System.Net.CookieContainer. However, System.Net.CookieContainer enforces strict validations on the values of various
    /// Cookie properties, which makes it not suitable for us.
    /// </summary>
    public sealed class CookieContainer
    {
        /// <summary>
        /// The syncLock to make operations on underlying collections thread safe.
        /// </summary>
        private readonly object syncLock;

        /// <summary>
        /// Internal container that holds all the Cookies.
        /// </summary>
        private CookieCollection cookieCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieContainer"/> class.
        /// </summary>
        public CookieContainer() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieContainer"/> class.
        /// </summary>
        /// <param name="cookieCollection">A cookie collection.</param>
        public CookieContainer(CookieCollection cookieCollection)
        {
            this.cookieCollection = cookieCollection ?? new CookieCollection();
            this.syncLock = new object();
        }

        /// <summary>
        /// Gets the count of cookies currently in the container.
        /// </summary>
        public int Count => this.cookieCollection.Count;

        /// <summary>
        /// Parse out a list of cookies from "Set-Cookie" header.
        /// </summary>
        /// <param name="requestUri">The request uri associated with the response "Set-Cookie" header.</param>
        /// <param name="setCookieHeader">The value of "Set-Cookie" header.</param>
        /// <returns>A collection of cookies.</returns>
        public static CookieCollection ParseResponseCookie(Uri requestUri, string setCookieHeader)
        {
            CookieCollection cookies = new CookieCollection();
            string[] cookieItems = setCookieHeader.Split(new string[] { ";," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var cookieItem in cookieItems)
            {
                string[] items = cookieItem.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                var cookie = new System.Net.Cookie
                {
                    Domain = requestUri?.Host ?? string.Empty,
                    Path = "/"
                };

                foreach (var item in items)
                {
                    string[] pairs = item.Split('=');
                    switch (pairs[0].ToLowerInvariant().Trim())
                    {
                        case "domain":
                            cookie.Domain = pairs[1].Trim();
                            break;

                        case "httponly":
                            cookie.HttpOnly = true;
                            break;

                        case "expires":
                            cookie.Expires = DateTime.Parse(pairs[1]);
                            break;

                        case "path":
                            cookie.Path = pairs[1].Trim();
                            break;

                        case "comment":
                            cookie.Comment = pairs[1].Trim();
                            break;

                        case "max-age":
                            cookie.Expires = DateTime.Now.AddSeconds(int.Parse(pairs[1]));
                            break;

                        case "version":
                            // The default value for the Version property is 0, complying with the original Netscape specification.
                            // If the value is explicitly set to 1, then this Cookie must conform to RFC 2109 when one uses
                            // System.Net.CookieContainer.Add() methods, which means that the "Domain" value must start with "."
                            // This is not the case all the time.
                            cookie.Version = int.Parse(pairs[1].Trim());
                            break;

                        case "secure":
                            cookie.Secure = true;
                            break;

                        default:
                            cookie.Name = pairs[0].Trim();
                            cookie.Value = pairs.Length > 1
                                ? cookie.Value = item.Substring(pairs[0].Length + 1)
                                : string.Empty;
                            break;
                    }
                }

                cookies.Add(cookie);
            }

            return cookies;
        }

        /// <summary>
        /// Parse out a list of cookies from "Set-Cookie" header.
        /// </summary>
        /// <param name="setCookieHeader">The value of "Set-Cookie" header.</param>
        /// <returns>A collection of cookies.</returns>
        public static CookieCollection ParseResponseCookie(string setCookieHeader)
        {
            return ParseResponseCookie(null, setCookieHeader);
        }

        /// <summary>
        /// Parse out a list of cookies from "Cookie" header.
        /// </summary>
        /// <param name="cookieHeader">The value of "Set-Cookie" header.</param>
        /// <returns>A collection of cookies.</returns>
        public static CookieCollection ParseRequestCookie(string cookieHeader)
        {
            CookieCollection collection = new CookieCollection();
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                return collection;
            }

            string[] items = cookieHeader.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                string[] pairs = item.Split('=');

                Cookie cookie = new Cookie
                {
                    Name = pairs[0].Trim(),
                    Value = pairs.Length > 1 ? item.Substring(pairs[0].Length + 1) : string.Empty
                };

                collection.Add(cookie);
            }

            return collection;
        }

        /// <summary>
        /// Add a cookie to the container.
        /// </summary>
        /// <param name="cookie">The cookie to be added.</param>
        public void Add(Cookie cookie)
        {
            lock (this.syncLock)
            {
                this.cookieCollection.Add(cookie);
            }
        }

        /// <summary>
        /// Add a collection of cookies to the container.
        /// </summary>
        /// <param name="cookies">The cookie collection to be added.</param>
        public void Add(CookieCollection cookies)
        {
            lock (this.syncLock)
            {
                this.cookieCollection.Add(cookies);
            }
        }

        /// <summary>
        /// Add a cookie to the container.
        /// </summary>
        /// <param name="uri">The uri the cookie associated with.</param>
        /// <param name="cookie">The cookie to be added.</param>
        public void Add(Uri uri, Cookie cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie.Domain))
            {
                cookie.Domain = uri.Host;
            }

            if (string.IsNullOrWhiteSpace(cookie.Path))
            {
                cookie.Path = uri.AbsolutePath;
            }

            if (uri.Scheme == Uri.UriSchemeHttps)
            {
                cookie.Secure = true;
            }

            this.Add(cookie);
        }

        /// <summary>
        /// Add a collection of cookies parsed out from "Set-Cookie" header.
        /// </summary>
        /// <param name="responseCookieString">The value of "Set-Cookie" header.</param>
        public void SetCookies(string responseCookieString)
        {
            var cookies = ParseResponseCookie(responseCookieString);
            for (int i = 0; i < cookies.Count; i++)
            {
                this.Add(cookies[i]);
            }
        }

        /// <summary>
        /// Get a list of cookies that apply to the given uri.
        /// </summary>
        /// <param name="uri">The request uri.</param>
        /// <returns>A list of cookies.</returns>
        public IEnumerable<Cookie> GetCookies(Uri uri)
        {
            lock (this.syncLock)
            {
                var collection =
                    this.cookieCollection.OfType<Cookie>()
                        .Where(c => uri.Host.EndsWith(c.Domain) && uri.AbsolutePath.StartsWith(c.Path));
                if (uri.Scheme == Uri.UriSchemeHttp)
                {
                    collection = collection.Where(c => !c.Secure);
                }

                return collection;
            }
        }

        /// <summary>
        /// Merge a list of request cookies with cookies found in this container that apply to the request uri. The values of cookies found in this container win when there are cookie name conflicts.
        /// </summary>
        /// <param name="requestUri">The request uri.</param>
        /// <param name="originalRequestCookies">The request cookies.</param>
        /// <returns>A new cookie collection that contains both the original requests and cookies found in this container.</returns>
        public CookieCollection MergeRequestCookies(Uri requestUri, CookieCollection originalRequestCookies)
        {
            CookieCollection requestCookies = new CookieCollection { originalRequestCookies };

            foreach (var cookie in this.GetCookies(requestUri))
            {
                requestCookies.Add(new Cookie(cookie.Name, cookie.Value));
            }

            return requestCookies;
        }

        /// <summary>
        /// Retrieve all the cookies with a given name.
        /// </summary>
        /// <param name="name">The "Name" property of a cookie.</param>
        /// <returns>A list of cookies with the given name.</returns>
        public IEnumerable<Cookie> GetCookiesByName(string name)
        {
            lock (this.syncLock)
            {
                var collection = this.cookieCollection.OfType<Cookie>().Where(c => c.Name.Equals(name));
                return collection;
            }
        }

        /// <summary>
        /// Remove a particular cookie from the container.
        /// </summary>
        /// <param name="cookie">The cookie to be removed.</param>
        /// <returns>The cookie removed from the container or null if not found.</returns>
        public Cookie RemoveCookie(Cookie cookie)
        {
            lock (this.syncLock)
            {
                var list = this.cookieCollection.OfType<Cookie>().ToList();
                var existingCookie =
                    this.cookieCollection.OfType<Cookie>().ToList().FirstOrDefault(
                        c =>
                            c.Secure == cookie.Secure &&
                            c.Domain.Equals(cookie.Domain, StringComparison.InvariantCultureIgnoreCase) &&
                            c.Path.Equals(cookie.Path) &&
                            c.Name.Equals(cookie.Name) && c.Secure == cookie.Secure);

                if (existingCookie != null)
                {
                    list.Remove(existingCookie);
                    this.cookieCollection = new CookieCollection();

                    foreach (Cookie c in list)
                    {
                        this.cookieCollection.Add(c);
                    }
                }

                return existingCookie;
            }
        }

        /// <summary>
        /// Remove all cookies in this container
        /// </summary>
        public void RemoveAllCookies()
        {
            lock (this.syncLock)
            {
                this.cookieCollection = new CookieCollection();
            }
        }
    }
}