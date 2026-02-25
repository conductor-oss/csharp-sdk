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
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates the complete workflow lifecycle:
    /// start, get, pause, resume, terminate, restart, retry, rerun, search.
    /// </summary>
    public class WorkflowOps
    {
        private readonly IWorkflowClient _workflowClient;
        private readonly IMetadataClient _metadataClient;
        private readonly WorkflowExecutor _workflowExecutor;

        public WorkflowOps()
        {
            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            _workflowClient = clients.GetWorkflowClient();
            _metadataClient = clients.GetMetadataClient();
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        public void Run()
        {
            Console.WriteLine("=== Workflow Operations Journey ===\n");

            // 1. Create and register a simple workflow
            Console.WriteLine("1. Creating and registering workflow...");
            var workflow = new ConductorWorkflow()
                .WithName("workflow_ops_demo_csharp")
                .WithDescription("Demo workflow for operations")
                .WithVersion(1)
                .WithTask(new WaitTask("wait_step", TimeSpan.FromMinutes(5)));

            _workflowExecutor.RegisterWorkflow(workflow, true);

            // 2. Start workflow
            Console.WriteLine("2. Starting workflow...");
            var startReq = new StartWorkflowRequest(name: "workflow_ops_demo_csharp", version: 1);
            var workflowId = _workflowClient.StartWorkflow(startReq);
            Console.WriteLine($"   Workflow started: {workflowId}");

            Thread.Sleep(1000);

            // 3. Get workflow status
            Console.WriteLine("3. Getting workflow status...");
            var wf = _workflowClient.GetWorkflow(workflowId, false);
            Console.WriteLine($"   Status: {wf.Status}");

            // 4. Pause workflow
            Console.WriteLine("4. Pausing workflow...");
            _workflowClient.PauseWorkflow(workflowId);
            wf = _workflowClient.GetWorkflow(workflowId, false);
            Console.WriteLine($"   Status after pause: {wf.Status}");

            // 5. Resume workflow
            Console.WriteLine("5. Resuming workflow...");
            _workflowClient.ResumeWorkflow(workflowId);
            wf = _workflowClient.GetWorkflow(workflowId, false);
            Console.WriteLine($"   Status after resume: {wf.Status}");

            // 6. Terminate workflow
            Console.WriteLine("6. Terminating workflow...");
            _workflowClient.Terminate(workflowId, "Demo termination");
            wf = _workflowClient.GetWorkflow(workflowId, false);
            Console.WriteLine($"   Status after terminate: {wf.Status}");

            // 7. Restart workflow
            Console.WriteLine("7. Restarting workflow...");
            _workflowClient.Restart(workflowId, true);
            wf = _workflowClient.GetWorkflow(workflowId, false);
            Console.WriteLine($"   Status after restart: {wf.Status}");

            // 8. Terminate and clean up
            Console.WriteLine("8. Final cleanup...");
            _workflowClient.Terminate(workflowId, "Final cleanup");

            // 9. Search workflows
            Console.WriteLine("9. Searching workflows...");
            var searchResults = _workflowClient.Search(query: "workflowType='workflow_ops_demo_csharp'", start: 0, size: 5);
            Console.WriteLine($"   Found {searchResults?.Results?.Count ?? 0} workflows.");

            Console.WriteLine("\nWorkflow Operations Journey completed!");
        }
    }
}
