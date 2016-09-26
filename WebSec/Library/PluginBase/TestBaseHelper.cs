//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.PluginBase
{
    using System.Collections.Generic;
    using System.Linq;
    using Engine;
    using Engine.Interfaces;

    /// <summary>
    /// The test base helper.
    /// </summary>
    public static class TestBaseHelper
    {
        /// <summary>
        ///  The magic value injected into the attack payload.
        /// </summary>
        public const string AttackSignature = "501337";

        /// <summary>
        /// The xss test cases.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The <see cref="IEnumerable" />.
        /// </returns>
        public static IEnumerable<TestCase> XssTestCases(ITarget target, IContext context)
        {
            var testCases = new List<TestCase>
            {
                new TestCase
                {
                    TestName = "DOM XSS - Script injection",
                    InjectionString = LoadTestCase("XssTestScriptInjection", context).ToArray(),
                    MatchString = "alert(" + AttackSignature + ")",
                    FilterResponseContentType = new[] { "text/html" }
                },

                // onload attack
                new TestCase
                {
                    TestName = "DOM XSS - HTML attribute attack",
                    InjectionString = LoadTestCase("XssTestAttributeOnLoad", context).ToArray(),
                    FilterResponseContentType = new[] { "text/html" },
                    MatchString = "onload"
                },

                // style attack
                new TestCase
                {
                    TestName = "DOM XSS - HTML style attribute attack",
                    InjectionString = LoadTestCase("XssTestAttributeOnStyle", context).ToArray(),
                    FilterResponseContentType = new[] { "text/html" },
                    MatchString = "style"
                },

                // on error attack
                new TestCase
                {
                    TestName = "DOM XSS - HTML onerror attribute attack",
                    InjectionString = LoadTestCase("XssTestAttributeOnError", context).ToArray(),
                    FilterResponseContentType = new[] { "text/html" },
                    MatchString = "onerror"
                },

                // source attack
                new TestCase
                {
                    TestName = "DOM XSS - HTML onerror attribute attack",
                    InjectionString = LoadTestCase("XssTestAttributeOnSrc", context).ToArray(),
                    FilterResponseContentType = new[] { "text/html" },
                    MatchString = "src"
                }
            };

            return testCases;
        }

        /// <summary>
        /// Enumerates load test case in this collection.
        /// </summary>
        /// <param name="payloadName">
        /// Name of the payload.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// An enumerator that allows for each to be used to process load test case in this collection.
        /// </returns>
        public static IEnumerable<string> LoadTestCase(string payloadName, IContext context)
        {
            return context.Payloads.LoadPayloads(payloadName);
        }

        /// <summary>
        /// The is param value numeric.
        /// </summary>
        /// <param name="urlParameter">
        /// The url parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsParamValueNumeric(UrlParameter urlParameter)
        {
            if (string.IsNullOrEmpty(urlParameter.ParameterValue))
            {
                return false;
            }

            double value;
            return double.TryParse(urlParameter.ParameterValue, out value);
        }
    }
}