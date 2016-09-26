//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Library.Fiddler
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using global::Fiddler;

    /// <summary>
    /// Enables content injection via Fiddler.
    /// </summary>
    public static class FiddlerProxy
    {
        /// <summary>
        /// The instance headers.
        /// </summary>
        private static readonly ConcurrentDictionary<int, IList<KeyValuePair<string, string>>> InstanceHeaders =
            new ConcurrentDictionary<int, IList<KeyValuePair<string, string>>>();

        /// <summary>
        /// The header pairs.
        /// </summary>
        private static readonly IList<KeyValuePair<string, string>> HeaderPairs =
            new List<KeyValuePair<string, string>>();

        /// <summary>
        /// The content types that are subject to inspection when a request fails.
        /// </summary>
        private static readonly string[] ResponseContentTypeFilter =
        {
            "text/html",
            "text/html; charset=utf-8",
            "application/json",
            "application/json; charset=utf-8"
        };

        /// <summary>
        /// The has started.
        /// </summary>
        private static bool hasStarted;

        /// <summary>
        /// Initializes static members of the <see cref="FiddlerProxy"/> class.
        /// </summary>
        static FiddlerProxy()
        {
            ResponseSession = new ConcurrentDictionary<string, Session>();
        }

        /// <summary>
        /// Gets or sets the request processed.
        /// </summary>
        public static Action<string> RequestProcessed { get; set; }

        /// <summary>
        /// Gets the response session.
        /// </summary>
        public static ConcurrentDictionary<string, Session> ResponseSession { get; }

        /// <summary>
        /// Engages Fiddler with HTTP header injection.
        /// </summary>
        /// <param name="headers">
        /// The headers to inject, in key:value format.
        /// </param>
        /// <param name="port">
        /// The proxy port to use.
        /// </param>
        public static void Initialize(string[] headers, int port)
        {
            Cleanup(port);

            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                var tokens = header.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 2)
                {
                    HeaderPairs.Add(new KeyValuePair<string, string>(tokens[0], tokens[1]));
                }
            }

            FiddlerApplication.BeforeRequest += FiddlerApplicationOnBeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerApplicationOnBeforeResponse;

            FiddlerApplication.Startup(port, FiddlerCoreStartupFlags.Default);
            hasStarted = true;
        }

        /// <summary>
        /// Creates a per-process request header registration.
        /// </summary>
        /// <param name="processId">The process Id of the process that originated the request.</param>
        /// <param name="userAgent">The User-Agent string to apply to a request.</param>
        /// <param name="headers">A set of header key:values to apply to a request.</param>
        public static void RegisterInstanceHeaders(int processId, string userAgent, string headers)
        {
            InstanceHeaders[processId] = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(userAgent))
            {
                InstanceHeaders[processId].Add(new KeyValuePair<string, string>("User-Agent", userAgent));
            }

            if (!string.IsNullOrEmpty(headers))
            {
                foreach (var header in headers.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var tokens = header.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 2)
                    {
                        InstanceHeaders[processId].Add(new KeyValuePair<string, string>(tokens[0], tokens[1]));
                    }
                }
            }
        }

        /// <summary>
        /// Clears request/response headers.
        /// </summary>
        /// <param name="processId">
        /// The process Id as the key.
        /// </param>
        public static void ClearHeaders(int processId)
        {
            IList<KeyValuePair<string, string>> remove;
            InstanceHeaders.TryRemove(processId, out remove);

            ResponseSession.Clear();
        }

        /// <summary>
        /// Shuts down the fiddler proxy.
        /// </summary>
        /// <param name="port">
        /// The port.
        /// </param>
        internal static void Cleanup(int port)
        {
            FiddlerApplication.BeforeRequest -= FiddlerApplicationOnBeforeRequest;
            FiddlerApplication.BeforeResponse -= FiddlerApplicationOnBeforeResponse;

            HeaderPairs.Clear();

            if (FiddlerApplication.oProxy != null)
            {
                FiddlerApplication.oProxy.Detach();
            }

            // Fiddler Shutdown() will not tear down a fiddler proxy started by another process
            // The workaround is to startup a proxy (which will override the other instance)
            // and then shut it down.
            if (!hasStarted)
            {
                FiddlerApplication.Startup(port, FiddlerCoreStartupFlags.Default);
            }

            FiddlerApplication.Shutdown();

            hasStarted = false;
        }

        /// <summary>
        /// The fiddler application on before request.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        private static void FiddlerApplicationOnBeforeRequest(Session session)
        {
            // Autopilot.Logger.WriteDebug("Proxy Request PID:{1} {0}", oSession.host, oSession.LocalProcessID);
            if (session.oRequest.headers["Accept"] != null &&
                session.oRequest.headers["Accept"].ContainsAnyOi(ResponseContentTypeFilter))
            {
                session.bBufferResponse = true;
            }

            if (InstanceHeaders.ContainsKey(session.LocalProcessID))
            {
                foreach (var header in InstanceHeaders[session.LocalProcessID].ToList())
                {
                    if (session.bBufferResponse)
                    {
                        // This is a "page" class of request, apply all overrides.
                        session.oRequest[header.Key] = header.Value;
                    }
                    else if (!session.bBufferResponse &&
                             (header.Key.EqualsOi("User-Agent") || header.Key.EqualsOi("Accept-Language")))
                    {
                        // This is a secondary (e.g image or script) request, only override certain headers
                        session.oRequest[header.Key] = header.Value;
                    }
                }
            }

            foreach (var header in HeaderPairs)
            {
                session.oRequest[header.Key] = header.Value;
            }

            RequestProcessed?.Invoke(session.host);
        }

        /// <summary>
        /// The fiddler application on before response.
        /// </summary>
        /// <param name="session">
        /// The o session.
        /// </param>
        private static void FiddlerApplicationOnBeforeResponse(Session session)
        {
            var key = Constants.FiddlerResponseSessionKey.FormatIc(session.LocalProcessID, session.fullUrl);

            // add session to be consumed by caller
            ResponseSession[key] = session;
        }
    }
}