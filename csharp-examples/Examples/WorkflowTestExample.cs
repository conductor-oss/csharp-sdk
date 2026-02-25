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
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;
using System;
using System.Collections.Generic;

namespace Conductor.Examples
{
    /// <summary>
    /// Demonstrates how to use workflow testing with mock task outputs.
    /// Uses TestWorkflow API to verify workflow logic without actually executing tasks.
    /// </summary>
    public class WorkflowTestExample
    {
        private readonly IWorkflowClient _workflowClient;
        private readonly WorkflowExecutor _workflowExecutor;

        public WorkflowTestExample()
        {
            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            _workflowClient = clients.GetWorkflowClient();
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        public ConductorWorkflow CreateTestableWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("testable_workflow_csharp")
                .WithDescription("A workflow designed for testing with mock outputs")
                .WithVersion(1);

            // Task 1: Fetch data from an API
            var fetchTask = new HttpTask("fetch_data_ref", new HttpTaskSettings
            {
                uri = "https://api.example.com/data"
            });

            // Task 2: Process the fetched data
            var processTask = new SimpleTask("process_data_ref", "process_data")
                .WithInput("raw_data", "${fetch_data_ref.output.response.body}");

            // Task 3: Decision based on processed data
            var switchTask = new SwitchTask("decision_ref", "${process_data_ref.output.status}");
            var successTask = new SimpleTask("success_ref", "handle_success")
                .WithInput("data", "${process_data_ref.output.result}");
            var failTask = new SimpleTask("fail_ref", "handle_failure")
                .WithInput("error", "${process_data_ref.output.error}");
            switchTask.WithDecisionCase("SUCCESS", successTask);
            switchTask.WithDecisionCase("FAILURE", failTask);

            workflow
                .WithTask(fetchTask)
                .WithTask(processTask)
                .WithTask(switchTask);

            return workflow;
        }

        public void RunTest()
        {
            Console.WriteLine("=== Workflow Test Example ===\n");

            // 1. Register the workflow
            var workflow = CreateTestableWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);
            Console.WriteLine("1. Workflow registered.");

            // 2. Define mock outputs for each task reference
            Console.WriteLine("2. Creating test with mock task outputs...");
            var taskRefToMockOutput = new Dictionary<string, List<TaskMock>>();

            // Mock the HTTP fetch task
            taskRefToMockOutput["fetch_data_ref"] = new List<TaskMock>
            {
                new TaskMock(
                    status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object>
                    {
                        { "response", new Dictionary<string, object>
                            {
                                { "statusCode", 200 },
                                { "body", new Dictionary<string, object>
                                    {
                                        { "items", new List<string> { "item1", "item2", "item3" } },
                                        { "count", 3 }
                                    }
                                }
                            }
                        }
                    }
                )
            };

            // Mock the process data task - SUCCESS path
            taskRefToMockOutput["process_data_ref"] = new List<TaskMock>
            {
                new TaskMock(
                    status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object>
                    {
                        { "status", "SUCCESS" },
                        { "result", "Processed 3 items successfully" }
                    }
                )
            };

            // Mock the success handler
            taskRefToMockOutput["success_ref"] = new List<TaskMock>
            {
                new TaskMock(
                    status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object>
                    {
                        { "message", "All items handled" }
                    }
                )
            };

            // 3. Create and execute the test request
            var testRequest = new WorkflowTestRequest(
                name: workflow.Name,
                version: workflow.Version,
                taskRefToMockOutput: taskRefToMockOutput,
                workflowDef: workflow,
                input: new Dictionary<string, object>
                {
                    { "source", "test" }
                }
            );

            Console.WriteLine("3. Executing workflow test...");
            var result = _workflowClient.TestWorkflow(testRequest);

            Console.WriteLine($"   Test completed. Status: {result?.Status}");
            Console.WriteLine($"   Tasks executed: {result?.Tasks?.Count ?? 0}");
            if (result?.Tasks != null)
            {
                foreach (var task in result.Tasks)
                {
                    Console.WriteLine($"   - {task.ReferenceTaskName}: {task.Status}");
                }
            }

            // 4. Test the FAILURE path
            Console.WriteLine("\n4. Testing FAILURE path...");
            taskRefToMockOutput["process_data_ref"] = new List<TaskMock>
            {
                new TaskMock(
                    status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object>
                    {
                        { "status", "FAILURE" },
                        { "error", "Data validation failed" }
                    }
                )
            };

            taskRefToMockOutput["fail_ref"] = new List<TaskMock>
            {
                new TaskMock(
                    status: TaskMock.StatusEnum.COMPLETED,
                    output: new Dictionary<string, object>
                    {
                        { "message", "Error logged and notification sent" }
                    }
                )
            };

            var failureTestRequest = new WorkflowTestRequest(
                name: workflow.Name,
                version: workflow.Version,
                taskRefToMockOutput: taskRefToMockOutput,
                workflowDef: workflow,
                input: new Dictionary<string, object>
                {
                    { "source", "test" }
                }
            );

            var failureResult = _workflowClient.TestWorkflow(failureTestRequest);
            Console.WriteLine($"   Failure path test completed. Status: {failureResult?.Status}");
            if (failureResult?.Tasks != null)
            {
                foreach (var task in failureResult.Tasks)
                {
                    Console.WriteLine($"   - {task.ReferenceTaskName}: {task.Status}");
                }
            }

            Console.WriteLine("\nWorkflow Test Example completed!");
        }
    }
}
