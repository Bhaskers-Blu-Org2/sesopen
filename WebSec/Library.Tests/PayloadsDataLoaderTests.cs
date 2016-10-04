//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Payloads;
    using PluginBase;
    using WebSec.Common;
    using WebSec.Common.TestInfrastructure;
    using Constants = Library.Constants;

    /// <summary>
    /// (Unit Test Class) the payloads data loader tests.
    /// </summary>
    [TestClass]
    public class PayloadsDataLoaderTests
    {
        /// <summary>
        /// (Unit Test Method) tests load payloads.
        /// </summary>
        [TestMethod]
        public void TestLoadPayloads()
        {
            // load the payloads
            var payloadsDataLoader = new PayloadsDataLoader(Constants.PayloadsQuickDataFolder);
            payloadsDataLoader.LoadPayloads();

            IEnumerable<Type> plugins = ReflectionHelper.GetAllTypes<PluginBaseAbstract>(Constants.PluginDllName);
            int countCompareValue = plugins.Count(p => PayloadsHelper.IsPayloadDataGenerated(Constants.PayloadsQuickDataFolder, p.Name));

            payloadsDataLoader.Payloads.Count.ShouldBeGreaterThanOrEqualTo(countCompareValue);
        }
    }
}