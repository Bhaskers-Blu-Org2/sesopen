//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Common.Validation;
    using global::Fiddler;

    /// <summary>
    /// Default caching implementation.
    /// </summary>
    internal class DefaultProxyCache : FiddlerProxyCache
    {
        /// <summary>
        /// The clear lock.
        /// </summary>
        private static readonly SemaphoreSlim ClearLock = new SemaphoreSlim(1);

        /// <summary>
        /// The request content type filter.
        /// </summary>
        private static readonly string[] RequestContentTypeFilter =
        {
            "image/png",
            "image/jpeg",
            "image/gif",
            "image/x-icon",
            "application/x-javascript",
            "text/css"
        };

        /// <summary>
        /// The cache check interval.
        /// </summary>
        private readonly Stopwatch cacheCheckInterval;

        /// <summary>
        /// The interval in seconds.
        /// </summary>
        private readonly int intervalInSeconds;

        /// <summary>
        /// The responses.
        /// </summary>
        private readonly ConcurrentDictionary<string, CachedResponse> responses;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProxyCache"/> class.
        /// </summary>
        /// <param name="intervalInSeconds">The life time in seconds that an item should stay in the cache.</param>
        public DefaultProxyCache(int intervalInSeconds)
        {
            this.intervalInSeconds = intervalInSeconds;
            this.responses = new ConcurrentDictionary<string, CachedResponse>();
            this.cacheCheckInterval = new Stopwatch();
            this.cacheCheckInterval.Start();
        }

        /// <inheritdoc/>
        public override void ProcessRequest(Session session)
        {
            Require.NotNull(() => session);

            // how long since we validated the cache contents?
            if (this.cacheCheckInterval.Elapsed.TotalSeconds >= 60)
            {
                // get the lock immediately or move on.
                if (ClearLock.Wait(0))
                {
                    // check->lock->check: now we have the lock do we still need to perform the action?
                    if (this.cacheCheckInterval.Elapsed.TotalSeconds > this.intervalInSeconds)
                    {
                        foreach (var response in this.responses
                            .Where(r => r.Value.Age.TotalSeconds > this.intervalInSeconds)
                            .ToArray())
                        {
                            CachedResponse temp;
                            this.responses.TryRemove(response.Key, out temp);
                        }

                        this.cacheCheckInterval.Restart();
                    }
                }
            }

            CachedResponse cached;

            if (this.responses.TryGetValue(session.url, out cached))
            {
                // cache hit, tell fiddler to use the cache, other continue as normal
                session.utilCreateResponseAndBypassServer();
                session.oResponse.headers = cached.Headers;
                session.responseBodyBytes = cached.Body;
            }
        }

        /// <inheritdoc/>
        public override void ProcessResponse(Session session)
        {
            Require.NotNull(() => session);

            if (session.oResponse.headers["Content-Type"] != null &&
                session.oResponse.headers["Content-Type"].ContainsAnyOi(RequestContentTypeFilter) &&
                !this.responses.ContainsKey(session.url))
            {
                this.responses.TryAdd(session.url, new CachedResponse(session));
            }
        }

        /// <summary>
        /// A cached item.
        /// </summary>
        private class CachedResponse
        {
            /// <summary>
            /// The cookies header.
            /// </summary>
            private const string CookiesHeader = "Set-Cookie";

            /// <summary>
            /// The created.
            /// </summary>
            private readonly DateTime created;

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedResponse"/> class.
            /// </summary>
            /// <param name="session">
            /// The session.
            /// </param>
            public CachedResponse(Session session)
            {
                this.created = DateTime.Now;

                this.Headers = (HTTPResponseHeaders) session.oResponse.headers.Clone();

                // Don't cache cookies, they *might* pollute a persisted user id.              
                this.Headers.Remove(CookiesHeader);

                this.Body = session.responseBodyBytes ?? new byte[0];
            }

            /// <summary>
            /// Gets the headers.
            /// </summary>
            public HTTPResponseHeaders Headers { get; }

            /// <summary>
            /// Gets the body.
            /// </summary>
            public byte[] Body { get; }

            /// <summary>
            /// Gets the age.
            /// </summary>
            public TimeSpan Age
            {
                get { return DateTime.Now.Subtract(this.created); }
            }
        }
    }
}