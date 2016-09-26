//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.PluginBase
{
    using System;

    /// <summary>
    /// Definition of runtime properties as applied to a TestBase type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the test base type.
        /// </summary>
        public TestBaseType TestBaseType { get; set; }

        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the test.
        /// </summary>
        public string Description { get; set; }
    }
}