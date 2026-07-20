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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Conductor.Api;
using conductor_csharp.test.Helper;
using Xunit;

namespace conductor_csharp.test.ApiUnit
{
    /// <summary>
    /// Server-less unit tests for the secret value transport, exercising the change that stopped
    /// JSON-encoding secret values: PutSecret must send the raw string as-is (not quoted), and
    /// GetSecret must return the response body verbatim (no JSON deserialization).
    ///
    /// These run without a live Conductor server by faking the HTTP transport via
    /// <see cref="MockApiClient"/>, so they execute in the coverage-collecting unit test job
    /// (no Integration trait).
    /// </summary>
    [Trait("Category", "Unit")]
    public class SecretResourceApiUnitTest
    {
        [Fact]
        public void GetSecret_ReturnsResponseBodyVerbatim()
        {
            // A raw, unquoted secret value that is NOT valid JSON. If the value were routed through
            // JSON deserialization (the old Object behavior) this would throw; as a raw string it is
            // returned exactly as received.
            const string rawSecret = "p@ss w0rd-not-json";
            var (client, _) = MockApiClient.Build<SecretResourceApi>(HttpStatusCode.OK, rawSecret);

            var value = client.GetSecret("my-key");

            Assert.Equal(rawSecret, value);
        }

        [Fact]
        public async Task GetSecretAsync_ReturnsResponseBodyVerbatim()
        {
            const string rawSecret = "async-p@ss w0rd-not-json";
            var (client, _) = MockApiClient.Build<SecretResourceApi>(HttpStatusCode.OK, rawSecret);

            var value = await client.GetSecretAsync("my-key");

            Assert.Equal(rawSecret, value);
        }

        [Fact]
        public void GetSecret_RequestsSecretPath()
        {
            var (client, handler) = MockApiClient.Build<SecretResourceApi>(HttpStatusCode.OK, "value");

            client.GetSecret("my-key");

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/secrets/my-key", request.RequestUri.AbsolutePath);
        }

        [Fact]
        public void PutSecret_SendsRawStringBody_NotJsonEncoded()
        {
            const string rawSecret = "super-secret-value";
            var (client, handler) = MockApiClient.Build<SecretResourceApi>(HttpStatusCode.OK, "{}");

            client.PutSecret(rawSecret, "my-key");

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/api/secrets/my-key", request.RequestUri.AbsolutePath);
            // The value must reach the wire exactly as provided. JSON-encoding would wrap it in
            // quotes ("super-secret-value") and corrupt the stored secret.
            Assert.Equal(rawSecret, handler.LastRequestBody);
            Assert.DoesNotContain("\"", handler.LastRequestBody);
        }

        [Fact]
        public async Task PutSecretAsync_SendsRawStringBody_NotJsonEncoded()
        {
            const string rawSecret = "async-super-secret-value";
            var (client, handler) = MockApiClient.Build<SecretResourceApi>(HttpStatusCode.OK, "{}");

            await client.PutSecretAsync(rawSecret, "my-key");

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/api/secrets/my-key", request.RequestUri.AbsolutePath);
            Assert.Equal(rawSecret, handler.LastRequestBody);
            Assert.DoesNotContain("\"", handler.LastRequestBody);
        }
    }
}
