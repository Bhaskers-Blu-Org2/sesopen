//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using HtmlAgilityPack;
    using Interfaces;
    using Logger;
    using OpenQA.Selenium;
    using Constants = Library.Constants;

    /// <inheritdoc />
    public sealed class Target : ITarget
    {
        /// <summary>
        /// The content.
        /// </summary>
        private string content;

        /// <summary>
        /// Prevents a default instance of the <see cref="Target"/> class from being created.
        /// Close creation access to Target by using the default constructor
        /// </summary>
        private Target()
        {
        }

        /// <inheritdoc />
        public IEnumerable<IWebElement> PostElements { get; set; }

        /// <inheritdoc />
        public Uri Uri { get; private set; }

        /// <inheritdoc />
        public string UrlBase { get; private set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, UrlParameter> Params { get; private set; }

        /// <inheritdoc />
        public string Content
        {
            get
            {
                return this.content;
            }

            set
            {
                this.content = value;
                try
                {
                    this.GetInputTextAreas();
                }
                catch
                {
                    Logger.WriteError("Non CLS exception catch (target:{0})", args: Uri.AbsoluteUri);
                }
            }
        }

        /// <summary>
        /// Factory function.
        /// </summary>
        /// <param name="targetSource"> source of the target</param>
        /// <returns>returns an ITarget representing the targetSource</returns>
        public static ITarget Create(string targetSource)
        {
            Uri uri = null;

            try
            {
                uri = new Uri(targetSource);
            }
            catch (UriFormatException ex)
            {
                Logger.WriteWarning(ex, targetSource);
            }
            catch (ArgumentNullException ex)
            {
                Logger.WriteWarning(ex, targetSource);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Logger.WriteWarning(ex, targetSource);
            }

            // Check if it's a valid URI.
            if (uri == null)
            {
                return null;
            }

            var target = new Target
            {
                Params = new Dictionary<string, UrlParameter>(),
                Uri = uri
            };

            var querypos = targetSource.IndexOf('?');
            if (querypos > 0)
            {
                // substract the URL
                target.UrlBase = target.Uri.OriginalString.Substring(0, querypos);

                // pull out the params
                var param = targetSource.Substring(querypos + 1)
                    .Split(new[] { '&', '#' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (
                    var r in
                        param.Select(s => s.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)).Where(
                            r => r.Length > 0 && !string.IsNullOrEmpty(r[0])))
                {
                    var v = string.Empty;
                    for (var i = 1; i < r.Length; i++)
                    {
                        v += r[i] + "=";
                    }

                    if (v.EndsWith("="))
                    {
                        v = v.Remove(v.Length - 1);
                    }

                    target.AddParameter(r[0], new UrlParameter { ParameterValue = v });
                }
            }

            return target;
        }

        /// <inheritdoc />
        public string GetParam(string testparam, string testval)
        {
            return this.GetParams(testparam, testval);
        }

        /// <inheritdoc />
        public string GetAllParams(string testval)
        {
            return this.GetParams(null, testval);
        }

        /// <summary>
        /// Reconstruct the target back
        /// </summary>
        /// <returns>A formated string</returns>
        public override string ToString()
        {
            var p = this.ConcatenateParams();
            return !string.IsNullOrEmpty(p) ? this.UrlBase + "?" + p : Uri.AbsoluteUri;
        }

        /// <summary>
        /// The get params.
        /// </summary>
        /// <param name="testparam">
        /// The test param.
        /// </param>
        /// <param name="testval">
        /// The test val.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetParams(string testparam = null, string testval = null)
        {
            var parameters = string.Empty;

            if (string.IsNullOrEmpty(testparam))
            {
                parameters = this.Params
                    .Aggregate(
                        string.Empty,
                        (current, param) =>
                            current + param.Key + "=" +
                            (string.IsNullOrEmpty(testval)
                                ? param.Value.ParameterValue.Replace("{{", string.Empty).Replace("}}", string.Empty)
                                : testval) + "&");
            }
            else
            {
                parameters = this.Params
                    .Aggregate(
                        parameters,
                        (current, param) =>
                            current +
                            (testparam == param.Key
                                ? param.Key + "=" + testval + "&"
                                : param.Key + "=" +
                                  param.Value.ParameterValue.Replace("{{", string.Empty).Replace("}}", string.Empty) +
                                  "&"));

                if (this.Params.ContainsKey(testparam) &&
                    this.Params[testparam].ParameterValue.Equals(Constants.PostParameterValue))
                {
                    parameters += testparam + "=" + testval;
                }
            }

            if (parameters.EndsWith("&"))
            {
                parameters = parameters.Substring(0, parameters.Length - 1);
            }

            return parameters;
        }

        /// <summary>
        /// Get Input Text Areas From The Content Page, only if html,
        /// to extend this for other types.
        /// </summary>
        private void GetInputTextAreas()
        {
            if (string.IsNullOrEmpty(this.content))
            {
                return;
            }

            // Experiment to track down Agility Pack crashes (BMx)
            if (!HtmlHelper.ContentIsHtml(this.content))
            {
                return;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(this.content);

            // get all unique links from the page
            var inputs = doc.DocumentNode?.SelectNodes(Constants.XPathInputTextType);
            if (inputs == null)
            {
                return;
            }

            foreach (
                var p in
                    inputs.Where(input => input.Attributes["name"] != null).Select(
                        input => input.Attributes["name"].Value).Where(p => !this.Params.ContainsKey(p)))
            {
                this.AddParameter(p, new UrlParameter { ParameterValue = Constants.PostParameterValue });
            }
        }

        /// <summary>
        /// Get a URL extensions just using the params that we shall fuzz
        /// </summary>
        /// <returns>a string with key=value for parameters marked to be tested</returns>
        private string ConcatenateParams()
        {
            var parameters = string.Empty;
            if (this.Params.Count == 0)
            {
                return string.Empty;
            }

            foreach (KeyValuePair<string, UrlParameter> param in this.Params)
            {
                if (string.IsNullOrEmpty(param.Value.ParameterValue))
                {
                    parameters += param.Key + "=&";
                }
                else
                {
                    parameters += param.Key + "=" + param.Value.ParameterValue + "&";
                }
            }

            // strip out the last  '&' if there is one
            if (parameters.EndsWith("&"))
            {
                parameters = parameters.Substring(0, parameters.Length - 1);
            }

            // return the parameters now we have substituted the test case(s)
            return parameters;
        }

        /// <summary>
        /// Adds a parameter to 'value'.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private void AddParameter(string key, UrlParameter value)
        {
            ((Dictionary<string, UrlParameter>)this.Params)[key] = value;
        }
    }
}