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

namespace Conductor.AI;

/// <summary>
/// Exposes the agent control-plane client from the standard client factory —
/// <c>orkesApiClient.GetAgentClient()</c> or <c>configuration.GetAgentClient()</c> —
/// both build an <see cref="OrkesAgentClient"/> on that same <see cref="Configuration"/>,
/// sharing its token cache with every other domain client built from it.
/// </summary>
public static class OrkesApiClientExtensions
{
    /// <summary>Build an <see cref="IAgentClient"/> sharing this client's <see cref="Configuration"/>.</summary>
    public static IAgentClient GetAgentClient(this OrkesApiClient client)
        => new OrkesAgentClient(client.Configuration);

    /// <summary>Build an <see cref="IAgentClient"/> on this <see cref="Configuration"/>.</summary>
    public static IAgentClient GetAgentClient(this Configuration configuration)
        => new OrkesAgentClient(configuration);
}
