//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.Linq;
    using System.Net;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Logger;
    using Library.PluginBase;

    /// <summary>
    /// The null injection tester. Tries to generate server side crashes.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Null injection",
        Description = "Test for side-effects of null characters in values")]
    public class NullInjectionTester : PluginBaseAbstract
    {
        /// <summary>
        /// The edit distance vuln type.
        /// </summary>
        private enum EditDistanceVulnType
        {
            /// <summary>
            /// The no vuln.
            /// </summary>
            NoVuln,

            /// <summary>
            /// The size.
            /// </summary>
            Size,

            /// <summary>
            /// The size and diff.
            /// </summary>
            SizeAndDiff,

            /// <summary>
            /// The diff.
            /// </summary>
            Diff
        }

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
                TestName = "Possible null injection",
                InjectionString = TestBaseHelper.LoadTestCase("NullInjectionTester", Context).ToArray()
            });
        }

        /// <inheritdoc/>
        protected override void CheckForVulnerabilities(
            WebRequestContext webRequestContext,
            TestCase testcase,
            string testedParam,
            string testValue)
        {
            HttpWebResponseHolder response = webRequestContext.ResponseHolder;

            if (response.StatusCode >= HttpStatusCode.OK &&
                response.StatusCode < HttpStatusCode.Ambiguous)
            {
                return;
            }

            try
            {
                var evidence = ExtractEvidence(response.ResponseContent, TestTarget.Content);

                if (!string.IsNullOrEmpty(evidence))
                {
                    this.AddVulnerability(
                        testcase.TestName,
                        testedParam,
                        testValue,
                        evidence,
                        response,
                        testcase,
                        VulnerabilityLevelEnum.Info);
                }
            }
            catch (OutOfMemoryException ex)
            {
                Logger.WriteError(ex);
            }
        }

        /// <summary>
        /// Checks for vulnerabilities between supposedly similar documents.
        /// </summary>
        /// <param name="pagebody">The actual page after an attack.</param>
        /// <param name="origpagebody">The original page contents.</param>
        /// <returns>A string describing the vulnerability evidence if a vulnerability exists, other string.Empty.</returns>
        private static string ExtractEvidence(string pagebody, string origpagebody)
        {
            if (string.IsNullOrEmpty(pagebody) || string.IsNullOrEmpty(origpagebody))
            {
                return "Null page found, possible a target crash hapenned!";
            }

            var testResult = PossibleVulnerabilities(origpagebody, pagebody);

            var evidence = string.Empty;
            var sizeDiff = Math.Abs(pagebody.Length - origpagebody.Length);

            switch (testResult)
            {
                case EditDistanceVulnType.NoVuln:
                    {
                        break;
                    }

                case EditDistanceVulnType.Size:
                    {
                        evidence = "Size difference: " + sizeDiff + " | Size Threshold: " +
                                   origpagebody.Length;
                        break;
                    }

                case EditDistanceVulnType.SizeAndDiff:
                    {
                        evidence = "Size difference: " + sizeDiff + " AND DISTANCE";
                        break;
                    }

                case EditDistanceVulnType.Diff:
                    {
                        evidence = "DISTANCE | Size difference: " + sizeDiff +
                                   " | Size Threshold High: ";
                        break;
                    }
            }

            return evidence;
        }

        /// <summary>
        /// Detects vulnerabilities by considering whether the response sizes and Levenshtein Distance meet thresholds
        /// </summary>
        /// <param name="a">The actual result following injection.</param>
        /// <param name="b">The expected result.</param>
        /// <returns>An EditDistanceVulnType describing the result.</returns>
        private static EditDistanceVulnType PossibleVulnerabilities(string a, string b)
        {
            uint lowSt = (uint)a.Length / 2;
            uint highSt = (uint)a.Length;

            // It appears to have always worked this way, need more comprehensive test data before making a change [BMx]
            uint lowDt = (uint)b.Length / 2;
            uint highDt = (uint)b.Length;

            var sizeDiff = Math.Abs(a.Length - b.Length);

            // if the size difference is big enough, just return
            // avoiding finding dist at all costs, very expensive call
            if (sizeDiff > highSt)
            {
                return EditDistanceVulnType.Size;
            }

            if (sizeDiff > lowSt)
            {
                var dist = EditDistance.ComputeDistance(a, b);

                // if low size difference and low distance, return
                if (dist > lowDt)
                {
                    return EditDistanceVulnType.SizeAndDiff;
                }

                // if low size difference but very high distance - we have a sneaky vuln maybe...
                if (dist > highDt)
                {
                    return EditDistanceVulnType.Diff;
                }
            }

            return EditDistanceVulnType.NoVuln;
        }
    }
}