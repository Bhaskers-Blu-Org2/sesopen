//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Browser
{
    using System;
    using System.Threading;
    using Logger;
    using OpenQA.Selenium;

    /// <summary>
    /// Retry mechanism for <see cref="WebDriverException" /> failures.
    /// </summary>
    public sealed class WebDriverExceptionRetry
    {
        /// <summary>
        /// The wait time in milliseconds.
        /// </summary>
        private const int WaitTimeInMilliseconds = 2000;

        /// <summary>
        /// The maximum attempts.
        /// </summary>
        private const int MaxAttempts = 2;

        /// <summary>
        /// The state.
        /// </summary>
        private readonly ActionState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverExceptionRetry" /> class.
        /// </summary>
        /// <param name="state">
        ///     The state.
        /// </param>
        public WebDriverExceptionRetry(ActionState state)
        {
            this.state = state;
        }

        /// <summary>
        /// Attempts a given action no more than the expected number of times.
        /// </summary>
        /// <param name="caller">
        ///     The name of the calling method.
        /// </param>
        /// <param name="context">
        ///     A description of the context within which the action invoked.
        /// </param>
        /// <param name="action">
        ///     The action to attempt.
        /// </param>
        public void Execute(string caller, string context, Action action)
        {
            int attempt = 1;
            while (true)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch (Exception exception)
                {
                    // announce that we saw an alert, let the caller decide if we should dismiss
                    if (exception is UnhandledAlertException && this.state.OnAlertVisible != null)
                    {
                        this.state.OnAlertVisible.Invoke((exception as UnhandledAlertException).AlertText);
                    }

                    if (attempt > MaxAttempts)
                    {
                        this.state.SetIsFaulted();

                        // let the caller decide how to handle this.
                        throw;
                    }

                    Logger.WriteWarning(
                    exception,
                    "{0}: Attempt {1} failed, retrying in {2} seconds ({3})",
                    caller,
                    attempt,
                    WaitTimeInMilliseconds / 1000,
                    context);

                    attempt++;
                }

                Thread.Sleep(WaitTimeInMilliseconds);
            }
        }

        /// <summary>
        /// Attempts a given function no more than the expected number of times.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The type of the return value.
        /// </typeparam>
        /// <param name="caller">
        ///     The name of the calling method.
        /// </param>
        /// <param name="context">
        ///     A description of the context within which the function was called.
        /// </param>
        /// <param name="func">
        ///     The function to attempt.
        /// </param>
        /// <returns>
        /// The response.
        /// </returns>
        public TResult Execute<TResult>(string caller, string context, Func<TResult> func)
        {
            int attempt = 1;
            while (true)
            {
                try
                {
                    TResult result = func.Invoke();
                    return result;
                }
                catch (Exception exception)
                {
                    // announce that we saw an alert, let the caller decide if we should dismiss
                    if (exception is UnhandledAlertException && this.state.OnAlertVisible != null)
                    {
                        this.state.OnAlertVisible.Invoke((exception as UnhandledAlertException).AlertText);
                    }

                    if (attempt > MaxAttempts)
                    {
                        this.state.SetIsFaulted();

                        // let the caller decide how to handle this.
                        throw;
                    }

                    Logger.WriteWarning(
                    exception,
                    "{0}: Attempt {1} failed, retrying in {2} seconds ({3})",
                    caller,
                    attempt,
                    WaitTimeInMilliseconds / 1000,
                    context);

                    attempt++;
                }

                Thread.Sleep(WaitTimeInMilliseconds);
            }
        }
    }
}