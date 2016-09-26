//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Tests.Common
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebSec.Common;
    using WebSec.Common.TestInfrastructure;

    /// <summary>
    /// (Unit Test Class) an object resolver tests.
    /// </summary>
    [TestClass]
    public class ObjectResolverTests
    {
        /// <summary>
        /// (Unit Test Method) tests register instance.
        /// </summary>
        [TestMethod]
        public void TestRegisterInstance()
        {
            var testClass = new TestClass("default");
            ObjectResolver.RegisterInstance(testClass);

            ObjectResolver.Resolve<TestClass>().Name.ShouldEqual("default");
        }

        /// <summary>
        /// (Unit Test Method) tests register multiple instances.
        /// </summary>
        [TestMethod]
        public void TestRegisterMultipleInstances()
        {
            var testClassInstance1 = new TestClass("instance1");
            var testClassInstance2 = new TestClass("instance2");
            ObjectResolver.RegisterInstance(testClassInstance1, testClassInstance1.Name);
            ObjectResolver.RegisterInstance(testClassInstance2, testClassInstance2.Name);

            var instance1 = ObjectResolver.Resolve<TestClass>("instance1");
            instance1.Name.ShouldEqual("instance1");
            instance1.ShouldEqual(testClassInstance1);

            var instance2 = ObjectResolver.Resolve<TestClass>("instance2");
            instance2.Name.ShouldEqual("instance2");
            instance2.ShouldEqual(testClassInstance2);
        }

        /// <summary>
        /// (Unit Test Class) a test class.
        /// </summary>
        private class TestClass
        {
            /// <summary>
            /// Initializes a new instance of the WebSec.Library.Tests.Common.ObjectResolverTests.TestClass
            /// class.
            /// </summary>
            /// <param name="name">
            /// The name.
            /// </param>
            internal TestClass(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            internal string Name { get; }
        }
    }
}