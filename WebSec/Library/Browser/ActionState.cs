//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;

    /// <summary>
    /// Represents the state of an action.
    /// </summary>
    public sealed class ActionState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionState"/> class.
        /// </summary>
        /// <param name="onAlert">The action to be invoked if an unexpected Alert dialog is encountered.</param>
        public ActionState(Action<string> onAlert)
        {
            this.OnAlertVisible = onAlert;
        }

        /// <summary>
        /// Gets a value indicating whether a fault has been encountered.
        /// </summary>
        public bool IsFaulted { get; private set; }

        /// <summary>
        /// Gets the action to be invoked if an unexpected Alert dialog is encountered.
        /// </summary>
        public Action<string> OnAlertVisible { get; private set; }

        /// <summary>
        /// Marks the state as Faulted after having thrown an exception. 
        /// </summary>
        public void SetIsFaulted()
        {
            this.IsFaulted = true;
        }
    }
}