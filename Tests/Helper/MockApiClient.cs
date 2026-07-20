/*
 * Copyright 2026 Conductor Authors.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Client;
using RestSharp;

namespace conductor_csharp.test.Helper
{
    /// <summary>
    /// A stub <see cref="HttpMessageHandler"/> for driving Conductor API clients through a fake
    /// transport in unit tests. Records every outgoing request (and its body) and returns a canned
    /// response supplied by the caller, so no live server is required.
    /// </summary>
    public sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> _respond;

        /// <summary>Every request the client sent, in order.</summary>
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        /// <summary>The string body of each request (null when there was no body), aligned with <see cref="Requests"/>.</summary>
        public List<string> RequestBodies { get; } = new List<string>();

        /// <summary>Responds to every request with the same fixed status and body.</summary>
        public StubHttpMessageHandler(HttpStatusCode status, string body)
            : this(_ => (status, body))
        {
        }

        /// <summary>Responds to each request using the supplied delegate, which may inspect the request.</summary>
        public StubHttpMessageHandler(Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> respond)
        {
            _respond = respond;
        }

        /// <summary>The most recently received request.</summary>
        public HttpRequestMessage LastRequest => Requests[Requests.Count - 1];

        /// <summary>The body of the most recently received request (null when there was no body).</summary>
        public string LastRequestBody => RequestBodies[RequestBodies.Count - 1];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            RequestBodies.Add(request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync());

            var (status, body) = _respond(request);
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(body ?? string.Empty, Encoding.UTF8, "application/json"),
            };
        }
    }

    /// <summary>
    /// Builds Conductor API clients wired to a stubbed HTTP transport, so unit tests can exercise
    /// request/response handling without a live server. HTTP is faked at
    /// <see cref="ApiClient.RestClient"/> via RestSharp's <c>ConfigureMessageHandler</c> seam.
    /// </summary>
    public static class MockApiClient
    {
        /// <summary>Base path used for the fake server. Only the path matters for assertions.</summary>
        public const string BasePath = "http://server/api";

        /// <summary>Creates a <see cref="Configuration"/> whose transport is backed by <paramref name="handler"/>.</summary>
        public static Configuration MockedConfiguration(StubHttpMessageHandler handler)
        {
            var configuration = new Configuration { BasePath = BasePath };
            configuration.ApiClient.RestClient = new RestClient(new RestClientOptions(BasePath)
            {
                ConfigureMessageHandler = _ => handler,
            });
            return configuration;
        }

        /// <summary>
        /// Builds an API client of type <typeparamref name="T"/> whose HTTP calls are answered by
        /// <paramref name="respond"/>. Returns the client together with the handler for assertions.
        /// </summary>
        public static (T Client, StubHttpMessageHandler Handler) Build<T>(
            Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> respond)
            where T : IApiAccessor, new()
        {
            var handler = new StubHttpMessageHandler(respond);
            var client = MockedConfiguration(handler).GetClient<T>();
            return (client, handler);
        }

        /// <summary>
        /// Builds an API client of type <typeparamref name="T"/> that receives the same fixed
        /// <paramref name="status"/> and <paramref name="body"/> for every request.
        /// </summary>
        public static (T Client, StubHttpMessageHandler Handler) Build<T>(HttpStatusCode status, string body)
            where T : IApiAccessor, new()
            => Build<T>(_ => (status, body));
    }
}
