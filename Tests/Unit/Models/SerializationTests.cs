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
using Conductor.Client.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit;

namespace Tests.Unit.Models
{
    /// <summary>
    /// Serialization/deserialization tests for key model classes.
    /// Ensures models can be correctly serialized to JSON and deserialized back.
    /// </summary>
    public class SerializationTests
    {
        private static T RoundTrip<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }

        #region WorkflowDef

        [Fact]
        public void WorkflowDef_SerializeDeserialize_PreservesProperties()
        {
            var wfDef = new WorkflowDef(name: "test_workflow", tasks: new List<WorkflowTask>
            {
                new WorkflowTask { Name = "task1", TaskReferenceName = "ref1", Type = "SIMPLE" }
            }, timeoutSeconds: 600);
            wfDef.Description = "Test description";
            wfDef.Version = 2;
            wfDef.Restartable = true;
            wfDef.OwnerEmail = "test@example.com";

            var result = RoundTrip(wfDef);

            Assert.Equal("test_workflow", result.Name);
            Assert.Equal("Test description", result.Description);
            Assert.Equal(2, result.Version);
            Assert.Equal(600, result.TimeoutSeconds);
            Assert.True(result.Restartable);
            Assert.Equal("test@example.com", result.OwnerEmail);
            Assert.Single(result.Tasks);
            Assert.Equal("task1", result.Tasks[0].Name);
        }

