//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Logger
{
    using System;

    /// <summary>
    /// A logger.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger FileLogger = new FileLogger();

        /// <summary>
        /// Gets or sets a value indicating whether the verbose.
        /// </summary>
        /// <value>
        /// true if verbose, false if not.
        /// </value>
        public static bool Verbose
        {
            get { return FileLogger.Verbose; }
            set { FileLogger.Verbose = value; }
        }

        /// <summary>
        /// Writes an exception to the info log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void WriteInfo(Exception ex)
        {
            FileLogger.WriteInfo(ex);
        }

        /// <summary>
        /// Writes an informational message to the service info log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteInfo(string format, params object[] args)
        {
            FileLogger.WriteInfo(format, args);
        }

        /// <summary>
        /// Writes an exception + informational message to the service info log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteInfo(Exception ex, string format, params object[] args)
        {
            FileLogger.WriteInfo(ex, format, args);
        }

        /// <summary>
        /// Writes an exception to the custom service error log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void WriteError(Exception ex)
        {
            FileLogger.WriteError(ex);
        }

        /// <summary>
        /// Writes an informational message to the custom service error log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteError(string format, params object[] args)
        {
            FileLogger.WriteError(format, args);
        }

        /// <summary>
        /// Writes an exception + informational message to the custom service error log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteError(Exception ex, string format, params object[] args)
        {
            FileLogger.WriteError(ex, format, args);
        }

        /// <summary>
        /// Writes an exception to the custom service debug log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void WriteDebug(Exception ex)
        {
            FileLogger.WriteDebug(ex);
        }

        /// <summary>
        /// Writes an informational message to the custom service debug log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteDebug(string format, params object[] args)
        {
            FileLogger.WriteDebug(format, args);
        }

        /// <summary>
        /// Writes an exception + informational message to the custom service debug log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteDebug(Exception ex, string format, params object[] args)
        {
            FileLogger.WriteDebug(ex, format, args);
        }

        /// <summary>
        /// Writes an exception to the custom service warning log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void WriteWarning(Exception ex)
        {
            FileLogger.WriteWarning(ex);
        }

        /// <summary>
        /// Writes an informational message to the custom service warning log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteWarning(string format, params object[] args)
        {
            FileLogger.WriteWarning(format, args);
        }

        /// <summary>
        /// Writes an exception + informational message to the custom service warning log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        public static void WriteWarning(Exception ex, string format, params object[] args)
        {
            FileLogger.WriteWarning(ex, format, args);
        }
    }
}