//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Logger;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) for logger to file implementation.
    /// </summary>
    [TestClass]
    public class LoggerTests
    {
        /// <summary>
        /// Class cleanup.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (
                var file in
                    Directory.EnumerateFiles(Environment.CurrentDirectory, "*.log", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests write information.
        /// </summary>
        [TestMethod]
        public void TestWriteInfo()
        {
            Logger.WriteInfo("test message");

            Directory.EnumerateFiles(Environment.CurrentDirectory, "*.Info.log").Count().ShouldEqual(1);
        }

        /// <summary>
        /// (Unit Test Method) tests write debug.
        /// </summary>
        [TestMethod]
        public void TestWriteDebug()
        {
            Logger.WriteDebug("test message");

            Directory.EnumerateFiles(Environment.CurrentDirectory, "*.Debug.log").Count().ShouldEqual(1);
        }

        /// <summary>
        /// (Unit Test Method) tests write warning.
        /// </summary>
        [TestMethod]
        public void TestWriteWarning()
        {
            Logger.WriteWarning("test message");

            Directory.EnumerateFiles(Environment.CurrentDirectory, "*.Warning.log").Count().ShouldEqual(1);
        }

        /// <summary>
        /// (Unit Test Method) tests write error.
        /// </summary>
        [TestMethod]
        public void TestWriteError()
        {
            Logger.WriteError("test message");

            Directory.EnumerateFiles(Environment.CurrentDirectory, "*.Error.log").Count().ShouldEqual(1);
        }
    }
}