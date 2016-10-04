//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Plugins
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using HtmlAgilityPack;
    using Library.Engine;
    using Library.Engine.Interfaces;
    using Library.PluginBase;

    /// <summary>
    /// A view state detector.
    /// </summary>
    /// <seealso cref="T:BackScatterScannerLib.Engine.TestBase"/>
    [TestBase(TestBaseType = TestBaseType.Detector,
        Name = "ViewState MAC Detector",
        Description = "Verifies that a page's ViewState is tamper resistant.")]
    public class ViewStateDetector : PluginBaseAbstract
    {
        /// <summary>
        /// Inspects the page response content.
        /// </summary>
        /// <param name="requestTarget">
        /// The request Target.
        /// </param>
        /// <param name="responseTarget">
        /// The response Target.
        /// </param>
        /// <param name="currentResponse">
        /// The current Response.
        /// </param>
        /// <param name="plugIn">
        /// The plug In.
        /// </param>
        /// <param name="testCase">
        /// The test Case.
        /// </param>
        /// <param name="testedParameter">
        /// The tested Parameter.
        /// </param>
        /// <param name="testedVal">
        /// The tested Val.
        /// </param>
        /// <seealso cref="M:BackScatterScannerLib.Engine.TestBase.InspectResponse(ITarget,ITarget,HttpWebResponseHolder,string,TestCase,string,string)"/>
        public override void InspectResponse(
            ITarget requestTarget,
            ITarget responseTarget,
            HttpWebResponseHolder currentResponse,
            string plugIn,
            TestCase testCase,
            string testedParameter,
            string testedVal)
        {
            var dom = new HtmlDocument();

            try
            {
                dom.LoadHtml(currentResponse.ResponseString);
            }
            catch (Exception)
            {
                // ignore parsing errors - we don't know which errors are possible here.
                return;
            }

            HtmlNodeCollection inputElements = dom.DocumentNode.SelectNodes("//input");

            if (inputElements != null)
            {
                foreach (HtmlNode inputElement in inputElements)
                {
                    string inputName = inputElement.GetAttributeValue("name", string.Empty);

                    if (inputName == "__VIEWSTATE")
                    {
                        string inputValue = inputElement.GetAttributeValue("value", string.Empty);

                        if (!string.IsNullOrWhiteSpace(inputValue))
                        {
                            var formatter = new LosFormatter();
                            var stringBuilder = new StringBuilder();
                            var stringWriter = new StringWriter(stringBuilder);

                            try
                            {
                                formatter.Serialize(stringWriter, formatter.Deserialize(inputValue));

                                if (inputValue.Length == stringBuilder.ToString().Length)
                                {
                                    AddVulnerability(
                                        "ViewState does not have MAC enabled.",
                                        testedParameter,
                                        testedVal,
                                        inputElement.OuterHtml,
                                        currentResponse,
                                        testCase,
                                        VulnerabilityLevelEnum.Info);
                                }
                            }
                            catch (ArgumentException)
                            {
                                // Caused by trying to serialize an encrypted ViewState.
                            }
                        }
                    }
                }
            }
        }
    }
}