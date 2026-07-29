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
// Run Monitoring — trigger a deployed agent by name from a separate process.
//
// Demonstrates:
//   - runtime.RunByNameAsync(name, prompt) — running an agent without an Agent object
//   - The deploy/serve/run separation in practice
//
// This is the companion to 63d_ServeFromAssembly:
//   Terminal 1: dotnet run --project 63d_ServeFromAssembly  (deploys + runs workers)
//   Terminal 2: dotnet run --project 63e_RunMonitoring       (triggers the agent by name)
//
// RunByNameAsync() assumes the workflow is already registered on the server.
// It dispatches by workflow name — no Agent object or tool registration needed
// in this process.
//
// Requirements:
//   - Conductor server running at CONDUCTOR_SERVER_URL
//   - monitoring_63d agent previously deployed (run 63d first)

using Conductor.AI;

await using var runtime = new AgentRuntime();

Console.WriteLine("--- Run Monitoring Agent by Name ---");
Console.WriteLine("Triggering 'monitoring_63d' workflow on the server...\n");

var result = await runtime.RunByNameAsync(
    "monitoring_63d",
    "Is everything healthy? Run a full check.");

result.PrintResult();
