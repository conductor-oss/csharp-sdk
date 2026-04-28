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
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Orkes;
using System;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates scheduler operations: create, get, pause, resume, delete schedules.
    /// </summary>
    public class ScheduleJourney
    {
        private readonly ISchedulerClient _schedulerClient;
        private const string TEST_SCHEDULE = "csharp_test_schedule";

        public ScheduleJourney()
        {
            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            _schedulerClient = clients.GetSchedulerClient();
        }

        public void Run()
        {
            Console.WriteLine("=== Schedule Journey ===\n");

            // 1. Search existing schedule executions
            Console.WriteLine("1. Searching existing schedule executions...");
            var executions = _schedulerClient.SearchScheduleExecutions(start: 0, size: 10);
            Console.WriteLine($"   Found {executions?.Results?.Count ?? 0} schedule executions.");

            // 2. Get all schedule names
            Console.WriteLine("2. Getting all schedule names...");
            var names = _schedulerClient.GetAllSchedules();
            Console.WriteLine($"   Total schedules: {names?.Count ?? 0}");

            // 3. Get next execution times for a cron expression
            Console.WriteLine("3. Getting next execution times for cron '0 */5 * * * ?'...");
            var nextTimes = _schedulerClient.GetNextFewScheduleExecutionTimes("0 */5 * * * ?", limit: 5);
            if (nextTimes != null)
            {
                foreach (var time in nextTimes)
                {
                    Console.WriteLine($"   Next: {time}");
                }
            }

            Console.WriteLine("\nSchedule Journey completed!");
        }
    }
}
