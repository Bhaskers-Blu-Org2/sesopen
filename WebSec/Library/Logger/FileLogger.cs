//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Logger
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// File logger implementation.
    /// </summary>
    public sealed class FileLogger : ILogger
    {
        /// <summary>
        /// The info log extension.
        /// </summary>
        private const string InfoLogExtension = ".Info.log";

        /// <summary>
        /// The debug log extension.
        /// </summary>
        private const string DebugLogExtension = ".Debug.log";

        /// <summary>
        /// The warning log extension.
        /// </summary>
        private const string WarningLogExtension = ".Warning.log";

        /// <summary>
        /// The error log extension.
        /// </summary>
        private const string ErrorLogExtension = ".Error.log";

        /// <summary>
        /// Pathname of the current directory.
        /// </summary>
        private readonly string currentDirectory;

        /// <summary>
        /// Use this to assure no dead-locks.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Name of the current process.
        /// </summary>
        private readonly string processName;

        /// <summary>
        /// Initializes a new instance of the WebSec.Library.Logger.FileLogger class.
        /// </summary>
        public FileLogger()
        {
            processName = Process.GetCurrentProcess().ProcessName;
            currentDirectory = Environment.CurrentDirectory;
        }

        /// <inheritdoc/> 
        public bool Verbose { get; set; }

        /// <inheritdoc/> 
        public void WriteInfo(Exception ex)
        {
            WriteToFile(InfoLogExtension, FormatExceptionMessage(ex, string.Empty));
        }

        /// <inheritdoc/> 
        public void WriteInfo(string format, params object[] args)
        {
            WriteToFile(InfoLogExtension, FormatExceptionMessage(null, format, args));
        }

        /// <inheritdoc/> 
        public void WriteInfo(Exception ex, string format, params object[] args)
        {
            WriteToFile(InfoLogExtension, FormatExceptionMessage(ex, format, args));
        }

        /// <inheritdoc/> 
        public void WriteError(Exception ex)
        {
            WriteToFile(ErrorLogExtension, FormatExceptionMessage(ex, string.Empty));
        }

        /// <inheritdoc/> 
        public void WriteError(string format, params object[] args)
        {
            WriteToFile(ErrorLogExtension, FormatExceptionMessage(null, format, args));
        }

        /// <inheritdoc/> 
        public void WriteError(Exception ex, string format, params object[] args)
        {
            WriteToFile(ErrorLogExtension, FormatExceptionMessage(ex, format, args));
        }

        /// <inheritdoc/> 
        public void WriteDebug(Exception ex)
        {
            WriteToFile(DebugLogExtension, FormatExceptionMessage(ex, string.Empty));
        }

        /// <inheritdoc/> 
        public void WriteDebug(string format, params object[] args)
        {
            WriteToFile(DebugLogExtension, FormatExceptionMessage(null, format, args));
        }

        /// <inheritdoc/> 
        public void WriteDebug(Exception ex, string format, params object[] args)
        {
            WriteToFile(DebugLogExtension, FormatExceptionMessage(ex, format, args));
        }

        /// <inheritdoc/> 
        public void WriteWarning(Exception ex)
        {
            WriteToFile(WarningLogExtension, FormatExceptionMessage(ex, string.Empty));
        }

        /// <inheritdoc/> 
        public void WriteWarning(string format, params object[] args)
        {
            WriteToFile(WarningLogExtension, FormatExceptionMessage(null, format, args));
        }

        /// <inheritdoc/> 
        public void WriteWarning(Exception ex, string format, params object[] args)
        {
            WriteToFile(WarningLogExtension, FormatExceptionMessage(ex, format, args));
        }

        /// <summary>
        /// Formats an exception with a standard layout.
        /// </summary>
        /// <param name="ex">The exception to present.</param>
        /// <param name="format">An optional string format to append to the layout.</param>
        /// <param name="args">Optional parameters to apply to the format.</param>
        /// <returns>A string containing the formatted message.</returns>
        private string FormatExceptionMessage(Exception ex, string format, params object[] args)
        {
            var builder = new StringBuilder();

            if (ex != null)
            {
                builder.Append("Exception: ");
                builder.Append(ex.GetType().Name);
                builder.Append(" ");
                builder.Append(ex.Message);
                builder.Append(" ");

                if (ex is ArgumentException)
                {
                    builder.Append("ParamName: ");
                    builder.Append((ex as ArgumentException).ParamName);
                    builder.Append(" ");
                }

                builder.Append(ex.StackTrace);
                builder.Append(" ");
                builder.Append(ex.Source);
                builder.Append(" ");

                if (ex.InnerException != null)
                {
                    builder.Append("Inner Exception: ");
                    builder.Append(ex.InnerException.GetType().Name);
                    builder.Append(" ");
                    builder.Append(ex.InnerException.Message);
                    builder.Append(" ");
                }
            }

            if (!string.IsNullOrEmpty(format))
            {
                builder.Append(format.FormatIc(args));
            }

            var builderOutput = builder.ToString();

            if (Verbose)
            {
                Console.WriteLine(builderOutput);
            }

            return builderOutput;
        }

        /// <summary>
        /// Writes to file.
        /// </summary>
        /// <param name="logExtension">
        /// The log extension.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        private void WriteToFile(string logExtension, string text)
        {
            lock (lockObject)
            {
                using (
                    var streamWriter = new StreamWriter(
                        Path.Combine(currentDirectory, $"{processName}{logExtension}"), true))
                {
                    streamWriter.WriteLine($"{DateTime.Now}\t{text}");
                }
            }
        }
    }
}