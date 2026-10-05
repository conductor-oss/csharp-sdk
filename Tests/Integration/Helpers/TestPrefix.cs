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
using SysEnv = System.Environment;

namespace Tests.Integration.Helpers
{
    /// <summary>
    /// Generates unique resource names per test run to avoid conflicts
    /// between concurrent runs or leftover data from previous runs.
    /// Format: csharp_sdk_{shortRunId}[_{attempt}]_{name}
    /// </summary>
    public static class TestPrefix
    {
        private static readonly string RunId = ResolveRunId();

        public static string Prefix => $"csharp_sdk_{RunId}";

        public static string Name(string name) => $"{Prefix}_{name}";

        // GITHUB_RUN_ID is stable across attempts of the same run, so on a re-run the task
        // queues would still be named after the failed attempt — and a task it orphaned (e.g.
        // a StartWorkflow whose response was lost) is still sitting in one, ready to be polled
        // by a test that did not create it. Including the attempt gives each try its own names.
        private static string ResolveRunId()
        {
            var runId = SysEnv.GetEnvironmentVariable("GITHUB_RUN_ID");
            if (string.IsNullOrEmpty(runId))
                return System.Guid.NewGuid().ToString("N")[..8];

            var attempt = SysEnv.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT");
            return string.IsNullOrEmpty(attempt) ? runId : $"{runId}_{attempt}";
        }
    }
}
