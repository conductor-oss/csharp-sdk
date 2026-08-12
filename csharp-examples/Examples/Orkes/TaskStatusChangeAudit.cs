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
using Conductor.Client.Worker;
using Conductor.Examples.Copilot;
using Conductor.Executor;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

namespace csharp_examples
{
    [WorkerTask]
    public class TaskStatusChangeAudit
    {
        private readonly Conductor.Client.Configuration _configuration;
        private readonly WorkflowExecutor executor;
        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metaDataClient;
        private readonly ILogger _logger;
        private const string TYPE = "audit_log";
        private const string SIMPLETASK1 = "simple_task_1";
        private const string SIMPLETASK2 = "simple_task_2";
        private const string SIMPLETYPE = "SIMPLE";
        private const string SIMPLE_TASK2_REF_NAME = "simple_task_2_ref";
        private const string SIMPLE_TASK1_REF_NAME = "simple_task_1_ref";
        private const string WORKFLOW_DEF_NAME = "test_audit_logs";

        public TaskStatusChangeAudit()
        {
            _configuration = new Conductor.Client.Configuration();
            executor = new WorkflowExecutor(_configuration);
            _workflowClient = ApiExtensions.GetClient<WorkflowResourceApi>();
            _metaDataClient = ApiExtensions.GetClient<MetadataResourceApi>();
            _logger = ApplicationLogging.CreateLogger<OpenAICopilot>();

            //Local testing
            //OrkesApiClient orkesApiClient = new OrkesApiClient(configuration, new OrkesAuthenticationSettings(Constants.KEY_ID, Constants.KEY_SECRET));
            //_workflowClient = _orkesApiClient.GetClient<WorkflowResourceApi>();
            //_metaDataClient = _orkesApiClient.GetClient<MetadataResourceApi>();
        }

        [WorkerTask(taskType: "audit_log", batchSize: 5, pollIntervalMs: 200, workerId: "workerId")]
        public void AuditLog(object workflowInput, string status, string name)
        {
            _logger.LogInformation($"task {name} is in {status} status, with workflow input as {workflowInput}");
        }

        [WorkerTask(taskType: "simple_task_1", batchSize: 5, pollIntervalMs: 200, workerId: "workerId")]
        public static string SimpleTask1(Conductor.Client.Models.Task task)
        {
            return "OK";
        }

        [WorkerTask(taskType: "simple_task_2", batchSize: 5, pollIntervalMs: 200, workerId: "workerId")]
        public static TaskResult SimpleTask2(Conductor.Client.Models.Task task)
        {
            return new TaskResult { Status = TaskResult.StatusEnum.FAILEDWITHTERMINALERROR };
        }

        public void TaskStatusChangeAuditTest()
        {
            var workflowDef = new WorkflowDef() { Name = WORKFLOW_DEF_NAME, Version = 1, Description = "test" };
            // Create an instance of StateChangeEvent
            StateChangeEvent stateChangeEvent = new StateChangeEvent(
            type: TYPE,
            payload: new Dictionary<string, object>
            {
{ "workflow_input", "${workflow.input}" },
{ "status", "${simple_task_1_ref.status}" },
{ "name", SIMPLE_TASK1_REF_NAME }
            });

            var task1 = new WorkflowTask()
            {
                Type = SIMPLETYPE,
                Name = SIMPLETASK1,
                TaskReferenceName = SIMPLE_TASK1_REF_NAME,
                OnStateChange = new Dictionary<string, List<StateChangeEvent>>()
                {
                    { "onStart", new List<StateChangeEvent>() { stateChangeEvent } }
                }
            };

            var task_def = new TaskDef();
            task_def.Name = SIMPLETASK2;
            task_def.RetryCount = 0;

            StateChangeEvent stateChangeEvent2 = new StateChangeEvent(
            type: TYPE,
            payload: new Dictionary<string, object>
            {
{ "workflow_input", "${workflow.input}" },
{ "status", "${simple_task_2_ref.status}" },
{ "name", SIMPLE_TASK2_REF_NAME }
            });
            var task2 = new WorkflowTask()
            {
                Type = SIMPLETYPE,
                Name = SIMPLETASK2,
                TaskReferenceName = SIMPLE_TASK2_REF_NAME,
                TaskDefinition = task_def,
                OnStateChange = new Dictionary<string, List<StateChangeEvent>>()
                {
                    { "onStart", new List<StateChangeEvent>() { stateChangeEvent2 } },
                    { "onFailed", new List<StateChangeEvent>() { stateChangeEvent2 } },
                    { "onScheduled", new List<StateChangeEvent>() { stateChangeEvent2 } }
                }
            };

            workflowDef.Tasks.Add(task1);
            workflowDef.Tasks.Add(task2);

            var waitHandle = new ManualResetEvent(false);
            var backgroundTask = System.Threading.Tasks.Task.Run(async () => await Conductor.Examples.Utils.WorkerUtil.StartBackGroundTask(waitHandle));
            waitHandle.WaitOne();

            executor.RegisterWorkflow(workflowDef, true);

            var startReq = new StartWorkflowRequest()
            {
                Name = workflowDef.Name,
                Version = workflowDef.Version,
                Input = {
{ "a", "aa" },
{ "b", "bb" },
{ "c", 42 }
}
            };

            var workflowId = executor.StartWorkflow(startReq);
            _logger.LogInformation(workflowId);
        }
    }
}