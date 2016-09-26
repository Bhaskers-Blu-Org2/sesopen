//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Payloads;
    using Plugins;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// Payloads tests.
    /// </summary>
    [TestClass]
    [DeploymentItem("Payloads", "Payloads")]
    public class PayloadsTests
    {
        /// <summary>
        /// (Unit Test Method) tests load payloads.
        /// </summary>
        [TestMethod]
        public void TestLoadPayloads()
        {
            var accessControlAllowMethodsDetector = new AccessControlAllowMethodsDetector();

            // load the payloads
            IPayloads payloads = new Payloads(Library.Constants.PayloadsQuickDataFolder);
            payloads.LoadPayloads(accessControlAllowMethodsDetector.GetType().Name).ShouldNotBeNull();
        }
    }
}