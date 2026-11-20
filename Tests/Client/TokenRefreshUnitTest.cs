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
using System.Collections.Concurrent;
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

        [Fact]
        public void PersistentRejection_RetriesWithEveryRefreshedToken()
        {
            var (api, sentTokens, issued) = AlwaysRejectingApi();

            var e = Assert.Throws<ApiException>(() => api.GetSecret("key"));

            AssertEveryRefreshWasUsed(e, sentTokens, issued());
        }

        [Fact]
        public async Task PersistentRejection_RetriesWithEveryRefreshedToken_Async()
        {
            var (api, sentTokens, issued) = AlwaysRejectingApi();

            var e = await Assert.ThrowsAsync<ApiException>(() => api.GetSecretAsync("key"));

            AssertEveryRefreshWasUsed(e, sentTokens, issued());
        }

        // A proxy or load balancer can answer 401 with an HTML page; that is not a token the client can fix by refreshing.
        [Fact]
        public void NonJson401_FailsWithApiExceptionAndNoRefresh()
        {
            var (api, issued) = HtmlRejectingApi();

            var e = Assert.Throws<ApiException>(() => api.GetSecret("key"));

            Assert.Equal(401, e.ErrorCode);
            Assert.Equal(1, issued());
        }

        [Fact]
        public async Task NonJson401_FailsWithApiExceptionAndNoRefresh_Async()
        {
            var (api, issued) = HtmlRejectingApi();

            var e = await Assert.ThrowsAsync<ApiException>(() => api.GetSecretAsync("key"));

            Assert.Equal(401, e.ErrorCode);
            Assert.Equal(1, issued());
        }

        private static (SecretResourceApi Api, Func<int> Issued) HtmlRejectingApi()
        {
            int issued = 0;
            var api = ApiFor(request => Task.FromResult(IsTokenRequest(request)
                ? (HttpStatusCode.OK, $"{{\"token\":\"t{Interlocked.Increment(ref issued)}\"}}")
                : (HttpStatusCode.Unauthorized, "<html><body><h1>401 Unauthorized</h1></body></html>")));
            return (api, () => Volatile.Read(ref issued));
        }

        private static (SecretResourceApi Api, ConcurrentQueue<string> SentTokens, Func<int> Issued) AlwaysRejectingApi()
        {
            int issued = 0;
            var sentTokens = new ConcurrentQueue<string>();
            var api = ApiFor(request =>
            {
                if (IsTokenRequest(request))
                {
                    return Task.FromResult((HttpStatusCode.OK, $"{{\"token\":\"t{Interlocked.Increment(ref issued)}\"}}"));
                }
                sentTokens.Enqueue(TokenOf(request));
                return Task.FromResult((HttpStatusCode.Unauthorized, InvalidToken));
            });
            return (api, sentTokens, () => Volatile.Read(ref issued));
        }

        private static void AssertEveryRefreshWasUsed(ApiException e, ConcurrentQueue<string> sentTokens, int issued)
        {
            Assert.Equal(401, e.ErrorCode);
            Assert.Equal(Constants.MAX_TOKEN_REFRESH_RETRY_COUNT + 1, sentTokens.Count);
            Assert.Equal(sentTokens.Count, issued);
            Assert.Equal($"t{issued}", sentTokens.Last());
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
