//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System.Linq;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// The integer overflow tester. Tries to generate server side crashes.
    /// </summary>
    [TestBase(TestBaseType = TestBaseType.Test, Name = "Integer overflow",
        Description = "Test for side-effects of integer overflow in values")]
    public sealed class IntOverflowTester : PluginBaseAbstract
    {
        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="currentcontext">
        /// The current context.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        public override void Init(IContext currentcontext, ITarget target)
        {
            base.Init(currentcontext, target);

            // the vulns will be catch by the error detector, so no action for check vulns in this test
            TestCases.Add(new TestCase
            {
                TestName = "Integer overflow - MaxInt+1/MaxLong+1",
                InjectionString = TestBaseHelper.LoadTestCase("IntOverflowTester", Context).ToArray(),
                FuzzOnlyNumericParam = true
            });
        }
    }
}