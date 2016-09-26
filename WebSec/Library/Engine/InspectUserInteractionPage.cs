//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Browser;
    using Interfaces;
    using OpenQA.Selenium;

    /// <summary>
    /// Inspect user interaction pages, try finding all user interaction controls.
    /// </summary>
    public sealed class InspectUserInteractionPage
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly IContext context;

        /// <summary>
        /// The input element.
        /// </summary>
        private readonly HashSet<IWebElement> inputElement;

        /// <summary>
        /// The xPath elements.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.XPath is a well known term," +
                            " stylecop confuses this with Hungarian notation.")]
        private readonly IEnumerable<string>
            xPathElements;

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectUserInteractionPage"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="xPathElements"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText",
            Justification = "Reviewed. Suppression is OK here."),
         SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation",
             Justification = "Reviewed. Suppression is OK here.")]
        public InspectUserInteractionPage(IContext context, IEnumerable<string> xPathElements)
        {
            this.context = context;
            this.xPathElements = xPathElements;
            this.inputElement = new HashSet<IWebElement>();
        }

        /// <summary>
        /// The inspect.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process inspect in this collection.
        /// </returns>
        public IEnumerable<IWebElement> Inspect(ITarget target, ref BrowserAbstract browser)
        {
            // clear the container in case we reuse the object
            this.inputElement.Clear();

            if (browser == null)
            {
                BrowserAbstract closureBrowser = this.context.AcquireBrowser(BrowserType.Chrome);
                browser = closureBrowser;

                if (browser == null)
                {
                    Logger.Logger.WriteWarning("Not able to acquire a browser");
                    return this.inputElement;
                }
            }

            browser.NavigateTo(target.Uri.OriginalString);

            // waiting for page to load
            browser.WaitForPageLoad(Common.Constants.BrowserWaitForPageLoadInMilliseconds);

            // dismiss any alert and register the text message, catch the vulnerability in the test plugin
            string alertText;
            browser.DismissedIfAlertDisplayed(out alertText);

            foreach (var pathElement in this.xPathElements)
            {
                IEnumerable<IWebElement> elements = browser.FindWebElements(By.XPath(pathElement));

                foreach (IWebElement webElement in elements)
                {
                    this.inputElement.Add(webElement);
                }
            }

            return this.inputElement;
        }
    }
}