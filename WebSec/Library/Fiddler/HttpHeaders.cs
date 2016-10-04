//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// The http headers.
    /// </summary>
    [Serializable]
    public class HttpHeaders : HashSet<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaders"/> class.
        /// </summary>
        public HttpHeaders()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaders"/> class with a given key/value pair.
        /// </summary>
        /// <param name="key">The key to add to the set.</param>
        /// <param name="value">The value to add tot he set.</param>
        public HttpHeaders(string key, string value) : this()
        {
            this.Add(new KeyValuePair<string, string>(key, value));
        }

        /// <inheritdoc/>
        protected HttpHeaders(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Adds the specified element to the set.
        /// </summary>
        /// <param name="key">The key to add to the set.</param>
        /// <param name="value">The value to add tot he set.</param>
        public void Add(string key, string value)
        {
            this.Add(new KeyValuePair<string, string>(key, value));
        }

        /// <summary>
        /// Queries a set of KeyValuePairs for the existence of a particular key and value combination.
        /// </summary>
        /// <param name="key">A string representing the expected key.</param>
        /// <param name="value">A string representing the expected value.</param>
        /// <returns>True if the key and value exist, otherwise false.</returns>
        public bool ExistsAndEquals(string key, string value)
        {
            return this.IsNotNull() && this.Any(t => t.Key.EqualsOi(key) && t.Value.EqualsOi(value));
        }

        /// <summary>
        /// Returns the set of KeyValuePair items formatted as an HTTP Header.
        /// </summary>
        /// <returns>A string formatted according to the HTTP Header specification.</returns>
        public string ToHttpHeaderString()
        {
            var builder = new StringBuilder();

            foreach (var pair in this)
            {
                builder.AppendFormat("{0}: {1}", pair.Key, pair.Value);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}