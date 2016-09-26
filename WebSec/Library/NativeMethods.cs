//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A place to keep native interop methods.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// The 32 create struct snap process.
        /// </summary>
        public const UInt32 TH32CS_SNAPPROCESS = 0x00000002;

        /// <summary>
        /// The software restore.
        /// </summary>
        public const int SW_RESTORE = 9;

        /// <summary>
        /// Full pathname of the maximum file.
        /// </summary>
        private const int MAX_PATH = 260;

        /// <summary>
        /// Enumerate windows procedure.
        /// </summary>
        /// <param name="hWnd">
        /// The window.
        /// </param>
        /// <param name="lParam">
        /// The parameter.
        /// </param>
        /// <returns>
        /// true if succeeded , false otherwise
        /// </returns>
        public delegate bool EnumWindowsProc(IntPtr hWnd, ref IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, out UInt32 lpdwProcessId);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowByCaption(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateToolhelp32Snapshot([In] UInt32 dwFlags, [In] UInt32 th32ProcessID);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool Process32First([In] IntPtr hSnapshot, [In, Out] PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool Process32Next([In] IntPtr hSnapshot, [In, Out] PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// A rectangle.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            /// <summary>
            /// The left.
            /// </summary>
            public int left;

            /// <summary>
            /// The top.
            /// </summary>
            public int top;

            /// <summary>
            /// The right.
            /// </summary>
            public int right;

            /// <summary>
            /// The bottom.
            /// </summary>
            public int bottom;
        }

        /// <summary>
        /// A process entry 32.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [BestFitMapping(false, ThrowOnUnmappableChar = true)]
        public class PROCESSENTRY32
        {
            /// <summary>
            /// The size.
            /// </summary>
            private UInt32 dwSize;

            /// <summary>
            /// The count usage.
            /// </summary>
            private UInt32 cntUsage;

            /// <summary>
            /// Identifier for the 32 process.
            /// </summary>
            private UInt32 th32ProcessID;

            /// <summary>
            /// Identifier for the 32 default heap.
            /// </summary>
            private IntPtr th32DefaultHeapID;

            /// <summary>
            /// Identifier for the 32 module.
            /// </summary>
            private UInt32 th32ModuleID;

            /// <summary>
            /// The count threads.
            /// </summary>
            private UInt32 cntThreads;

            /// <summary>
            /// Identifier for the 32 parent process.
            /// </summary>
            private UInt32 th32ParentProcessID;

            /// <summary>
            /// The PC priority class base.
            /// </summary>
            private Int32 pcPriClassBase;

            /// <summary>
            /// The flags.
            /// </summary>
            private UInt32 dwFlags;

            /// <summary>
            /// The executable file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH * sizeof(char))]
            private string szExeFile;

            /// <summary>
            /// Initializes a new instance of the <see cref="PROCESSENTRY32" /> class.
            /// </summary>
            public PROCESSENTRY32()
            {
                // IMPORTANT: Must set dwSize based on the size of an instance vs. the type.
                this.dwSize = (uint)Marshal.SizeOf(this);
                this.th32ProcessID = 0;
                this.th32ParentProcessID = 0;
            }

            /// <summary>
            /// Gets the identifier for this process.
            /// </summary>
            public int ProcessId
            {
                get { return (int)th32ProcessID; }
            }

            /// <summary>
            /// Gets the parent identifier for this process.
            /// </summary>
            public int ParentProcessId
            {
                get { return (int)th32ParentProcessID; }
            }
        }
    }
}