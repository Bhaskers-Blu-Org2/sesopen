//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Globalization;
    using System.Net;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// Fuzzing cookies and looks for internal server errors.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Cookie Tester",
        Description = "Cookies fuzzer")]
    public sealed class CookieTester : PluginBaseAbstract
    {
        /// <summary>
        /// The random number.
        /// </summary>
        private static readonly Random RandomNumber = new Random();

        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="currentcontext">
        /// The current context.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        public override void Init(IContext currentcontext, ITarget target)
        {
            base.Init(currentcontext, target);
            TestCases.Add(new TestCase
            {
                TestName = "Random cookie strings",
                InjectionCookieCallBack = FuzzCookies,
                SaveRequestCookies = false,
                InjectionString = new[] { "microsoft" }
            });
        }

        /// <summary>
        /// The fuzz cookies.
        /// </summary>
        /// <param name="cookies">
        /// The cookies.
        /// </param>
        /// <returns>
        /// The <see cref="CookieCollection"/>.
        /// </returns>
        private static CookieCollection FuzzCookies(CookieCollection cookies)
        {
            var initialCookies = new Cookie[cookies.Count];
            for (var i = 0; i < cookies.Count; i++)
            {
                initialCookies[i] = cookies[i];
            }

            var fuzzedCookies = new CookieCollection();
            const string Punctuation = "+-_@%# \"\\";

            foreach (var initialCookie in initialCookies)
            {
                var cookieName = initialCookie.Name;
                string cookieValue;

                ////Remove the name 25% of the time
                if (RandomNumber.Next(0, 100) < 25)
                {
                    cookieName = "CookieRemoved" +
                                 RandomNumber.Next(0, 100000).ToString(CultureInfo.InvariantCulture);
                }

                ////A quarter of the time we fuzz it
                if (RandomNumber.Next(0, 100) < 25)
                {
                    var temp = string.Empty;
                    var len = RandomNumber.Next(0, 257);

                    for (var i = 0; i < len; i++)
                    {
                        var toss = RandomNumber.Next(0, 3);
                        switch (toss)
                        {
                            case 0:
                                temp +=
                                    ((char)(RandomNumber.Next(0, 26) + 'a')).ToString(
                                        CultureInfo.InvariantCulture);
                                break;

                            case 1:
                                temp +=
                                    ((char)(RandomNumber.Next(0, 10) + '0')).ToString(
                                        CultureInfo.InvariantCulture);
                                break;

                            case 2:
                                temp +=
                                    Punctuation[RandomNumber.Next(0, Punctuation.Length)].ToString(
                                        CultureInfo.InvariantCulture);
                                break;
                        }
                    }

                    ////30% of the time set the string to empty (covering the null case)
                    if (RandomNumber.Next(0, 100) < 30)
                    {
                        temp = string.Empty;
                    }

                    if (initialCookie.Value.Contains("&"))
                    {
                        cookieValue = string.Empty;
                        var parts = initialCookie.Value.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var part in parts)
                        {
                            var pair = part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            cookieValue += RandomNumber.Next(0, 100) < 20
                                ? (pair.Length == 2
                                    ? (RandomNumber.Next(0, 2) == 0
                                        ? pair[0] + "=" + temp + "&"
                                        : temp + "=" + pair[1] + "&")
                                    : part + "&")
                                : part + "&";
                        }
                    }
                    else
                    {
                        cookieValue = temp;
                    }
                }
                else
                {
                    cookieValue = initialCookie.Value;
                }

                if (cookieValue.EndsWith("&") &&
                    cookieValue.Length > 1)
                {
                    cookieValue = cookieValue.Substring(0, cookieValue.Length - 1);
                }

                fuzzedCookies.Add(new Cookie(cookieName, cookieValue));
            }

            return fuzzedCookies;
        }
    }
}