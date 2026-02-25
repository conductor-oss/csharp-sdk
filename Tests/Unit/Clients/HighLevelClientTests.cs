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
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Tests.Unit.Clients
{
    /// <summary>
    /// Tests for the high-level client interfaces to verify they work with mocked implementations.
    /// </summary>
    public class HighLevelClientTests
    {
        #region IWorkflowClient

        [Fact]
        public void WorkflowClient_StartWorkflow_ReturnId()
        {
            var mock = new Mock<IWorkflowClient>();
            var req = new StartWorkflowRequest(name: "test_wf", version: 1);
            mock.Setup(c => c.StartWorkflow(req)).Returns("wf-123");

            var result = mock.Object.StartWorkflow(req);
            Assert.Equal("wf-123", result);
        }

        [Fact]
        public void WorkflowClient_GetWorkflow_ReturnsWorkflow()
        {
            var mock = new Mock<IWorkflowClient>();
            var wf = new Workflow { WorkflowId = "wf-456", Status = Workflow.StatusEnum.RUNNING };
            mock.Setup(c => c.GetWorkflow("wf-456", false)).Returns(wf);

            var result = mock.Object.GetWorkflow("wf-456", false);
            Assert.Equal(Workflow.StatusEnum.RUNNING, result.Status);
        }

        [Fact]
        public void WorkflowClient_PauseResumeTerminate_DoNotThrow()
        {
            var mock = new Mock<IWorkflowClient>();
            mock.Object.PauseWorkflow("wf-123");
            mock.Object.ResumeWorkflow("wf-123");
            mock.Object.Terminate("wf-123", "test reason");
            mock.Object.Restart("wf-123", true);
            mock.Object.Retry("wf-123");

            mock.Verify(c => c.PauseWorkflow("wf-123"), Times.Once);
            mock.Verify(c => c.ResumeWorkflow("wf-123"), Times.Once);
            mock.Verify(c => c.Terminate("wf-123", "test reason", false), Times.Once);
            mock.Verify(c => c.Restart("wf-123", true), Times.Once);
            mock.Verify(c => c.Retry("wf-123", false), Times.Once);
        }

        [Fact]
        public void WorkflowClient_Search_ReturnsResults()
        {
            var mock = new Mock<IWorkflowClient>();
            var searchResult = new ScrollableSearchResultWorkflowSummary();
            mock.Setup(c => c.Search("workflowType='test'", null, 0, 10, null)).Returns(searchResult);

            var result = mock.Object.Search(query: "workflowType='test'", start: 0, size: 10);
            Assert.NotNull(result);
        }

        [Fact]
        public void WorkflowClient_TestWorkflow_ReturnsWorkflow()
        {
            var mock = new Mock<IWorkflowClient>();
            var testReq = new WorkflowTestRequest(name: "test_wf");
            var wf = new Workflow { Status = Workflow.StatusEnum.COMPLETED };
            mock.Setup(c => c.TestWorkflow(testReq)).Returns(wf);

            var result = mock.Object.TestWorkflow(testReq);
            Assert.Equal(Workflow.StatusEnum.COMPLETED, result.Status);
        }

        [Fact]
        public void WorkflowClient_BulkOperations_ReturnBulkResponse()
        {
            var mock = new Mock<IWorkflowClient>();
            var ids = new List<string> { "wf-1", "wf-2" };
            var response = new BulkResponse();
            mock.Setup(c => c.PauseBulk(ids)).Returns(response);
            mock.Setup(c => c.ResumeBulk(ids)).Returns(response);
            mock.Setup(c => c.TerminateBulk(ids, null, false)).Returns(response);

            Assert.NotNull(mock.Object.PauseBulk(ids));
            Assert.NotNull(mock.Object.ResumeBulk(ids));
            Assert.NotNull(mock.Object.TerminateBulk(ids));
        }

        #endregion

        #region ITaskClient

        [Fact]
        public void TaskClient_PollTask_ReturnsTask()
        {
            var mock = new Mock<ITaskClient>();
            var task = new Task { TaskId = "task-1", Status = Task.StatusEnum.INPROGRESS };
            mock.Setup(c => c.PollTask("simple_task", "worker-1", null)).Returns(task);

            var result = mock.Object.PollTask("simple_task", "worker-1");
            Assert.Equal("task-1", result.TaskId);
        }

        [Fact]
        public void TaskClient_BatchPollTasks_ReturnsMultipleTasks()
        {
            var mock = new Mock<ITaskClient>();
            var tasks = new List<Task>
            {
                new Task { TaskId = "task-1" },
                new Task { TaskId = "task-2" }
            };
            mock.Setup(c => c.BatchPollTasks("simple_task", "worker-1", null, 2, 100)).Returns(tasks);

            var result = mock.Object.BatchPollTasks("simple_task", "worker-1", count: 2, timeout: 100);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void TaskClient_UpdateTask_ReturnsId()
        {
            var mock = new Mock<ITaskClient>();
            var taskResult = new TaskResult { TaskId = "task-1", Status = TaskResult.StatusEnum.COMPLETED };
            mock.Setup(c => c.UpdateTask(taskResult)).Returns("task-1");

            var result = mock.Object.UpdateTask(taskResult);
            Assert.Equal("task-1", result);
        }

        [Fact]
        public void TaskClient_GetQueueSizeForTasks_ReturnsDictionary()
        {
            var mock = new Mock<ITaskClient>();
            var sizes = new Dictionary<string, int?> { { "simple_task", 42 } };
            mock.Setup(c => c.GetQueueSizeForTasks(It.IsAny<List<string>>())).Returns(sizes);

            var result = mock.Object.GetQueueSizeForTasks(new List<string> { "simple_task" });
            Assert.Equal(42, result["simple_task"]);
        }

        #endregion

        #region IMetadataClient

        [Fact]
        public void MetadataClient_RegisterAndGetTaskDef()
        {
            var mock = new Mock<IMetadataClient>();
            var taskDef = new TaskDef(name: "test_task");
            mock.Setup(c => c.GetTaskDef("test_task")).Returns(taskDef);

            mock.Object.RegisterTaskDefs(new List<TaskDef> { taskDef });
            var result = mock.Object.GetTaskDef("test_task");

            Assert.Equal("test_task", result.Name);
            mock.Verify(c => c.RegisterTaskDefs(It.IsAny<List<TaskDef>>()), Times.Once);
        }

        [Fact]
        public void MetadataClient_RegisterAndGetWorkflowDef()
        {
            var mock = new Mock<IMetadataClient>();
            var wfDef = new WorkflowDef(name: "test_wf", tasks: new List<WorkflowTask>(), timeoutSeconds: 0);
            mock.Setup(c => c.GetWorkflowDef("test_wf", null)).Returns(wfDef);

            mock.Object.RegisterWorkflowDef(wfDef, true);
            var result = mock.Object.GetWorkflowDef("test_wf");

            Assert.Equal("test_wf", result.Name);
        }

        [Fact]
        public void MetadataClient_GetAllTaskDefs_ReturnsList()
        {
            var mock = new Mock<IMetadataClient>();
            var defs = new List<TaskDef>
            {
                new TaskDef(name: "task1"),
                new TaskDef(name: "task2")
            };
            mock.Setup(c => c.GetAllTaskDefs()).Returns(defs);

            var result = mock.Object.GetAllTaskDefs();
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region ISchedulerClient

        [Fact]
        public void SchedulerClient_SaveAndGetSchedule()
        {
            var mock = new Mock<ISchedulerClient>();
            var schedule = new WorkflowSchedule();
            mock.Setup(c => c.GetSchedule("test_schedule")).Returns(schedule);

            mock.Object.SaveSchedule(new SaveScheduleRequest(
                name: "test_schedule",
                cronExpression: "0 0 * * *",
                startWorkflowRequest: new StartWorkflowRequest(name: "scheduled_wf")));
            var result = mock.Object.GetSchedule("test_schedule");

            Assert.NotNull(result);
            mock.Verify(c => c.SaveSchedule(It.IsAny<SaveScheduleRequest>()), Times.Once);
        }

        [Fact]
        public void SchedulerClient_PauseAndResume()
        {
            var mock = new Mock<ISchedulerClient>();
            mock.Object.PauseSchedule("test");
            mock.Object.ResumeSchedule("test");

            mock.Verify(c => c.PauseSchedule("test"), Times.Once);
            mock.Verify(c => c.ResumeSchedule("test"), Times.Once);
        }

        #endregion

        #region ISecretClient

        [Fact]
        public void SecretClient_PutGetDeleteSecret()
        {
            var mock = new Mock<ISecretClient>();
            mock.Setup(c => c.GetSecret("my_secret")).Returns("secret_value");
            mock.Setup(c => c.SecretExists("my_secret")).Returns(true);

            mock.Object.PutSecret("my_secret", "secret_value");
            Assert.Equal("secret_value", mock.Object.GetSecret("my_secret"));
            Assert.True(mock.Object.SecretExists("my_secret"));

            mock.Object.DeleteSecret("my_secret");
            mock.Verify(c => c.DeleteSecret("my_secret"), Times.Once);
        }

        #endregion

        #region IPromptClient

        [Fact]
        public void PromptClient_SaveAndGetMessageTemplate()
        {
            var mock = new Mock<IPromptClient>();
            var template = new MessageTemplate();
            mock.Setup(c => c.GetMessageTemplate("test_prompt")).Returns(template);

            mock.Object.SaveMessageTemplate("test_prompt", "template text", "description");
            var result = mock.Object.GetMessageTemplate("test_prompt");

            Assert.NotNull(result);
            mock.Verify(c => c.SaveMessageTemplate("test_prompt", "template text", "description", null), Times.Once);
        }

        #endregion

        #region IAuthorizationClient

        [Fact]
        public void AuthorizationClient_ListApplicationsAndUsers()
        {
            var mock = new Mock<IAuthorizationClient>();
            mock.Setup(c => c.ListApplications()).Returns(new List<ExtendedConductorApplication>());
            mock.Setup(c => c.ListUsers(null)).Returns(new List<ConductorUser>());

            Assert.NotNull(mock.Object.ListApplications());
            Assert.NotNull(mock.Object.ListUsers());
        }

        [Fact]
        public void AuthorizationClient_GroupCRUD()
        {
            var mock = new Mock<IAuthorizationClient>();
            mock.Setup(c => c.ListGroups()).Returns(new List<Group>());

            var req = new UpsertGroupRequest(description: "test");
            mock.Object.UpsertGroup(req, "group-1");
            mock.Object.DeleteGroup("group-1");

            mock.Verify(c => c.UpsertGroup(req, "group-1"), Times.Once);
            mock.Verify(c => c.DeleteGroup("group-1"), Times.Once);
        }

        #endregion
    }
}
