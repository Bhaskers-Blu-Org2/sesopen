//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser.Interfaces
{
    /// <summary>
    /// Describes the methods and properties of Process.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Gets a value indicating whether the associated process has exited.
        /// </summary>
        bool HasExited { get; }

        /// <summary>
        /// Gets the unique identifier for the associated process.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the unique identifier for the parent process.
        /// </summary>
        int ParentId { get; }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        string ProcessName { get; }

        /// <summary>
        /// Gets the amount of private memory allocated to the associated process.
        /// </summary>
        long PrivateMemorySize64 { get; }

        /// <summary>
        /// Immediately stops the associated process.
        /// </summary>
        void Kill();

        /// <summary>
        /// Instructs the IProcess component to wait indefinitely for the associated process to exit.
        /// </summary>
        void WaitForExit();
    }
}