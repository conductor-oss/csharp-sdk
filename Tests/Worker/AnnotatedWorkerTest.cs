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
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Executor;
using conductor_csharp.test.Helper;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Tests.Worker
{
    [Trait("Category", "CloudIntegration")]
    public class AnnotatedWorkerTest
    {
        private readonly MetadataResourceApi _metaDataClient;
        private readonly WorkflowResourceApi _workflowClient;
        private readonly WorkflowExecutor _workflowExecutor;

        private const string WORKFLOW_NAME = "test-annotation";
        private const int WORKFLOW_VERSION = 1;
        private const string WORKFLOW_DESC = "test-annotation-desc";
        private const string OWNER_EMAIL = "test-sdk-csharp@conductor.com";

        public AnnotatedWorkerTest()
        {
            _metaDataClient = ApiExtensions.GetClient<MetadataResourceApi>();
            _workflowClient = ApiExtensions.GetClient<WorkflowResourceApi>();
            var config = new Configuration();
            _workflowExecutor = new WorkflowExecutor(config);

            //dev local testing
            // var _orkesApiClient = new OrkesApiClient(config, new OrkesAuthenticationSettings(Constants.KEY_ID, Constants.KEY_SECRET));
            //_metaDataClient = _orkesApiClient.GetClient<MetadataResourceApi>();
            //_workflowClient = _orkesApiClient.GetClient<WorkflowResourceApi>();
        }

        [Fact]
        public void TestAnnotatedMethods()
        {
            var workflow = GetConductorWorkflow();
            TaskDef taskDef = new TaskDef() { Description = TestConstants.TaskDescription, Name = TestConstants.QuoteTaskName, OwnerEmail = TestConstants.OWNER_EMAIL };
            var getQuoteTask = new SimpleTask(TestConstants.QuoteTaskName, TestConstants.QuoteTaskName);
            getQuoteTask.Description = TestConstants.TaskDescription;
            workflow.WithTask(getQuoteTask);

            RegisterAndStartWorkflow(workflow, new List<TaskDef> { taskDef });
        }

        [Fact]
        public void TestAnnotationWithInputParam()
        {
            var workflow = GetConductorWorkflow();
            TaskDef taskDef = new TaskDef() { Description = TestConstants.TaskDescription, Name = TestConstants.InputTaskName, OwnerEmail = TestConstants.OWNER_EMAIL };
            var getInputValue = new SimpleTask(TestConstants.InputTaskName, TestConstants.InputTaskName).WithInput("userId", workflow.Input("userId")).WithInput("otp", workflow.Input("otp"));
            getInputValue.Description = TestConstants.TaskDescription;
            workflow.WithTask(getInputValue);

            var testInput = new Dictionary<string, object>
            {
            { "userId", "Test" }, { "otp", 123 },
            };

            RegisterAndStartWorkflow(workflow, new List<TaskDef> { taskDef }, testInput);
        }

        [Fact]
        public void TestAnnotationWithMultipleInputParam()
        {
            var workflow = GetConductorWorkflow();
            TaskDef taskDef = new TaskDef() { Description = TestConstants.TaskDescription, Name = TestConstants.TestAddTask, OwnerEmail = TestConstants.OWNER_EMAIL };
            var getSumValue = new SimpleTask(TestConstants.TestAddTask, TestConstants.TestAddTask).WithInput("numberOne", workflow.Input("numberOne")).WithInput("numberTwo", workflow.Input("numberTwo"));
            getSumValue.Description = TestConstants.TaskDescription;
            workflow.WithTask(getSumValue);

            var testInput = new Dictionary<string, object>
            {
            { "numberOne", 7 }, { "numberTwo", 77 },
            };

            RegisterAndStartWorkflow(workflow, new List<TaskDef> { taskDef }, testInput);
        }

        [Fact]
        public void TestMultipleAnnotatedMethods()
        {
            var workflow = GetConductorWorkflow();
            List<TaskDef> taskDefs = new List<TaskDef>() {
                new TaskDef() { Description = TestConstants.TaskDescription, Name = TestConstants.TestAddTask, OwnerEmail = TestConstants.OWNER_EMAIL },
                new TaskDef() { Description = TestConstants.TaskDescription, Name = TestConstants.InputTaskName, OwnerEmail = TestConstants.OWNER_EMAIL }
            };
            var getInputValue = new SimpleTask(TestConstants.InputTaskName, TestConstants.InputTaskName).WithInput("userId", workflow.Input("userId")).WithInput("otp", workflow.Input("otp"));
            getInputValue.Description = TestConstants.TaskDescription;
            workflow.WithTask(getInputValue);
            var getSumValue = new SimpleTask(TestConstants.TestAddTask, TestConstants.TestAddTask).WithInput("numberOne", workflow.Input("numberOne")).WithInput("numberTwo", workflow.Input("numberTwo"));
            getSumValue.Description = TestConstants.TaskDescription;
            workflow.WithTask(getSumValue);

            var testInput = new Dictionary<string, object>
            {
            { "numberOne", 7 }, { "numberTwo", 77 },
            { "userId", "Test" }, { "otp", 123 }
            };

            RegisterAndStartWorkflow(workflow, taskDefs, testInput);
        }

        /// <summary>
        /// Returns a ConductorWorkflow object
        /// </summary>
        /// <returns></returns>
        private ConductorWorkflow GetConductorWorkflow()
        {
            return new ConductorWorkflow()
                .WithName(WORKFLOW_NAME)
                .WithVersion(WORKFLOW_VERSION)
                .WithDescription(WORKFLOW_DESC)
                .WithOwner(OWNER_EMAIL);

        }

        /// <summary>
        /// Registers and starts a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="taskDefs"></param>
        /// <param name="inputData"></param>
        private void RegisterAndStartWorkflow(ConductorWorkflow workflow, List<TaskDef> taskDefs, Dictionary<string, object> inputData = default)
        {
            _metaDataClient.RegisterTaskDef(taskDefs);
            _metaDataClient.UpdateWorkflowDefinitions(new List<WorkflowDef>(1) { workflow });

            StartWorkflowRequest startWorkflowRequest = new StartWorkflowRequest()
            {
                Name = workflow.Name,
                Input = inputData,
                Version = workflow.Version,
                WorkflowDef = workflow,
                CreatedBy = Constants.OWNER_EMAIL
            };

            _workflowClient.StartWorkflow(startWorkflowRequest);
            var waitHandle = new ManualResetEvent(false);
            var backgroundTask = System.Threading.Tasks.Task.Run(async () => await Conductor.Examples.Utils.WorkerUtil.StartBackGroundTask(waitHandle));
            waitHandle.WaitOne();
        }
    }
}
