//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Logger;

    /// <summary>
    /// Implementation that wraps selected members of the .NET Process type.
    /// </summary>
    /// <seealso cref="T:Microsoft.Search.Security.Common.BrowserPool.Interfaces.IProcessManager"/>
    public class ProcessManager : IProcessManager
    {
        /// <inheritdoc/>
        public IProcess GetProcessByWindowTitle(string windowTitle)
        {
            int iterations = 0;
            var response = GetProcessByWindowTitleImplementation(windowTitle);

            while (response == null && iterations++ < 3)
            {
                Logger.WriteDebug("GetProcessByWindowTitle: Attempt {0} failed, will retry in 1 second.", iterations);
                Thread.Sleep(1000);

                response = GetProcessByWindowTitleImplementation(windowTitle);
            }

            return response;
        }

        /// <summary>
        /// Gets process by window title implementation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="windowTitle">
        ///     The window title.
        /// </param>
        /// <returns>
        /// The process by window title implementation.
        /// </returns>
        private static IProcess GetProcessByWindowTitleImplementation(string windowTitle)
        {
            var handleWindow = NativeMethods.FindWindowByCaption(null, windowTitle);

            if (handleWindow != IntPtr.Zero)
            {
                uint handleProcess;
                NativeMethods.GetWindowThreadProcessId(handleWindow, out handleProcess);

                if (handleProcess == 0)
                {
                    throw new InvalidOperationException("Could not get process information for known process");
                }

                var process = Process.GetProcessById((int)handleProcess);

                return new ProcessWrapper(process, GetParentProcessIdentifier(process));
            }

            return null;
        }

        /// <summary>
        /// Gets the parent process identifier.
        /// </summary>
        /// <exception cref="Win32Exception">
        ///     Thrown when a window 32 error condition occurs.
        /// </exception>
        /// <param name="process">
        ///     The process.
        /// </param>
        /// <returns>
        /// The parent process identifier.
        /// </returns>
        private static int GetParentProcessIdentifier(Process process)
        {
            // Based on an approach from http://blogs.msdn.com/b/toffer/archive/2005/07/21/441540.aspx
            var processEntry = new NativeMethods.PROCESSENTRY32();
            IntPtr handleToSnapshot = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.TH32CS_SNAPPROCESS, 0);

            try
            {
                if (!NativeMethods.Process32First(handleToSnapshot, processEntry))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (process.Id == processEntry.ProcessId)
                {
                    return processEntry.ParentProcessId;
                }

                while (NativeMethods.Process32Next(handleToSnapshot, processEntry))
                {
                    if (process.Id == processEntry.ProcessId)
                    {
                        return processEntry.ParentProcessId;
                    }
                }
            }
            catch (Win32Exception exception)
            {
                Logger.WriteWarning(exception);
            }
            finally
            {
                NativeMethods.CloseHandle(handleToSnapshot);
            }

            return 0;
        }
    }
}