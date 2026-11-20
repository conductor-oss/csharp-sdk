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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Authentication;
using conductor_csharp.test.Helper;
using RestSharp;
using Xunit;

namespace Tests.Client
{
    /// <summary>
    /// Server-less tests for the refresh-and-retry on a 401 that rejects the client's token.
    /// </summary>
    [Trait("Category", "Unit")]
    public class TokenRefreshUnitTest
    {
        private const string InvalidToken = "{\"error\":\"INVALID_TOKEN\"}";

        // After a failover every in-flight request is rejected at once; together they must fetch one new token, not one each.
        [Fact]
        public async Task ConcurrentRejections_FetchOneNewToken()
        {
            const int n = 20;
            int issued = 0, rejected = 0;
            var failedOver = false;
            var allRejected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var api = ApiFor(async request =>
            {
                if (IsTokenRequest(request))
                {
                    // Hold the fetch open so the other requests' refreshes overlap it.
                    await Task.Delay(50);
                    return (HttpStatusCode.OK, $"{{\"token\":\"t{Interlocked.Increment(ref issued)}\"}}");
                }
                if (Volatile.Read(ref failedOver) && TokenOf(request) == "t1")
                {
                    if (Interlocked.Increment(ref rejected) == n)
                    {
                        allRejected.TrySetResult(true);
                    }
                    await Task.WhenAny(allRejected.Task, Task.Delay(TimeSpan.FromSeconds(5)));
                    return (HttpStatusCode.Unauthorized, InvalidToken);
                }
                return (HttpStatusCode.OK, "secret");
            });
            await api.GetSecretAsync("key");
            Volatile.Write(ref failedOver, true);

            var results = await Task.WhenAll(Enumerable.Range(0, n).Select(_ => api.GetSecretAsync("key")));

            Assert.All(results, value => Assert.Equal("secret", value));
            Assert.Equal(n, rejected);
            Assert.Equal(2, issued); // t1 before the failover, t2 shared by every retry
        }

        [Fact]
        public async Task ConcurrentFirstCalls_FetchOneToken()
        {
            const int n = 20;
            int issued = 0;
            var api = ApiFor(async request =>
            {
                if (IsTokenRequest(request))
                {
                    await Task.Delay(50);
                    return (HttpStatusCode.OK, $"{{\"token\":\"t{Interlocked.Increment(ref issued)}\"}}");
                }
                return (HttpStatusCode.OK, "secret");
            });

            // The token fetch is synchronous, so each caller blocks a thread; LongRunning keeps them off the thread pool.
            await Task.WhenAll(Enumerable.Range(0, n).Select(_ =>
                Task.Factory.StartNew(() => api.GetSecret("key"), TaskCreationOptions.LongRunning)));

            Assert.Equal(1, issued);
        }

        private static SecretResourceApi ApiFor(Func<HttpRequestMessage, Task<(HttpStatusCode Status, string Body)>> respond)
        {
            var configuration = new Configuration
            {
                BasePath = MockApiClient.BasePath,
                AuthenticationSettings = new OrkesAuthenticationSettings("key", "secret"),
            };
            configuration.ApiClient.RestClient = new RestClient(new RestClientOptions(MockApiClient.BasePath)
            {
                ConfigureMessageHandler = _ => new AsyncStubHandler(respond),
            });
            return configuration.GetClient<SecretResourceApi>();
        }

        private static bool IsTokenRequest(HttpRequestMessage request) => request.RequestUri.AbsolutePath.EndsWith("/token");

        private static string TokenOf(HttpRequestMessage request) =>
            request.Headers.TryGetValues("X-Authorization", out var values) ? values.First() : null;

        // StubHttpMessageHandler records requests in a List, which is not safe for concurrent requests.
        private sealed class AsyncStubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<(HttpStatusCode Status, string Body)>> _respond;

            public AsyncStubHandler(Func<HttpRequestMessage, Task<(HttpStatusCode Status, string Body)>> respond)
            {
                _respond = respond;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var (status, body) = await _respond(request);
                return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            }
        }
    }
}
