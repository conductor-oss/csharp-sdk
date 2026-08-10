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
using conductor.csharp.Client.Extensions;
using Conductor.Api;
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace Test.Worker
{
    [Xunit.Trait("Category", "CloudIntegration")]
    public class TestWorkflows
    {
        private readonly WorkflowResourceApi workflowClient;
        private readonly MetadataResourceApi _metaDataClient;
        private ILogger _logger;

        public TestWorkflows()
        {
            workflowClient = ApiExtensions.GetClient<WorkflowResourceApi>();
            _metaDataClient = ApiExtensions.GetClient<MetadataResourceApi>();
            _logger = ApplicationLogging.CreateLogger<TestWorkflows>();
        }

        // Skipped: This test mocks hello_C_1 as FAILED first, then COMPLETED on retry.
        // The server's OrkesWorkflowTestService.runSimulation() has a bug in its
        // terminal-task cleanup loop (lines ~166-169) that calls mockData.remove(refName)
        // for every task whose status is terminal — including FAILED.  When the first
        // attempt of hello_C_1 fails, the cleanup destroys the remaining COMPLETED mock
        // that was queued for the retry attempt.  The retry sits in SCHEDULED forever and
        // the workflow never completes (returns RUNNING after maxLoops iterations).
        //
        // Fix: the cleanup should only remove a taskRef from mockData when the mock list
        // for that ref is already empty, e.g.:
        //   .filter(ref -> { var remaining = mockData.get(ref);
        //                    return remaining == null || remaining.isEmpty(); })
        //
        // Tracked: https://orkes.atlassian.net/browse/CCOR-12975
        [Fact(Skip = "Server bug: runSimulation() terminal-task cleanup destroys retry mock data — CCOR-12975")]
        public void TestWorkflowExecution()
        {
            var workflow = new ConductorWorkflow()
            .WithName("unit_testing_example")
            .WithDescription("test unit test")
            .WithVersion(1)
            .WithOwner("exampleEmail@conductor.com");
            var task1 = new SimpleTask("hello_C_1", "hello_ref_C_1");
            var task2 = new SimpleTask("hello_C_2", "hello_ref_C_2");
            var task3 = new SimpleTask("hello_C_3", "hello_ref_C_3");


            var decision = new SwitchTask("switch_ref", task1.Output("city"));
            decision.WithDecisionCase("NYC", task2);
            decision.WithDefaultCase(task3);

            var http = new HttpTask("http", new HttpTaskSettings { uri = "https://orkes-api-tester.orkesconductor.com/api" });
            workflow.WithTask(http);
            workflow.WithTask(task1);
            workflow.WithTask(decision);

            var taskRefToMockOutput = new Dictionary<string, List<TaskMock>>();

            taskRefToMockOutput[task1.TaskReferenceName] = new List<TaskMock>
{
new TaskMock { ExecutionTime= 1, Status = TaskMock.StatusEnum.FAILED, QueueWaitTime= 10, Output = new Dictionary<string, Object> {{ "key", "failed" }}},
new TaskMock{ ExecutionTime= 1, Status = TaskMock.StatusEnum.COMPLETED, QueueWaitTime=10, Output = new Dictionary<string, Object> {{"city", "NYC"}}}
};

            taskRefToMockOutput[task2.TaskReferenceName] = new List<TaskMock>
{
new TaskMock{ ExecutionTime= 1, Status = TaskMock.StatusEnum.COMPLETED, QueueWaitTime= 10, Output = new Dictionary < string, Object > {{ "key", "task2.output" }}}
};

            taskRefToMockOutput[http.TaskReferenceName] = new List<TaskMock>
{
new TaskMock{ ExecutionTime= 1, Status = TaskMock.StatusEnum.COMPLETED, QueueWaitTime= 10, Output = new Dictionary<string, Object> {{"key", "http.output"}}}
};

            _metaDataClient.UpdateWorkflowDefinitions(new List<WorkflowDef>(1) { workflow });

            var testRequest = new WorkflowTestRequest(name: workflow.Name, version: workflow.Version, taskRefToMockOutput: taskRefToMockOutput, workflowDef: workflow);
            var run = workflowClient.TestWorkflow(testRequest);

            _logger.LogInformation($"Completed the test run {run}");
            _logger.LogInformation($"Status: {run.Status}");
            Assert.Equal("COMPLETED", run.Status.ToString());

            _logger.LogInformation($"First task (HTTP) status: {run.Tasks[0].TaskType}");
            Assert.Equal("HTTP", run.Tasks[0].TaskType);

            _logger.LogInformation($"{run.Tasks[1].ReferenceTaskName} status: {run.Tasks[1].Status} (expected to be FAILED)");
            Assert.Equal("FAILED", run.Tasks[1].Status.ToString());

            _logger.LogInformation($"{run.Tasks[2].ReferenceTaskName} status: {run.Tasks[2].Status} (expected to be COMPLETED)");
            Assert.Equal("COMPLETED", run.Tasks[2].Status.ToString());

            _logger.LogInformation($"{run.Tasks[4].ReferenceTaskName} status: {run.Tasks[4].Status} (expected to be COMPLETED)");
            Assert.Equal("COMPLETED", run.Tasks[4].Status.ToString());

            //Assert that task2 was executed
            Assert.Equal(task2.TaskReferenceName, run.Tasks[4].ReferenceTaskName);
        }

        /// <summary>
        /// Same workflow shape as TestWorkflowExecution (HTTP → SIMPLE → Switch → SIMPLE)
        /// but without the FAILED→retry pattern that triggers the server bug.
        /// Each task gets a single COMPLETED mock so the simulation completes cleanly.
        /// </summary>
        [Fact]
        public void TestWorkflowExecutionNoRetry()
        {
            var workflow = new ConductorWorkflow()
            .WithName("unit_testing_example_no_retry")
            .WithDescription("test unit test with no retry")
            .WithVersion(1)
            .WithOwner("exampleEmail@conductor.com");
            var task1 = new SimpleTask("hello_C_1", "hello_ref_C_1");
            var task2 = new SimpleTask("hello_C_2", "hello_ref_C_2");
            var task3 = new SimpleTask("hello_C_3", "hello_ref_C_3");


            var decision = new SwitchTask("switch_ref", task1.Output("city"));
            decision.WithDecisionCase("NYC", task2);
            decision.WithDefaultCase(task3);

            var http = new HttpTask("http", new HttpTaskSettings { uri = "https://orkes-api-tester.orkesconductor.com/api" });
            workflow.WithTask(http);
            workflow.WithTask(task1);
            workflow.WithTask(decision);

            var taskRefToMockOutput = new Dictionary<string, List<TaskMock>>();

            taskRefToMockOutput[task1.TaskReferenceName] = new List<TaskMock>
{
new TaskMock{ ExecutionTime= 1, Status = TaskMock.StatusEnum.COMPLETED, QueueWaitTime=10, Output = new Dictionary<string, Object> {{"city", "NYC"}}}
};

            taskRefToMockOutput[task2.TaskReferenceName] = new List<TaskMock>
{
new TaskMock{ ExecutionTime= 1, Status = TaskMock.StatusEnum.COMPLETED, QueueWaitTime= 10, Output = new Dictionary < string, Object > {{ "key", "task2.output" }}}
};

            taskRefToMockOutput[http.TaskReferenceName] = new List<TaskMock>
{
new TaskMock{ ExecutionTime= 1, Status = TaskMock.StatusEnum.COMPLETED, QueueWaitTime= 10, Output = new Dictionary<string, Object> {{"key", "http.output"}}}
};

            _metaDataClient.UpdateWorkflowDefinitions(new List<WorkflowDef>(1) { workflow });

            var testRequest = new WorkflowTestRequest(name: workflow.Name, version: workflow.Version, taskRefToMockOutput: taskRefToMockOutput, workflowDef: workflow);
            var run = workflowClient.TestWorkflow(testRequest);

            _logger.LogInformation($"Completed the test run {run}");
            _logger.LogInformation($"Status: {run.Status}");
            Assert.Equal("COMPLETED", run.Status.ToString());

            _logger.LogInformation($"First task (HTTP) status: {run.Tasks[0].TaskType}");
            Assert.Equal("HTTP", run.Tasks[0].TaskType);
            Assert.Equal("COMPLETED", run.Tasks[0].Status.ToString());

            _logger.LogInformation($"{run.Tasks[1].ReferenceTaskName} status: {run.Tasks[1].Status} (expected to be COMPLETED)");
            Assert.Equal("COMPLETED", run.Tasks[1].Status.ToString());

            // Tasks[2] is the switch operator
            _logger.LogInformation($"{run.Tasks[3].ReferenceTaskName} status: {run.Tasks[3].Status} (expected to be COMPLETED)");
            Assert.Equal("COMPLETED", run.Tasks[3].Status.ToString());

            //Assert that task2 was executed (NYC branch)
            Assert.Equal(task2.TaskReferenceName, run.Tasks[3].ReferenceTaskName);
        }
    }
}