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
// Scheduler pause/resume verb fallback.
//
// OSS/embedded Conductor accepts PUT for schedule pause/resume; Orkes Conductor historically
// accepted only GET. SchedulerResourceApi.PauseSchedule/ResumeSchedule send PUT first and fall
// back to GET only on an HTTP 405. Point CONDUCTOR_SERVER_URL at either server family and set a
// breakpoint in SchedulerResourceApi.ExecuteStateChange to step through it.
//
// Usage:
//   CONDUCTOR_SERVER_URL=http://localhost:8080/api \
//   dotnet run --project sdk/csharp/examples/93_SchedulerVerbFallback/Example93SchedulerVerbFallback.csproj [scheduleName]

using Conductor.Api;
using Conductor.Client.Models;

var serverUrl = Environment.GetEnvironmentVariable("CONDUCTOR_SERVER_URL") ?? "http://localhost:8080/api";
var scheduleName = args.Length > 0 ? args[0] : "demo-schedule";

Console.WriteLine($"Server:   {serverUrl}");
Console.WriteLine($"Schedule: {scheduleName}");

var client = new SchedulerResourceApi(serverUrl);

Console.WriteLine("\nCreating schedule...");
client.SaveSchedule(new SaveScheduleRequest(
    cronExpression: "0 0 9 * * *",
    name: scheduleName,
    startWorkflowRequest: new StartWorkflowRequest(name: "demo_workflow")));
Console.WriteLine("✓ Created");

Console.WriteLine("\nPausing...");
client.PauseSchedule(scheduleName);
Console.WriteLine("✓ Paused");

Console.WriteLine("\nResuming...");
client.ResumeSchedule(scheduleName);
Console.WriteLine("✓ Resumed");

Console.WriteLine("\nDeleting schedule...");
client.DeleteSchedule(scheduleName);
Console.WriteLine("✓ Deleted");
