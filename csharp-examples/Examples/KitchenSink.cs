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
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Definition.TaskType.LlmTasks;
using Conductor.Executor;
using System;
using System.Collections.Generic;

namespace Conductor.Examples
{
    /// <summary>
    /// Kitchen Sink example demonstrating all available task types in a single workflow.
    /// </summary>
    public class KitchenSink
    {
        private readonly WorkflowExecutor _workflowExecutor;

        public KitchenSink()
        {
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        public ConductorWorkflow CreateKitchenSinkWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("kitchen_sink_csharp")
                .WithDescription("Demonstrates all task types in C# SDK")
                .WithVersion(1);

            // 1. Simple Task
            var simpleTask = new SimpleTask("simple_task_ref", "simple_task")
                .WithInput("key", "value");

            // 2. HTTP Task
            var httpTask = new HttpTask("http_task_ref", new HttpTaskSettings
            {
                uri = "https://jsonplaceholder.typicode.com/posts/1"
            });

            // 3. HTTP Poll Task
            var httpPollTask = new HttpPollTask("http_poll_ref", new HttpTaskSettings
            {
                uri = "https://jsonplaceholder.typicode.com/posts/1"
            });

            // 4. Inline/Javascript Task
            var inlineTask = new InlineTask("inline_ref",
                "function e() { return $.input_val * 2; } e();");
            inlineTask.WithInput("input_val", 21);

            // 5. JSON JQ Transform
            var jqTask = new JQTask("jq_ref", ".input | { result: .value }")
                .WithInput("input", new Dictionary<string, object> { { "value", 42 } });

            // 6. Set Variable Task
            var setVarTask = new SetVariableTask("set_var_ref")
                .WithInput("my_var", "hello_from_set_variable");

            // 7. Wait Task
            var waitTask = new WaitTask("wait_ref", TimeSpan.FromSeconds(2));

            // 8. Sub Workflow Task
            var subWfParams = new SubWorkflowParams(name: "simple_sub_workflow", version: 1);
            var subWorkflow = new SubWorkflowTask("sub_wf_ref", subWfParams);

            // 9. Start Workflow Task
            var startWfTask = new StartWorkflowTask("start_wf_ref", "simple_sub_workflow", 1,
                input: new Dictionary<string, object> { { "param", "from_parent" } });

            // 10. Switch Task
            var switchTask = new SwitchTask("switch_ref", "$.case_value");
            var caseATask = new SimpleTask("case_a_ref", "case_a_task");
            var caseBTask = new SimpleTask("case_b_ref", "case_b_task");
            switchTask.WithDecisionCase("A", caseATask);
            switchTask.WithDecisionCase("B", caseBTask);

            // 11. Fork/Join Task
            var forkTask1 = new SimpleTask("fork_task_1_ref", "fork_task_1");
            var forkTask2 = new SimpleTask("fork_task_2_ref", "fork_task_2");
            var forkJoin = new ForkJoinTask("fork_ref", new WorkflowTask[] { forkTask1 }, new WorkflowTask[] { forkTask2 });

            // 12. Do-While Task
            var loopBody = new SimpleTask("loop_body_ref", "loop_body_task");
            var doWhile = new LoopTask("loop_ref", 3, loopBody);

            // 13. Terminate Task
            var terminateTask = new TerminateTask("terminate_ref",
                WorkflowStatus.StatusEnum.COMPLETED, "Kitchen sink completed successfully");

            // Build the workflow
            workflow
                .WithTask(simpleTask)
                .WithTask(httpTask)
                .WithTask(httpPollTask)
                .WithTask(inlineTask)
                .WithTask(jqTask)
                .WithTask(setVarTask)
                .WithTask(switchTask)
                .WithTask(forkJoin)
                .WithTask(doWhile)
                .WithTask(startWfTask)
                .WithTask(terminateTask);

            return workflow;
        }

        public void RegisterAndRun()
        {
            var workflow = CreateKitchenSinkWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);
            var workflowInput = new Dictionary<string, object>
            {
                { "case_value", "A" },
                { "input_val", 21 }
            };
            var workflowId = _workflowExecutor.StartWorkflow(new StartWorkflowRequest(name: workflow.Name, version: workflow.Version, input: workflowInput));
            Console.WriteLine($"Kitchen Sink workflow started. WorkflowId: {workflowId}");
        }
    }
}
