//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    /// <summary>
    /// Factory pattern responsible for instantiating a caching implementation.
    /// </summary>
    internal static class ProxyCacheFactory
    {
        /// <summary>
        /// Returns a caching implementation.
        /// </summary>
        /// <param name="enabled">A value indicating whether the cache should be enabled.</param>
        /// <param name="intervalInSeconds">The lifetime in seconds that an item should remain in the cache.</param>
        /// <returns>The appropriate caching implementation.</returns>
        internal static FiddlerProxyCache Create(bool enabled, int intervalInSeconds)
        {
            return enabled && intervalInSeconds > 0
                ? (FiddlerProxyCache) new DefaultProxyCache(intervalInSeconds)
                : new NullProxyCache();
        }
    }
}