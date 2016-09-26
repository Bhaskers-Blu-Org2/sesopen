//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System.Diagnostics;
    using Common.Validation;
    using Interfaces;

    /// <summary>
    /// Provides access to local processes and enables you to stop local system processes.
    /// </summary>
    public class ProcessWrapper : IProcess
    {
        /// <summary>
        /// The process.
        /// </summary>
        private readonly Process process;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessWrapper" /> class.
        /// </summary>
        /// <param name="process">the underlying process.</param>
        /// <param name="parentId">The process Id of the process that created the underlying process.</param>
        public ProcessWrapper(Process process, int parentId)
        {
            Require.NotNull(() => process);

            this.process = process;

            // non-dynamic values are cached. This avoids potential timing issues when using these properties 
            // after a process has exited.
            this.ParentId = parentId;
            this.ProcessName = process.ProcessName;
            this.Id = process.Id;
        }

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string ProcessName { get; }

        /// <inheritdoc />
        public long PrivateMemorySize64
        {
            get
            {
                this.process.Refresh();
                return this.process.PrivateMemorySize64;
            }
        }

        /// <inheritdoc />
        public int ParentId { get; }

        /// <inheritdoc />
        public bool HasExited
        {
            get
            {
                this.process.Refresh();
                return this.process.HasExited;
            }
        }

        /// <inheritdoc />
        public void Kill()
        {
            this.process.Kill();
        }

        /// <inheritdoc />
        public void WaitForExit()
        {
            this.process.WaitForExit();
        }
    }
}