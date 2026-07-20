/*
 * Copyright 2024 Conductor Authors.
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Api;
using Conductor.Client;
using RestSharp;
using Xunit;

namespace conductor_csharp.test.Api
{
    /// <summary>
    /// Server-less unit tests for the secret value transport, exercising the change that stopped
    /// JSON-encoding secret values: PutSecret must send the raw string as-is (not quoted), and
    /// GetSecret must return the response body verbatim (no JSON deserialization).
    ///
    /// These run without a live Conductor server by swapping the RestClient's underlying HTTP
    /// handler, so they execute in the coverage-collecting unit test job (no Integration trait).
    /// </summary>
    [Trait("Category", "Unit")]
    public class SecretResourceApiUnitTest
    {
        private const string FakeBasePath = "http://localhost:8080/api";

        private static SecretResourceApi BuildClient(RecordingHttpMessageHandler handler)
        {
            var configuration = new Configuration { BasePath = FakeBasePath };
            var options = new RestClientOptions(FakeBasePath)
            {
                ConfigureMessageHandler = _ => handler
            };
            configuration.ApiClient.RestClient = new RestClient(options);
            return new SecretResourceApi(configuration);
        }

        [Fact]
        public void GetSecret_ReturnsResponseBodyVerbatim()
        {
            // A raw, unquoted secret value that is NOT valid JSON. If the value were routed through
            // JSON deserialization (the old Object behavior) this would throw; as a raw string it is
            // returned exactly as received.
            const string rawSecret = "p@ss w0rd-not-json";
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, rawSecret);
            var client = BuildClient(handler);

            var value = client.GetSecret("my-key");

            Assert.Equal(rawSecret, value);
        }

        [Fact]
        public async Task GetSecretAsync_ReturnsResponseBodyVerbatim()
        {
            const string rawSecret = "async-p@ss w0rd-not-json";
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, rawSecret);
            var client = BuildClient(handler);

            var value = await client.GetSecretAsync("my-key");

            Assert.Equal(rawSecret, value);
        }

        [Fact]
        public void GetSecret_RequestsSecretPath()
        {
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, "value");
            var client = BuildClient(handler);

            client.GetSecret("my-key");

            Assert.Equal(HttpMethod.Get, handler.LastMethod);
            Assert.Contains("/secrets/my-key", handler.LastRequestUri);
        }

        [Fact]
        public void PutSecret_SendsRawStringBody_NotJsonEncoded()
        {
            const string rawSecret = "super-secret-value";
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, string.Empty);
            var client = BuildClient(handler);

            client.PutSecret(rawSecret, "my-key");

            Assert.Equal(HttpMethod.Put, handler.LastMethod);
            Assert.Contains("/secrets/my-key", handler.LastRequestUri);
            // The value must reach the wire exactly as provided. JSON-encoding would wrap it in
            // quotes ("super-secret-value") and corrupt the stored secret.
            Assert.Equal(rawSecret, handler.LastRequestBody);
            Assert.DoesNotContain("\"", handler.LastRequestBody);
        }

        [Fact]
        public async Task PutSecretAsync_SendsRawStringBody_NotJsonEncoded()
        {
            const string rawSecret = "async-super-secret-value";
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, string.Empty);
            var client = BuildClient(handler);

            await client.PutSecretAsync(rawSecret, "my-key");

            Assert.Equal(HttpMethod.Put, handler.LastMethod);
            Assert.Contains("/secrets/my-key", handler.LastRequestUri);
            Assert.Equal(rawSecret, handler.LastRequestBody);
            Assert.DoesNotContain("\"", handler.LastRequestBody);
        }

        /// <summary>
        /// Intercepts the outgoing HTTP request so tests can assert on what was sent and return a
        /// canned response, without any network access.
        /// </summary>
        private sealed class RecordingHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _responseBody;

            public HttpMethod LastMethod { get; private set; }
            public string LastRequestUri { get; private set; }
            public string LastRequestBody { get; private set; }

            public RecordingHttpMessageHandler(HttpStatusCode status, string responseBody)
            {
                _status = status;
                _responseBody = responseBody;
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastMethod = request.Method;
                LastRequestUri = request.RequestUri?.ToString();
                if (request.Content != null)
                {
                    LastRequestBody = await request.Content.ReadAsStringAsync();
                }

                return new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_responseBody ?? string.Empty)
                };
            }
        }
    }
}
