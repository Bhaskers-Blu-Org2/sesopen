//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using Engine;
    using Engine.Interfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// Test class of BackScatterScannerLib.Engine.Target
    /// </summary>
    [TestClass]
    public class TargetTests
    {
        /// <summary>
        /// (Unit Test Method) tests target.
        /// </summary>
        [TestMethod]
        public void TestTarget()
        {
            ITarget target = Target.Create("http://www.bing.com?q=test");
            target.UrlBase.ShouldEqual("http://www.bing.com");
            target.Params.Count.ShouldEqual(1);
            target.Params.ContainsKey("q");
        }
    }
}