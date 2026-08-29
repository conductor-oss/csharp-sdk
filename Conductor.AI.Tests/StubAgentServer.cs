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
// A stubbed agent server for tests that drive AgentHandle against canned
// responses, so an execution's terminal state can be stated as a payload.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Conductor.Client;
using RestSharp;

namespace Conductor.AI.Tests;

internal static class StubAgentServer
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> _respond;
        public StubHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var (status, body) = _respond(request);
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    /// <summary>A client configuration whose every request is answered by <paramref name="respond"/>.</summary>
    internal static Configuration Configure(Func<HttpRequestMessage, (HttpStatusCode, string)> respond)
    {
        var configuration = new Configuration { BasePath = "http://server/api" };
        configuration.ApiClient.RestClient = new RestClient(new RestClientOptions("http://server/api")
        {
            ConfigureMessageHandler = _ => new StubHandler(respond),
        });
        return configuration;
    }

    /// <summary>
    /// Answer the three reads <see cref="AgentHandle.WaitAsync"/> makes on reaching
    /// a terminal state: the status, the execution record, and the workflow with its tasks.
    /// </summary>
    internal static (HttpStatusCode, string) Route(
        HttpRequestMessage request, string statusBody, string executionBody, string workflowBody)
    {
        var path = request.RequestUri!.AbsolutePath;
        if (path.EndsWith("/status")) return (HttpStatusCode.OK, statusBody);
        if (path.Contains("/execution/")) return (HttpStatusCode.OK, executionBody);
        return (HttpStatusCode.OK, workflowBody);
    }
}
