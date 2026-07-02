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
using System.Collections.Generic;
using Conductor.Client.Models;
using Xunit;

namespace Tests.Models
{
    public class EnvironmentAndStateChangeModelTests
    {
        [Fact]
        public void EnvironmentVariable_ExposesNameValueAndTags()
        {
            var variable = new EnvironmentVariable
            {
                Name = "MY_KEY",
                Value = "my-value",
                Tags = new List<Tag> { new Tag() }
            };

            Assert.Equal("MY_KEY", variable.Name);
            Assert.Equal("my-value", variable.Value);
            Assert.Single(variable.Tags);
        }

        [Fact]
        public void StateChangeEvent_ConstructorAssignsTypeAndPayload()
        {
            var payload = new Dictionary<string, object> { ["taskRef"] = "audit_task" };

            var stateChangeEvent = new StateChangeEvent("onSuccess", payload);

            Assert.Equal("onSuccess", stateChangeEvent.Type);
            Assert.Same(payload, stateChangeEvent.Payload);
            Assert.Equal("audit_task", stateChangeEvent.Payload["taskRef"]);
        }

        [Fact]
        public void WorkflowTask_OnStateChange_HoldsListOfStateChangeEvents()
        {
            var onStateChange = new Dictionary<string, List<StateChangeEvent>>
            {
                ["onSuccess"] = new List<StateChangeEvent>
                {
                    new StateChangeEvent("onSuccess", new Dictionary<string, object>())
                }
            };

            var task = new WorkflowTask(taskReferenceName: "task_ref", onStateChange: onStateChange);

            Assert.Equal("task_ref", task.TaskReferenceName);
            Assert.Single(task.OnStateChange);
            Assert.Single(task.OnStateChange["onSuccess"]);
            Assert.Equal("onSuccess", task.OnStateChange["onSuccess"][0].Type);
        }
    }
}
