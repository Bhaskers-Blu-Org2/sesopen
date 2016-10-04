//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Logger
{
    /// <summary>
    /// Null logger implementation ignores all requests.
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <inheritdoc/> 
        public void WriteInfo(System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteInfo(string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteInfo(System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteError(System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteError(string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteError(System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteDebug(System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteDebug(string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteDebug(System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteWarning(System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteWarning(string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteWarning(System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public bool IsEnabled()
        {
            return false;
        }

        /// <inheritdoc/> 
        public void WriteInfoToCustomLog(string customLogId, System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteInfoToCustomLog(string customLogId, System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteInfoToCustomLog(string customLogId, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteErrorToCustomLog(string customLogId, System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteErrorToCustomLog(string customLogId, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteErrorToCustomLog(string customLogId, System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteDebugToCustomLog(string customLogId, System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteDebugToCustomLog(string customLogId, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteDebugToCustomLog(string customLogId, System.Exception ex, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteWarningToCustomLog(string customLogId, System.Exception ex)
        {
        }

        /// <inheritdoc/> 
        public void WriteWarningToCustomLog(string customLogId, string format, params object[] args)
        {
        }

        /// <inheritdoc/> 
        public void WriteWarningToCustomLog(string customLogId, System.Exception ex, string format, params object[] args)
        {
        }
    }
}