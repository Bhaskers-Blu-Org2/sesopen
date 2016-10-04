//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Reflection helper.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets all types in this collection.
        /// </summary>
        /// <typeparam name="T">
        /// Generic type parameter.
        /// </typeparam>
        /// <param name="assemblyName">
        /// Name of the assembly.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process all types in this collection.
        /// </returns>
        public static IEnumerable<Type> GetAllTypes<T>(string assemblyName)
        {
            return Assembly.LoadFrom(assemblyName).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(T)) || t.GetInterfaces().Any(i => i == typeof(T)));
        }
    }
}