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
using Conductor.Client.Interfaces;
using Conductor.Client.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using ConductorTask = Conductor.Client.Models.Task;
using ConductorTaskResult = Conductor.Client.Models.TaskResult;

namespace Tests.Unit.Worker
{
    /// <summary>
    /// Validates that WorkflowTaskExecutor falls back from task-update-v2 to v1
    /// when the server returns HTTP 404 or 405, and that the fallback is sticky.
    /// Mirrors the fix applied to the Python SDK (PRs #387 and #388).
    /// </summary>
    public class UpdateTaskV2FallbackTests
    {
        private readonly Mock<IWorkflowTaskClient> _taskClientMock;
        private readonly Mock<IWorkflowTask> _workerMock;
        private readonly WorkflowTaskExecutor _executor;
        private readonly ConductorTaskResult _taskResult;

        public UpdateTaskV2FallbackTests()
        {
            _taskClientMock = new Mock<IWorkflowTaskClient>();
            _workerMock = new Mock<IWorkflowTask>();

            var config = new WorkflowTaskExecutorConfiguration
            {
                WorkerId = "test-worker",
                Domain = "test-domain"
            };
            _workerMock.Setup(w => w.TaskType).Returns("TEST_TASK");
            _workerMock.Setup(w => w.WorkerSettings).Returns(config);

            var monitor = new WorkflowTaskMonitor(NullLogger<WorkflowTaskMonitor>.Instance);

            _executor = new WorkflowTaskExecutor(
                NullLogger<WorkflowTaskExecutor>.Instance,
                _taskClientMock.Object,
                _workerMock.Object,
                monitor
            );

            _taskResult = new ConductorTaskResult(taskId: "task-123", workflowInstanceId: "wf-456");
        }

        [Fact]
        public void UpdateTask_V2Succeeds_ReturnsNextTask()
        {
            var nextTask = new ConductorTask { TaskId = "next-task-id" };
            _taskClientMock
                .Setup(c => c.UpdateTaskAndGetNext(_taskResult, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(nextTask);

            var result = _executor.UpdateTask(_taskResult);

            Assert.Equal(nextTask, result);
            _taskClientMock.Verify(c => c.UpdateTask(It.IsAny<ConductorTaskResult>()), Times.Never);
        }

        [Fact]
        public void UpdateTask_V2Returns404_FallsBackToV1AndReturnsNull()
        {
            _taskClientMock
                .Setup(c => c.UpdateTaskAndGetNext(_taskResult, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException(404, "Not Found"));
            _taskClientMock
                .Setup(c => c.UpdateTask(_taskResult))
                .Returns("ok");

            var result = _executor.UpdateTask(_taskResult);

            Assert.Null(result);
            _taskClientMock.Verify(c => c.UpdateTask(_taskResult), Times.Once);
        }

        [Fact]
        public void UpdateTask_V2Returns405_FallsBackToV1AndReturnsNull()
        {
            _taskClientMock
                .Setup(c => c.UpdateTaskAndGetNext(_taskResult, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException(405, "Method Not Allowed"));
            _taskClientMock
                .Setup(c => c.UpdateTask(_taskResult))
                .Returns("ok");

            var result = _executor.UpdateTask(_taskResult);

            Assert.Null(result);
            _taskClientMock.Verify(c => c.UpdateTask(_taskResult), Times.Once);
        }

        [Fact]
        public void UpdateTask_FallbackIsSticky_V2NeverCalledAgainAfter404()
        {
            var taskResult2 = new ConductorTaskResult(taskId: "task-789", workflowInstanceId: "wf-456");

            _taskClientMock
                .Setup(c => c.UpdateTaskAndGetNext(_taskResult, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException(404, "Not Found"));
            _taskClientMock
                .Setup(c => c.UpdateTask(It.IsAny<ConductorTaskResult>()))
                .Returns("ok");

            // First call: hits 404, falls back to v1
            _executor.UpdateTask(_taskResult);

            // Second call: should go directly to v1, not try v2 again
            _executor.UpdateTask(taskResult2);

            _taskClientMock.Verify(
                c => c.UpdateTaskAndGetNext(It.IsAny<ConductorTaskResult>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once); // v2 only called once (first attempt)
            _taskClientMock.Verify(
                c => c.UpdateTask(It.IsAny<ConductorTaskResult>()),
                Times.Exactly(2)); // both calls use v1
        }

        [Fact]
        public void UpdateTask_FallbackIsSticky_V2NeverCalledAgainAfter405()
        {
            var taskResult2 = new ConductorTaskResult(taskId: "task-789", workflowInstanceId: "wf-456");

            _taskClientMock
                .Setup(c => c.UpdateTaskAndGetNext(_taskResult, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException(405, "Method Not Allowed"));
            _taskClientMock
                .Setup(c => c.UpdateTask(It.IsAny<ConductorTaskResult>()))
                .Returns("ok");

            _executor.UpdateTask(_taskResult);
            _executor.UpdateTask(taskResult2);

            _taskClientMock.Verify(
                c => c.UpdateTaskAndGetNext(It.IsAny<ConductorTaskResult>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
            _taskClientMock.Verify(
                c => c.UpdateTask(It.IsAny<ConductorTaskResult>()),
                Times.Exactly(2));
        }

        [Fact]
        public void UpdateTask_V2Returns500_DoesNotFallBack_Retries()
        {
            // 500 is a server error — should NOT trigger fallback, just retry
            _taskClientMock
                .Setup(c => c.UpdateTaskAndGetNext(_taskResult, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ApiException(500, "Internal Server Error"));

            Assert.Throws<System.Exception>(() => _executor.UpdateTask(_taskResult));

            // v2 should have been retried UPDATE_TASK_RETRY_COUNT_LIMIT times, never v1
            _taskClientMock.Verify(
                c => c.UpdateTaskAndGetNext(It.IsAny<ConductorTaskResult>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(5));
            _taskClientMock.Verify(c => c.UpdateTask(It.IsAny<ConductorTaskResult>()), Times.Never);
        }
    }
}
