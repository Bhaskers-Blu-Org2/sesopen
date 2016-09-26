//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common.TestInfrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///    Factory class for instantiating assertion failure exceptions based on test framework.
    /// </summary>
    public static class AssertFailureExceptionFactory
    {
        /// <summary>
        ///    Gets or sets the framework exception constructor.
        /// </summary>
        /// <value>
        ///    The framework exception constructor.
        /// </value>
        public static Func<string, Exception, Exception> FrameworkExceptionConstructor { get; set; }

        /// <summary>
        ///    Creates default assertion failure instance for the current test framework.
        /// </summary>
        /// <param name = "message">
        ///    The exception message.
        /// </param>
        /// <returns>
        ///    The created instance.
        /// </returns>
        public static Exception CreateException(string message)
        {
            Exception rv = CreateException(message, null);
            return rv;
        }

        /// <summary>
        ///    Creates default assertion failure instance for the current test framework.
        /// </summary>
        /// <param name = "message">
        ///    The exception message.
        /// </param>
        /// <param name = "innerException">
        ///    The inner exception.
        /// </param>
        /// <returns>
        ///    The created instance.
        /// </returns>
        public static Exception CreateException(string message, Exception innerException)
        {
            if (ReferenceEquals(FrameworkExceptionConstructor, null))
            {
                throw new InvalidOperationException("The framework exception constructor is null.");
            }

            Exception rv = FrameworkExceptionConstructor.Invoke(message, innerException);
            return rv;
        }

        /// <summary>
        /// Configures the system to use the msTest Reference.
        /// </summary>
        public static void ConfigureForMicrosoftTest()
        {
            Assembly microsoftTest = Assembly.Load("Microsoft.VisualStudio.QualityTools.UnitTestFramework");
            Type[] types = microsoftTest.GetTypes();
            Type exceptionType =
                types
                    .First(t => t.FullName == "Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException");

            Func<string, Exception, Exception> fec =
                (message, innerException) => exceptionType.Create(message, innerException).CastTo<Exception>();
            FrameworkExceptionConstructor = fec;
        }
    }
}