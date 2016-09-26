//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    using System;

    /// <summary>
    ///    Catches exception thrown by an action.
    /// </summary>
    /// <remarks>
    ///    Originally borrowed from Machine.Specifications, licensed under MS-PL.
    /// </remarks>
    internal static class Catch
    {
        /// <summary>
        ///    Executes and action, capturing the thrown exception if any.
        /// </summary>
        /// <param name = "throwingAction">
        ///    The action to execute.
        /// </param>
        /// <returns>
        ///    The thrown exception; otherwise null.
        /// </returns>
        public static Exception Exception(Action throwingAction)
        {
            try
            {
                throwingAction();
            }
            catch (Exception exception)
            {
                return exception;
            }

            return null;
        }
    }
}