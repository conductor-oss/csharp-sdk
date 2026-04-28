using Conductor.Client.Events;
using Conductor.Client.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Unit.Events
{
    public class EventDispatcherTests
    {
        [Fact]
        public void Register_TaskRunnerListener_ReceivesEvents()
        {
            var dispatcher = new EventDispatcher();
            var listener = new TestTaskRunnerListener();
            dispatcher.Register(listener);

            dispatcher.OnPolling("test_task", "worker-1", "default");

            Assert.Equal(1, listener.PollingCount);
            Assert.Equal("test_task", listener.LastTaskType);
        }

        [Fact]
        public void Register_WorkflowListener_ReceivesEvents()
        {
            var dispatcher = new EventDispatcher();
            var listener = new TestWorkflowListener();
            dispatcher.Register(listener);

            dispatcher.OnWorkflowStarted("wf-123", "my_workflow");

            Assert.Equal(1, listener.StartedCount);
            Assert.Equal("wf-123", listener.LastWorkflowId);
        }

        [Fact]
        public void Unregister_TaskRunnerListener_StopsReceivingEvents()
        {
            var dispatcher = new EventDispatcher();
            var listener = new TestTaskRunnerListener();
            dispatcher.Register(listener);
            dispatcher.Unregister(listener);

            dispatcher.OnPolling("test_task", "worker-1", "default");

            Assert.Equal(0, listener.PollingCount);
        }

        [Fact]
        public void Unregister_WorkflowListener_StopsReceivingEvents()
        {
            var dispatcher = new EventDispatcher();
            var listener = new TestWorkflowListener();
            dispatcher.Register(listener);
            dispatcher.Unregister(listener);

            dispatcher.OnWorkflowStarted("wf-123", "my_workflow");

            Assert.Equal(0, listener.StartedCount);
        }

        [Fact]
        public void MultipleListeners_AllReceiveEvents()
        {
            var dispatcher = new EventDispatcher();
            var listener1 = new TestTaskRunnerListener();
            var listener2 = new TestTaskRunnerListener();
            dispatcher.Register(listener1);
            dispatcher.Register(listener2);

            dispatcher.OnPolling("test_task", "worker-1", "default");

            Assert.Equal(1, listener1.PollingCount);
            Assert.Equal(1, listener2.PollingCount);
        }

        [Fact]
        public void ErrorInListener_DoesNotAffectOtherListeners()
        {
            var dispatcher = new EventDispatcher();
            var errorListener = new ErrorThrowingTaskRunnerListener();
            var normalListener = new TestTaskRunnerListener();
            dispatcher.Register(errorListener);
            dispatcher.Register(normalListener);

            dispatcher.OnPolling("test_task", "worker-1", "default");

            Assert.Equal(1, normalListener.PollingCount);
        }

        [Fact]
        public void AllTaskRunnerEvents_Dispatched()
        {
            var dispatcher = new EventDispatcher();
            var listener = new TestTaskRunnerListener();
            dispatcher.Register(listener);

            var tasks = new List<Task>();
            var task = new Task();
            var result = new TaskResult();
            var ex = new Exception("test error");

            dispatcher.OnPolling("t", "w", "d");
            dispatcher.OnPollSuccess("t", "w", tasks);
            dispatcher.OnPollEmpty("t", "w");
            dispatcher.OnPollError("t", "w", ex);
            dispatcher.OnTaskExecutionStarted("t", task);
            dispatcher.OnTaskExecutionCompleted("t", task, result);
            dispatcher.OnTaskExecutionFailed("t", task, ex);
            dispatcher.OnTaskUpdateSent("t", result);
            dispatcher.OnTaskUpdateFailed("t", result, ex);

            Assert.Equal(1, listener.PollingCount);
            Assert.Equal(1, listener.PollSuccessCount);
            Assert.Equal(1, listener.PollEmptyCount);
            Assert.Equal(1, listener.PollErrorCount);
            Assert.Equal(1, listener.ExecutionStartedCount);
            Assert.Equal(1, listener.ExecutionCompletedCount);
            Assert.Equal(1, listener.ExecutionFailedCount);
            Assert.Equal(1, listener.UpdateSentCount);
            Assert.Equal(1, listener.UpdateFailedCount);
        }

        [Fact]
        public void AllWorkflowEvents_Dispatched()
        {
            var dispatcher = new EventDispatcher();
            var listener = new TestWorkflowListener();
            dispatcher.Register(listener);

            dispatcher.OnWorkflowStarted("id", "type");
            dispatcher.OnWorkflowCompleted("id", "type");
            dispatcher.OnWorkflowFailed("id", "type", "reason");
            dispatcher.OnWorkflowTerminated("id", "type", "reason");
            dispatcher.OnWorkflowPaused("id", "type");
            dispatcher.OnWorkflowResumed("id", "type");

            Assert.Equal(1, listener.StartedCount);
            Assert.Equal(1, listener.CompletedCount);
            Assert.Equal(1, listener.FailedCount);
            Assert.Equal(1, listener.TerminatedCount);
            Assert.Equal(1, listener.PausedCount);
            Assert.Equal(1, listener.ResumedCount);
        }

        [Fact]
        public void Instance_ReturnsSingleton()
        {
            var instance1 = EventDispatcher.Instance;
            var instance2 = EventDispatcher.Instance;

            Assert.Same(instance1, instance2);
        }

        // Test helpers
        private class TestTaskRunnerListener : ITaskRunnerEventListener
        {
            public int PollingCount;
            public int PollSuccessCount;
            public int PollEmptyCount;
            public int PollErrorCount;
            public int ExecutionStartedCount;
            public int ExecutionCompletedCount;
            public int ExecutionFailedCount;
            public int UpdateSentCount;
            public int UpdateFailedCount;
            public string LastTaskType;

            public void OnPolling(string taskType, string workerId, string domain) { PollingCount++; LastTaskType = taskType; }
            public void OnPollSuccess(string taskType, string workerId, List<Task> tasks) { PollSuccessCount++; }
            public void OnPollEmpty(string taskType, string workerId) { PollEmptyCount++; }
            public void OnPollError(string taskType, string workerId, Exception exception) { PollErrorCount++; }
            public void OnTaskExecutionStarted(string taskType, Task task) { ExecutionStartedCount++; }
            public void OnTaskExecutionCompleted(string taskType, Task task, TaskResult result) { ExecutionCompletedCount++; }
            public void OnTaskExecutionFailed(string taskType, Task task, Exception exception) { ExecutionFailedCount++; }
            public void OnTaskUpdateSent(string taskType, TaskResult result) { UpdateSentCount++; }
            public void OnTaskUpdateFailed(string taskType, TaskResult result, Exception exception) { UpdateFailedCount++; }
        }

        private class ErrorThrowingTaskRunnerListener : ITaskRunnerEventListener
        {
            public void OnPolling(string taskType, string workerId, string domain) { throw new Exception("listener error"); }
            public void OnPollSuccess(string taskType, string workerId, List<Task> tasks) { throw new Exception("listener error"); }
            public void OnPollEmpty(string taskType, string workerId) { throw new Exception("listener error"); }
            public void OnPollError(string taskType, string workerId, Exception exception) { throw new Exception("listener error"); }
            public void OnTaskExecutionStarted(string taskType, Task task) { throw new Exception("listener error"); }
            public void OnTaskExecutionCompleted(string taskType, Task task, TaskResult result) { throw new Exception("listener error"); }
            public void OnTaskExecutionFailed(string taskType, Task task, Exception exception) { throw new Exception("listener error"); }
            public void OnTaskUpdateSent(string taskType, TaskResult result) { throw new Exception("listener error"); }
            public void OnTaskUpdateFailed(string taskType, TaskResult result, Exception exception) { throw new Exception("listener error"); }
        }

        private class TestWorkflowListener : IWorkflowEventListener
        {
            public int StartedCount;
            public int CompletedCount;
            public int FailedCount;
            public int TerminatedCount;
            public int PausedCount;
            public int ResumedCount;
            public string LastWorkflowId;

            public void OnWorkflowStarted(string workflowId, string workflowType) { StartedCount++; LastWorkflowId = workflowId; }
            public void OnWorkflowCompleted(string workflowId, string workflowType) { CompletedCount++; }
            public void OnWorkflowFailed(string workflowId, string workflowType, string reason) { FailedCount++; }
            public void OnWorkflowTerminated(string workflowId, string workflowType, string reason) { TerminatedCount++; }
            public void OnWorkflowPaused(string workflowId, string workflowType) { PausedCount++; }
            public void OnWorkflowResumed(string workflowId, string workflowType) { ResumedCount++; }
        }
    }
}
