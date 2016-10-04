//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System.Collections.Generic;
    using Browser;
    using Browser.Interfaces;
    using Common;
    using Payloads;

    /// <summary>
    /// Objects registration.
    /// </summary>
    public static class RegisterObjects
    {
        /// <summary>
        /// Register the objects.
        /// </summary>
        /// <param name="useFullPayloadsData">
        /// use full payloads data
        /// </param>
        public static void Register(bool useFullPayloadsData)
        {
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };

            ObjectResolver.RegisterInstance<IPayloads>(
                new Payloads(useFullPayloadsData
                    ? Library.Constants.PayloadsFullDataFolder
                    : Library.Constants.PayloadsQuickDataFolder));

            ObjectResolver.RegisterInstance<IProcessManager>(new ProcessManager());
            ObjectResolver.RegisterInstance<IBrowserManager>(new BrowserManager(browserInstances));
        }
    }
}