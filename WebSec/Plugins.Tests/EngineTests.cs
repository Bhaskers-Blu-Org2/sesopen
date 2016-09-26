//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common;
    using Common.TestInfrastructure;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// (Unit Test Class) an engine tests.
    /// </summary>
    [TestClass]
    public class EngineTests
    {
        /// <summary>
        /// Class cleanup.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestCleanup.Cleanup();
        }

        /// <summary>
        /// (Unit Test Method) tests start engine vulnerabilities found.
        /// </summary>
        [TestMethod]
        public void TestStartEngineVulnerabilitiesFound()
        {
            var urls = new[]
            {
                "http://joirwin-ssbank3284.cloudapp.net:2034/Account/Login.aspx?a=1",
                "http://joirwin-ssbank3284.cloudapp.net:2034/ActionDone.aspx?Title=Application%20Completed&Text=1",
                "http://joirwin-ssbank3284.cloudapp.net:2034/ViewPage.aspx?Page=test"
            };

            var activePlugins = new Dictionary<string, bool>
            {
                ["DomXssTest"] = true,
                ["InformationLeakageDetector"] = true,
                ["DirectoryTraversalTest"] = true
            };

            var uniqueVulnerabilities = new HashSet<Vulnerability>();

            IEnumerable<Vulnerability> vulnerabilities = Engine.StartEngine(
                urls,
                activePlugins,
                false,
                true);

            foreach (var vulnerability in vulnerabilities)
            {
                if (uniqueVulnerabilities.Any(v => v.Title.EqualsOi(vulnerability.Title)))
                {
                    continue;
                }

                uniqueVulnerabilities.Add(vulnerability);
            }

            vulnerabilities.Count().ShouldBeGreaterThan(4);
        }

        /// <summary>
        /// (Unit Test Method) tests start engine run only active plugins.
        /// </summary>
        [TestMethod]
        public void TestStartEngineRunOnlyActivePlugins()
        {
            var urls = new[] { $"{Constants.VulnerabilitiesAddress}SimplePageWithForm.html?q=test" };

            IReadOnlyDictionary<string, bool> activePlugins = LoadActivePluginsConfiguration();

            IEnumerable<Vulnerability> vulnerabilities = Engine.StartEngine(
                urls,
                activePlugins,
                false,
                true);

            vulnerabilities.Count().ShouldBeGreaterThan(3);
        }

        /// <summary>
        /// Loads active plugins configuration.
        /// </summary>
        /// <returns>
        /// The active plugins configuration.
        /// </returns>
        private IReadOnlyDictionary<string, bool> LoadActivePluginsConfiguration()
        {
            var ret = new Dictionary<string, bool>();

            string[] configurationContentLines = File.ReadAllText(Library.Constants.ActivePluginsFilePath)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in configurationContentLines)
            {
                string[] keyValuePair = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                ret[keyValuePair[0]] = bool.Parse(keyValuePair[1]);
            }

            return ret;
        }
    }
}