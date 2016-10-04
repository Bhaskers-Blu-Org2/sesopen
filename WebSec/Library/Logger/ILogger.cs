//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Logger
{
    using System;

    /// <summary>
    /// Interface for logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets or sets a value indicating whether the verbose.
        /// </summary>
        /// <value>
        /// true if verbose, false if not.
        /// </value>
        bool Verbose { get; set; }

        /// <summary>
        /// Writes an exception to the custom service info log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void WriteInfo(Exception ex);

        /// <summary>
        /// Writes an informational message to the custom service info log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteInfo(string format, params object[] args);

        /// <summary>
        /// Writes an exception + informational message to the custom service info log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteInfo(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes an exception to the custom service error log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void WriteError(Exception ex);

        /// <summary>
        /// Writes an informational message to the custom service error log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteError(string format, params object[] args);

        /// <summary>
        /// Writes an exception + informational message to the custom service error log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteError(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes an exception to the custom service debug log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void WriteDebug(Exception ex);

        /// <summary>
        /// Writes an informational message to the custom service debug log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteDebug(string format, params object[] args);

        /// <summary>
        /// Writes an exception + informational message to the custom service debug log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteDebug(Exception ex, string format, params object[] args);

        /// <summary>
        /// Writes an exception to the custom service warning log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void WriteWarning(Exception ex);

        /// <summary>
        /// Writes an informational message to the custom service warning log.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteWarning(string format, params object[] args);

        /// <summary>
        /// Writes an exception + informational message to the custom service warning log.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional parameters applied to the message format.</param>
        void WriteWarning(Exception ex, string format, params object[] args);
    }
}