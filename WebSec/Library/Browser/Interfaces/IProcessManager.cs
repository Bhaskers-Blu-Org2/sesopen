//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser.Interfaces
{
    /// <summary>
    /// Describes a type that wraps selected members of the .NET System.Diagnostics.Process type.
    /// </summary>
    /// <remarks>
    /// This is done to simplify mocking of the System.Diagnostics.Process type, since the base implementation does not enable this.
    /// </remarks>
    public interface IProcessManager
    {
        /// <summary>
        /// Returns a process based on its window title.
        /// </summary>
        /// <param name="windowTitle">The exact title text to look for.</param>
        /// <returns>The process.</returns>
        IProcess GetProcessByWindowTitle(string windowTitle);
    }
}