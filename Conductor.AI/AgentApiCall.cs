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
using Conductor.Client;
using RestSharp;

namespace Conductor.AI;

/// <summary>
/// Low-level JSON-over-HTTP invocation shared by <see cref="OrkesAgentClient"/> and
/// <see cref="Scheduling.Schedules"/> — routes through <see cref="ApiClient.ExecuteAsync{T}"/>
/// on the caller's <see cref="Configuration"/>, inheriting X-Authorization injection,
/// <c>TokenHandler</c> mint/cache, and one-shot 401 refresh-retry. Callers own their
/// own error mapping (different response-status conventions per surface).
/// </summary>
internal static class AgentApiCall
{
    public static async Task<(int StatusCode, string? Body)> InvokeAsync(
        Configuration configuration, Method method, string path, System.Text.Json.Nodes.JsonNode? body,
        CancellationToken ct, List<KeyValuePair<string, string>>? queryParams = null)
    {
        var headerParams = new Dictionary<string, string>(configuration.DefaultHeader);
        var formParams = new Dictionary<string, string>();
        var fileParams = new Dictionary<string, FileParameter>();
        var pathParams = new Dictionary<string, string>();
        object? postBody = body?.ToJsonString();

        var resp = await configuration.ApiClient.ExecuteAsync<string>(
            path, method, queryParams ?? new List<KeyValuePair<string, string>>(), postBody,
            headerParams, formParams, fileParams, pathParams,
            "application/json", configuration, exceptionFactory: null, operationName: $"{method} {path}");

        return (resp.StatusCode, resp.Data);
    }
}
