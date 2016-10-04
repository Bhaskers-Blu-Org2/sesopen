//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using Common.TestInfrastructure;
    using Library.Browser;
    using Library.Browser.Interfaces;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.Fiddler;
    using Library.Payloads;
    using Library.PluginBase;

    /// <summary>
    /// A plugin test base.
    /// </summary>
    /// <typeparam name="T">
    /// Generic type parameter.
    /// </typeparam>
    public class PluginTestBase<T> where T : class, new()
    {
        /// <summary>
        /// true if this object has been initialized.
        /// </summary>
        private static bool hasBeenInitialized;

        /// <summary>
        /// Initializes a new instance of the PluginTestBase class.
        /// </summary>
        public PluginTestBase()
        {
            FiddlerProxy.Initialize(new string[0], Constants.FiddlerPort);
            if (!hasBeenInitialized)
            {
                Initialize();
                hasBeenInitialized = true;
            }

            this.Context = new Context();
        }

        /// <summary>
        /// Gets or sets the response holder.
        /// </summary>
        protected HttpWebResponseHolder ResponseHolder { get; set; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        protected PluginBaseAbstract Instance { get; private set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        protected IContext Context { get; }

        /// <summary>
        /// Gets or sets a Action that will be invoked just prior to calling the TestBase.InspectResponse method.
        /// </summary>
        protected Action DetectorPreInspectionAction { get; set; }

        /// <summary>
        /// Verifies a vulnerability object has the basic plumbing correct.
        /// </summary>
        /// <param name="vuln">
        /// The vuln.
        /// </param>
        /// <param name="instance">
        /// The instance.
        /// </param>
        /// <param name="responseHolder">
        /// The response holder.
        /// </param>
        public static void AssertBasicVulnProperties(
            Vulnerability vuln, 
            PluginBaseAbstract instance,
            HttpWebResponseHolder responseHolder)
        {
            vuln.TestPlugin.ShouldEqual(instance.GetType().Name);
            vuln.HttpResponse.ShouldEqual(responseHolder);
            vuln.TestedParam.ShouldEqual("testedParam");
            vuln.TestedVal.ShouldEqual("testedValue");
        }

        /// <summary>
        /// Creates an instance of T and executes a request.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="target">
        /// Target for the.
        /// </param>
        /// <returns>
        /// A list of.
        /// </returns>
        public ReadOnlyCollection<Vulnerability> ExecutePluginDetectorRequest(ITarget target)
        {
            this.Instance = new T() as PluginBaseAbstract;
            if (this.Instance == null)
            {
                throw new InvalidOperationException("T must derive from TestBase");
            }

            // Execute
            this.Instance.Init(this.Context, target);
            var webRequestContext =
                this.Context.SendRequest(new ContextSendRequestParameter { Url = target.Uri.ToString() });

            this.ResponseHolder = webRequestContext.ResponseHolder;

            this.DetectorPreInspectionAction?.Invoke();

            this.Instance.InspectResponse(
                target, 
                target, 
                this.ResponseHolder, 
                "parentPlugin", 
                null, 
                "testedParam",
                "testedValue");

            foreach (var vulnDescription in this.Instance.Vulnerabilities)
            {
                AssertBasicVulnProperties(vulnDescription, this.Instance, this.ResponseHolder);
            }

            // We are done with the browser, give it back.
            BrowserHelper.ReleaseBrowser(webRequestContext);

            return new ReadOnlyCollection<Vulnerability>(this.Instance.Vulnerabilities.ToList());
        }

        /// <summary>
        /// Creates an instance of T and executes a request.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="target">
        /// Target for the.
        /// </param>
        /// <returns>
        /// A list of.
        /// </returns>
        public IReadOnlyCollection<Vulnerability> ExecutePluginTestRequest(ITarget target)
        {
            this.Instance = new T() as PluginBaseAbstract;
            if (this.Instance == null)
            {
                throw new InvalidOperationException("T must derive from TestBase");
            }

            // Execute
            this.Instance.Init(this.Context, target);
            this.Instance.DoTests();

            return new ReadOnlyCollection<Vulnerability>(this.Instance.Vulnerabilities.ToList());
        }

        /// <summary>
        /// Initializes this object.
        /// </summary>
        private static void Initialize()
        {
            ObjectResolver.RegisterInstance<IPayloads>(new Payloads(Library.Constants.PayloadsQuickDataFolder));
            ObjectResolver.RegisterType<IProcessManager, ProcessManager>();
            var browserInstances = new Dictionary<BrowserType, int> { { BrowserType.Chrome, 1 } };
            ObjectResolver.RegisterInstance<IBrowserManager>(new BrowserManager(browserInstances));
        }
    }
}