        [Fact]
        public void WorkflowDef_WithInputTemplate_SerializesCorrectly()
        {
            var wfDef = new WorkflowDef(name: "templated_wf", tasks: new List<WorkflowTask>(), timeoutSeconds: 0);
            wfDef.InputTemplate = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 }
            };

            var result = RoundTrip(wfDef);

            Assert.Equal("value1", result.InputTemplate["key1"].ToString());
            Assert.Equal("42", result.InputTemplate["key2"].ToString());
        }

        [Fact]
        public void WorkflowDef_WithTimeoutPolicy_SerializesEnum()
        {
            var wfDef = new WorkflowDef(name: "timeout_wf", tasks: new List<WorkflowTask>(), timeoutSeconds: 300);
            wfDef.TimeoutPolicy = WorkflowDef.TimeoutPolicyEnum.TIMEOUTWF;

            var json = JsonConvert.SerializeObject(wfDef);
            Assert.Contains("TIME_OUT_WF", json);

            var result = JsonConvert.DeserializeObject<WorkflowDef>(json);
            Assert.Equal(WorkflowDef.TimeoutPolicyEnum.TIMEOUTWF, result.TimeoutPolicy);
        }

        #endregion

        #region TaskDef

        [Fact]
        public void TaskDef_SerializeDeserialize_PreservesProperties()
        {
            var taskDef = new TaskDef(name: "my_task");
            taskDef.Description = "A test task";
            taskDef.RetryCount = 3;
            taskDef.RetryDelaySeconds = 10;
            taskDef.TimeoutSeconds = 120;
            taskDef.ConcurrentExecLimit = 5;

            var result = RoundTrip(taskDef);

            Assert.Equal("my_task", result.Name);
            Assert.Equal("A test task", result.Description);
            Assert.Equal(3, result.RetryCount);
            Assert.Equal(10, result.RetryDelaySeconds);
            Assert.Equal(120, result.TimeoutSeconds);
            Assert.Equal(5, result.ConcurrentExecLimit);
        }

        [Fact]
        public void TaskDef_RetryLogicEnum_Serializes()
        {
            var taskDef = new TaskDef(name: "retry_task");
            taskDef.RetryLogic = TaskDef.RetryLogicEnum.EXPONENTIALBACKOFF;

            var json = JsonConvert.SerializeObject(taskDef);
            Assert.Contains("EXPONENTIAL_BACKOFF", json);

            var result = JsonConvert.DeserializeObject<TaskDef>(json);
            Assert.Equal(TaskDef.RetryLogicEnum.EXPONENTIALBACKOFF, result.RetryLogic);
        }

        #endregion

        #region WorkflowTask

        [Fact]
        public void WorkflowTask_SimpleTask_SerializesCorrectly()
        {
            var task = new WorkflowTask
            {
                Name = "simple_task",
                TaskReferenceName = "ref1",
                Type = "SIMPLE",
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" },
                    { "param2", "${workflow.input.param}" }
                }
            };

            var result = RoundTrip(task);

            Assert.Equal("simple_task", result.Name);
            Assert.Equal("ref1", result.TaskReferenceName);
            Assert.Equal("SIMPLE", result.Type);
            Assert.Equal("value1", result.InputParameters["param1"].ToString());
        }

        [Fact]
        public void WorkflowTask_WithDecisionCases_SerializesNestedStructure()
        {
            var task = new WorkflowTask
            {
                Name = "switch",
                TaskReferenceName = "switch_ref",
                Type = "SWITCH",
                DecisionCases = new Dictionary<string, List<WorkflowTask>>
                {
                    { "case_a", new List<WorkflowTask>
                        {
                            new WorkflowTask { Name = "task_a", TaskReferenceName = "a_ref", Type = "SIMPLE" }
                        }
                    }
                }
            };

            var result = RoundTrip(task);

            Assert.NotNull(result.DecisionCases);
            Assert.True(result.DecisionCases.ContainsKey("case_a"));
            Assert.Single(result.DecisionCases["case_a"]);
            Assert.Equal("task_a", result.DecisionCases["case_a"][0].Name);
        }

        [Fact]
        public void WorkflowTask_WithForkTasks_SerializesDoublyNestedList()
        {
            var task = new WorkflowTask
            {
                Name = "fork",
                TaskReferenceName = "fork_ref",
                Type = "FORK_JOIN",
                ForkTasks = new List<List<WorkflowTask>>
                {
                    new List<WorkflowTask>
                    {
                        new WorkflowTask { Name = "branch1", TaskReferenceName = "b1_ref", Type = "SIMPLE" }
                    },
                    new List<WorkflowTask>
                    {
                        new WorkflowTask { Name = "branch2", TaskReferenceName = "b2_ref", Type = "SIMPLE" }
                    }
                }
            };

            var result = RoundTrip(task);

            Assert.Equal(2, result.ForkTasks.Count);
            Assert.Equal("branch1", result.ForkTasks[0][0].Name);
            Assert.Equal("branch2", result.ForkTasks[1][0].Name);
        }

        [Fact]
        public void WorkflowTask_TypeEnum_AllValuesSerialize()
        {
            // Test that key enum values serialize/deserialize correctly
            var enums = new[]
            {
                WorkflowTask.WorkflowTaskTypeEnum.SIMPLE,
                WorkflowTask.WorkflowTaskTypeEnum.HTTP,
                WorkflowTask.WorkflowTaskTypeEnum.SWITCH,
                WorkflowTask.WorkflowTaskTypeEnum.FORKJOIN,
                WorkflowTask.WorkflowTaskTypeEnum.DOWHILE,
                WorkflowTask.WorkflowTaskTypeEnum.SUBWORKFLOW,
                WorkflowTask.WorkflowTaskTypeEnum.LLMCHATCOMPLETE,
                WorkflowTask.WorkflowTaskTypeEnum.HTTPPOLL,
                WorkflowTask.WorkflowTaskTypeEnum.KAFKAPUBLISH,
            };

            foreach (var e in enums)
            {
                var task = new WorkflowTask { WorkflowTaskType = e };
                var result = RoundTrip(task);
                Assert.Equal(e, result.WorkflowTaskType);
            }
        }

        #endregion

        #region Task

        [Fact]
        public void Task_SerializeDeserialize_PreservesProperties()
        {
            var task = new Task
            {
                TaskId = "task-123",
                TaskType = "SIMPLE",
                TaskDefName = "my_task",
                Status = Task.StatusEnum.COMPLETED,
                WorkflowInstanceId = "wf-456",
                InputData = new Dictionary<string, object> { { "key", "value" } },
                OutputData = new Dictionary<string, object> { { "result", "success" } },
                RetryCount = 2
            };

            var result = RoundTrip(task);

            Assert.Equal("task-123", result.TaskId);
            Assert.Equal("SIMPLE", result.TaskType);
            Assert.Equal(Task.StatusEnum.COMPLETED, result.Status);
            Assert.Equal("wf-456", result.WorkflowInstanceId);
            Assert.Equal("value", result.InputData["key"].ToString());
            Assert.Equal("success", result.OutputData["result"].ToString());
        }

        [Fact]
        public void Task_StatusEnum_AllValuesRoundTrip()
        {
            var statuses = new[]
            {
                Task.StatusEnum.INPROGRESS,
                Task.StatusEnum.COMPLETED,
                Task.StatusEnum.FAILED,
                Task.StatusEnum.TIMEDOUT,
                Task.StatusEnum.CANCELED,
                Task.StatusEnum.SCHEDULED,
            };

            foreach (var status in statuses)
            {
                var task = new Task { Status = status };
                var result = RoundTrip(task);
                Assert.Equal(status, result.Status);
            }
        }

        #endregion

        #region TaskResult

        [Fact]
        public void TaskResult_SerializeDeserialize_PreservesProperties()
        {
            var result = new TaskResult
            {
                TaskId = "task-789",
                WorkflowInstanceId = "wf-101",
                WorkerId = "worker-1",
                Status = TaskResult.StatusEnum.COMPLETED,
                OutputData = new Dictionary<string, object> { { "output", 42 } },
                CallbackAfterSeconds = 30
            };

            var deserialized = RoundTrip(result);

            Assert.Equal("task-789", deserialized.TaskId);
            Assert.Equal("wf-101", deserialized.WorkflowInstanceId);
            Assert.Equal(TaskResult.StatusEnum.COMPLETED, deserialized.Status);
            Assert.Equal("42", deserialized.OutputData["output"].ToString());
        }

        #endregion

        #region StartWorkflowRequest

        [Fact]
        public void StartWorkflowRequest_SerializeDeserialize_Full()
        {
            var req = new StartWorkflowRequest(
                name: "test_wf",
                version: 1,
                input: new Dictionary<string, object>
                {
                    { "param1", "value1" },
                    { "param2", 100 }
                },
                correlationId: "corr-123",
                priority: 5,
                taskToDomain: new Dictionary<string, string>
                {
                    { "task1", "domain1" }
                }
            );

            var result = RoundTrip(req);

            Assert.Equal("test_wf", result.Name);
            Assert.Equal(1, result.Version);
            Assert.Equal("corr-123", result.CorrelationId);
            Assert.Equal(5, result.Priority);
            Assert.Equal("value1", result.Input["param1"].ToString());
            Assert.Equal("domain1", result.TaskToDomain["task1"]);
        }

        [Fact]
        public void StartWorkflowRequest_WithEmbeddedWorkflowDef_Serializes()
        {
            var wfDef = new WorkflowDef(name: "embedded_wf", tasks: new List<WorkflowTask>(), timeoutSeconds: 0);
            var req = new StartWorkflowRequest(name: "embedded_wf", workflowDef: wfDef);

            var result = RoundTrip(req);

            Assert.NotNull(result.WorkflowDef);
            Assert.Equal("embedded_wf", result.WorkflowDef.Name);
        }

        #endregion

        #region Workflow

        [Fact]
        public void Workflow_SerializeDeserialize_PreservesStatus()
        {
            var wf = new Workflow
            {
                WorkflowId = "wf-abc",
                WorkflowName = "test_wf",
                Status = Workflow.StatusEnum.RUNNING,
                Input = new Dictionary<string, object> { { "key", "value" } },
                Output = new Dictionary<string, object> { { "result", "done" } },
            };

            var result = RoundTrip(wf);

            Assert.Equal("wf-abc", result.WorkflowId);
            Assert.Equal("test_wf", result.WorkflowName);
            Assert.Equal(Workflow.StatusEnum.RUNNING, result.Status);
        }

        [Fact]
        public void Workflow_StatusEnum_AllValuesRoundTrip()
        {
            var statuses = new[]
            {
                Workflow.StatusEnum.RUNNING,
                Workflow.StatusEnum.COMPLETED,
                Workflow.StatusEnum.FAILED,
                Workflow.StatusEnum.TERMINATED,
                Workflow.StatusEnum.TIMEDOUT,
                Workflow.StatusEnum.PAUSED,
            };

            foreach (var status in statuses)
            {
                var wf = new Workflow { Status = status };
                var result = RoundTrip(wf);
                Assert.Equal(status, result.Status);
            }
        }

        #endregion

        #region WorkflowStatus

        [Fact]
        public void WorkflowStatus_SerializeDeserialize()
        {
            var status = new WorkflowStatus
            {
                WorkflowId = "wf-xyz",
                CorrelationId = "corr-1",
                Output = new Dictionary<string, object> { { "key", "val" } },
                Variables = new Dictionary<string, object> { { "var1", "val1" } }
            };

            var result = RoundTrip(status);

            Assert.Equal("wf-xyz", result.WorkflowId);
            Assert.Equal("corr-1", result.CorrelationId);
            Assert.Equal("val", result.Output["key"].ToString());
        }

        #endregion

        #region SubWorkflowParams

        [Fact]
        public void SubWorkflowParams_SerializeDeserialize()
        {
            var sub = new SubWorkflowParams(
                name: "child_wf",
                version: 2,
                taskToDomain: new Dictionary<string, string> { { "task1", "domain1" } }
            );

            var result = RoundTrip(sub);

            Assert.Equal("child_wf", result.Name);
            Assert.Equal(2, result.Version);
            Assert.Equal("domain1", result.TaskToDomain["task1"]);
        }

        [Fact]
        public void SubWorkflowParams_WithWorkflowDefinition_Serializes()
        {
            var wfDef = new WorkflowDef(name: "inline_wf", tasks: new List<WorkflowTask>(), timeoutSeconds: 0);
            var sub = new SubWorkflowParams(
                name: "inline_wf",
                workflowDefinition: wfDef
            );

            var result = RoundTrip(sub);

            Assert.NotNull(result.WorkflowDefinition);
            Assert.Equal("inline_wf", result.WorkflowDefinition.Name);
        }

        #endregion

        #region TaskMock

        [Fact]
        public void TaskMock_SerializeDeserialize()
        {
            var mock = new TaskMock(
                status: TaskMock.StatusEnum.COMPLETED,
                output: new Dictionary<string, object> { { "result", "mocked" } },
                executionTime: 100,
                queueWaitTime: 10
            );

            var result = RoundTrip(mock);

            Assert.Equal(TaskMock.StatusEnum.COMPLETED, result.Status);
            Assert.Equal("mocked", result.Output["result"].ToString());
            Assert.Equal(100, result.ExecutionTime);
            Assert.Equal(10, result.QueueWaitTime);
        }

        #endregion

        #region WorkflowTestRequest

        [Fact]
        public void WorkflowTestRequest_SerializeDeserialize()
        {
            var testReq = new WorkflowTestRequest(
                name: "test_wf",
                version: 1,
                input: new Dictionary<string, object> { { "key", "val" } },
                taskRefToMockOutput: new Dictionary<string, List<TaskMock>>
                {
                    { "task_ref", new List<TaskMock>
                        {
                            new TaskMock(status: TaskMock.StatusEnum.COMPLETED,
                                output: new Dictionary<string, object> { { "result", 42 } })
                        }
                    }
                }
            );

            var result = RoundTrip(testReq);

            Assert.Equal("test_wf", result.Name);
            Assert.Equal(1, result.Version);
            Assert.True(result.TaskRefToMockOutput.ContainsKey("task_ref"));
            Assert.Equal(TaskMock.StatusEnum.COMPLETED, result.TaskRefToMockOutput["task_ref"][0].Status);
        }

        #endregion

        #region Complex nested serialization

        [Fact]
        public void ComplexWorkflow_WithAllTaskTypes_SerializesCorrectly()
        {
            var wfDef = new WorkflowDef(
                name: "complex_wf",
                timeoutSeconds: 600,
                tasks: new List<WorkflowTask>
                {
                    new WorkflowTask
                    {
                        Name = "simple",
                        TaskReferenceName = "simple_ref",
                        Type = "SIMPLE"
                    },
                    new WorkflowTask
                    {
                        Name = "switch",
                        TaskReferenceName = "switch_ref",
                        Type = "SWITCH",
                        CaseExpression = "$.status",
                        DecisionCases = new Dictionary<string, List<WorkflowTask>>
                        {
                            { "ok", new List<WorkflowTask>
                                {
                                    new WorkflowTask { Name = "ok_task", TaskReferenceName = "ok_ref", Type = "SIMPLE" }
                                }
                            }
                        },
                        DefaultCase = new List<WorkflowTask>
                        {
                            new WorkflowTask { Name = "default_task", TaskReferenceName = "default_ref", Type = "SIMPLE" }
                        }
                    },
                    new WorkflowTask
                    {
                        Name = "fork",
                        TaskReferenceName = "fork_ref",
                        Type = "FORK_JOIN",
                        ForkTasks = new List<List<WorkflowTask>>
                        {
                            new List<WorkflowTask>
                            {
                                new WorkflowTask { Name = "branch1", TaskReferenceName = "b1", Type = "SIMPLE" }
                            }
                        }
                    }
                }
            );

            var json = JsonConvert.SerializeObject(wfDef, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<WorkflowDef>(json);

            Assert.Equal(3, result.Tasks.Count);
            Assert.Equal("switch", result.Tasks[1].Name);
            Assert.NotNull(result.Tasks[1].DecisionCases);
            Assert.NotNull(result.Tasks[2].ForkTasks);
        }

        #endregion
    }
}
