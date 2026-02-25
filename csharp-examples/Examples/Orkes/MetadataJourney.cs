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
using Conductor.Client.Models;
using Conductor.Client.Orkes;
using System;
using System.Collections.Generic;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates the full lifecycle of metadata operations using the high-level MetadataClient.
    /// Covers: task definition CRUD, workflow definition CRUD, tagging.
    /// </summary>
    public class MetadataJourney
    {
        private readonly IMetadataClient _metadataClient;
        private const string TEST_TASK = "csharp_test_task_def";
        private const string TEST_WORKFLOW = "csharp_test_workflow_def";

        public MetadataJourney()
        {
            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            _metadataClient = clients.GetMetadataClient();
        }

        public void Run()
        {
            // 1. Register a task definition
            Console.WriteLine("1. Registering task definition...");
            var taskDef = new TaskDef(name: TEST_TASK, description: "Test task for metadata journey");
            taskDef.RetryCount = 3;
            taskDef.TimeoutSeconds = 300;
            _metadataClient.RegisterTaskDefs(new List<TaskDef> { taskDef });
            Console.WriteLine($"   Task '{TEST_TASK}' registered.");

            // 2. Get the task definition
            Console.WriteLine("2. Getting task definition...");
            var fetchedTask = _metadataClient.GetTaskDef(TEST_TASK);
            Console.WriteLine($"   Task: {fetchedTask.Name}, RetryCount: {fetchedTask.RetryCount}");

            // 3. Update the task definition
            Console.WriteLine("3. Updating task definition...");
            fetchedTask.RetryCount = 5;
            _metadataClient.UpdateTaskDef(fetchedTask);
            var updated = _metadataClient.GetTaskDef(TEST_TASK);
            Console.WriteLine($"   Updated RetryCount: {updated.RetryCount}");

            // 4. Register a workflow definition
            Console.WriteLine("4. Registering workflow definition...");
            var workflowDef = new WorkflowDef(name: TEST_WORKFLOW, version: 1);
            workflowDef.Description = "Test workflow for metadata journey";
            workflowDef.Tasks = new List<WorkflowTask>
            {
                new WorkflowTask { Name = TEST_TASK, TaskReferenceName = "task_ref_1", Type = "SIMPLE" }
            };
            _metadataClient.RegisterWorkflowDef(workflowDef, true);
            Console.WriteLine($"   Workflow '{TEST_WORKFLOW}' registered.");

            // 5. Get the workflow definition
            Console.WriteLine("5. Getting workflow definition...");
            var fetchedWf = _metadataClient.GetWorkflowDef(TEST_WORKFLOW, 1);
            Console.WriteLine($"   Workflow: {fetchedWf.Name}, Version: {fetchedWf.Version}");

            // 6. Get all task definitions
            Console.WriteLine("6. Getting all task definitions...");
            var allTasks = _metadataClient.GetAllTaskDefs();
            Console.WriteLine($"   Total task definitions: {allTasks.Count}");

            // 7. Clean up
            Console.WriteLine("7. Cleaning up...");
            _metadataClient.UnregisterWorkflowDef(TEST_WORKFLOW, 1);
            _metadataClient.UnregisterTaskDef(TEST_TASK);
            Console.WriteLine("   Cleanup complete.");
            Console.WriteLine("\nMetadata Journey completed successfully!");
        }
    }
}
