//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System;
    using System.Linq;
    using Common;
    using Common.TestInfrastructure;
    using Library.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The ssl certificate tests.
    /// </summary>
    [TestClass]
    public class SslCertificateTests : PluginTestBase<SslCertificateTester>
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
        /// (Unit Test Method) when invoke the ssl certificate test and the result is clear.
        /// </summary>
        [TestMethod]
        public void WhenInvokeTheSslCertificateTestAndTheResultIsClear()
        {
            var target = Target.Create("https://www.hotmail.com");

            var vulns = this.ExecutePluginTestRequest(target);

            vulns.ShouldNotBeNull();
            vulns.Count.ShouldEqual(0);
        }

        /// <summary>
        /// (Unit Test Method) when invoke the ssl certificate test and the result is positive.
        /// </summary>
        [TestMethod]
        public void WhenInvokeTheSslCertificateTestAndTheResultIsPositive()
        {
            var target = Target.Create($"{Constants.VulnerabilitiesAddressSsl}");

            var vulns = this.ExecutePluginTestRequest(target);

            vulns.ShouldNotBeNull();
                vulns.Count.ShouldBeGreaterThanOrEqualTo(1);

            vulns.Count(v =>
                v.Evidence.EqualsOi("root certificate authority \"CN=localhost\" is not on the approved list."))
                .ShouldEqual(1);
        }
    }
}