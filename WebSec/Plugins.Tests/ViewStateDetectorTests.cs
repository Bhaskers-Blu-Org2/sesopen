//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System.Linq;
    using Common.TestInfrastructure;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The view state detector tests.
    /// </summary>
    [TestClass]
    public class ViewStateDetectorTests : PluginTestBase<ViewStateDetector>
    {
        /// <summary>
        /// The test_viewstate_mac_passive_vuln_positive.
        /// </summary>
        [TestMethod]
        public void Test_viewstate_mac_passive_vuln_positive()
        {
            // Update: ViewState MAC is now forced on, even if you specify otherwise at the page level. 
            // Because of this, the positive case can longer be verified on a machine with the latest patch level.
            // https://technet.microsoft.com/library/security/2905247

            // Setup
            var target = Target.Create("http://localhost:21316/vuln_viewstate_positive.aspx?q=attack_vector");

            var vulns = this.ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(1);
            vulns.All(x => x.Title == "ViewState does not have MAC enabled.").ShouldBeTrue();
        }

        /// <summary>
        /// The test_viewstate_mac_passive_vuln_negative.
        /// </summary>
        [TestMethod]
        public void Test_viewstate_mac_passive_vuln_negative()
        {
            // Setup
            var target = Target.Create("http://localhost:21316/vuln_viewstate_negative.aspx?q=attack_vector");

            var vulns = this.ExecutePluginDetectorRequest(target);

            // Validate
            vulns.Count.ShouldEqual(0);
        }
    }
}