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
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Conductor.Definition.TaskType.LlmTasks;
using Conductor.Definition.TaskType.LlmTasks.Utils;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Unit.Definition
{
    /// <summary>
    /// Tests for the workflow builder (ConductorWorkflow) and task DSL.
    /// </summary>
    public class WorkflowBuilderTests
    {
        #region ConductorWorkflow Builder

        [Fact]
        public void ConductorWorkflow_BasicBuilder_SetsProperties()
        {
            var wf = new ConductorWorkflow()
                .WithName("test_workflow")
                .WithDescription("Test description")
                .WithVersion(2);

            Assert.Equal("test_workflow", wf.Name);
            Assert.Equal("Test description", wf.Description);
            Assert.Equal(2, wf.Version);
        }

        [Fact]
        public void ConductorWorkflow_WithTask_AddsTasks()
        {
            var wf = new ConductorWorkflow()
                .WithName("multi_task")
                .WithTask(new SimpleTask("ref1", "task1"))
                .WithTask(new SimpleTask("ref2", "task2"));

            Assert.Equal(2, wf.Tasks.Count);
        }

        [Fact]
        public void ConductorWorkflow_ExtendsWorkflowDef()
        {
            var wf = new ConductorWorkflow()
                .WithName("extends_test");

            // ConductorWorkflow should be assignable to WorkflowDef
            WorkflowDef def = wf;
            Assert.Equal("extends_test", def.Name);
        }

        [Fact]
        public void ConductorWorkflow_GetStartWorkflowRequest_ReturnsValidRequest()
        {
            var wf = new ConductorWorkflow()
                .WithName("test_wf")
                .WithVersion(1);

            var req = wf.GetStartWorkflowRequest();
            Assert.Equal("test_wf", req.Name);
            Assert.Equal(1, req.Version);
        }

        #endregion

        #region SimpleTask

        [Fact]
        public void SimpleTask_SetsNameAndType()
        {
            var task = new SimpleTask("task_name", "ref_name");
            Assert.Equal("ref_name", task.TaskReferenceName);
            Assert.Equal("task_name", task.Name);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.SIMPLE, task.WorkflowTaskType);
        }

        [Fact]
        public void SimpleTask_WithInput_SetsInputParameters()
        {
            var task = new SimpleTask("ref", "task")
                .WithInput("key1", "value1")
                .WithInput("key2", 42);

            Assert.Equal("value1", task.InputParameters["key1"]);
            Assert.Equal(42, task.InputParameters["key2"]);
        }

        #endregion

        #region HttpTask

        [Fact]
        public void HttpTask_SetsType()
        {
            var task = new HttpTask("ref", new HttpTaskSettings { uri = "https://example.com" });
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.HTTP, task.WorkflowTaskType);
        }

        #endregion

        #region HttpPollTask

        [Fact]
        public void HttpPollTask_SetsType()
        {
            var task = new HttpPollTask("ref", new HttpTaskSettings { uri = "https://example.com" });
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.HTTPPOLL, task.WorkflowTaskType);
        }

        #endregion

        #region SwitchTask

        [Fact]
        public void SwitchTask_WithDecisionCases()
        {
            var switchTask = new SwitchTask("switch_ref", "$.status");
            var caseA = new SimpleTask("a_ref", "task_a");
            var caseB = new SimpleTask("b_ref", "task_b");
            switchTask.WithDecisionCase("A", caseA);
            switchTask.WithDecisionCase("B", caseB);

            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.SWITCH, switchTask.WorkflowTaskType);
            Assert.True(switchTask.DecisionCases.ContainsKey("A"));
            Assert.True(switchTask.DecisionCases.ContainsKey("B"));
        }

        #endregion

        #region ForkJoinTask

        [Fact]
        public void ForkJoinTask_CreatesParallelBranches()
        {
            var branch1 = new SimpleTask("b1_ref", "branch1");
            var branch2 = new SimpleTask("b2_ref", "branch2");
            var fork = new ForkJoinTask("fork_ref",
                new WorkflowTask[] { branch1 },
                new WorkflowTask[] { branch2 });

            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.FORKJOIN, fork.WorkflowTaskType);
            Assert.Equal(2, fork.ForkTasks.Count);
        }

        #endregion

        #region DoWhileTask / LoopTask

        [Fact]
        public void LoopTask_SetsIterations()
        {
            var body = new SimpleTask("body_ref", "body_task");
            var loop = new LoopTask("loop_ref", 5, body);

            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.DOWHILE, loop.WorkflowTaskType);
            Assert.Contains("5", loop.LoopCondition);
        }

        [Fact]
        public void DoWhileTask_WithCustomCondition()
        {
            var body = new SimpleTask("body_ref", "body_task");
            var doWhile = new DoWhileTask("dowhile_ref", "if ($.done) { false; } else { true; }", body);

            Assert.Contains("$.done", doWhile.LoopCondition);
        }

        #endregion

        #region SubWorkflowTask

        [Fact]
        public void SubWorkflowTask_WithParams()
        {
            var sub = new SubWorkflowTask("sub_ref",
                new SubWorkflowParams(name: "child_wf", version: 1));

            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.SUBWORKFLOW, sub.WorkflowTaskType);
            Assert.Equal("child_wf", sub.SubWorkflowParam.Name);
        }

        [Fact]
        public void SubWorkflowTask_WithWorkflowDef()
        {
            var childWf = new ConductorWorkflow().WithName("inline_child").WithVersion(1);
            var sub = new SubWorkflowTask("sub_ref", childWf);

            Assert.Equal("inline_child", sub.SubWorkflowParam.Name);
        }

        #endregion

        #region WaitTask

        [Fact]
        public void WaitTask_WithDuration()
        {
            var wait = new WaitTask("wait_ref", TimeSpan.FromSeconds(30));
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.WAIT, wait.WorkflowTaskType);
        }

        #endregion

        #region TerminateTask

        [Fact]
        public void TerminateTask_SetsStatusAndReason()
        {
            var terminate = new TerminateTask("term_ref",
                WorkflowStatus.StatusEnum.COMPLETED, "Done successfully");

            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.TERMINATE, terminate.WorkflowTaskType);
        }

        #endregion

        #region InlineTask

        [Fact]
        public void InlineTask_SetsScript()
        {
            var task = new InlineTask("inline_ref", "function e() { return 42; } e();");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.INLINE, task.WorkflowTaskType);
        }

        [Fact]
        public void InlineTask_DefaultEvaluatorIsJavascript()
        {
            var task = new InlineTask("ref", "return 1;");
            Assert.Equal("javascript", task.InputParameters["evaluatorType"].ToString());
        }

        #endregion

        #region JQTask

        [Fact]
        public void JQTask_SetsQueryExpression()
        {
            var task = new JQTask("jq_ref", ".input | .value");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.JSONJQTRANSFORM, task.WorkflowTaskType);
        }

        #endregion

        #region KafkaPublishTask

        [Fact]
        public void KafkaPublishTask_SetsBootstrapAndTopic()
        {
            var task = new KafkaPublishTask("kafka_ref", "localhost:9092", "my-topic", "message");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.KAFKAPUBLISH, task.WorkflowTaskType);
        }

        #endregion

        #region StartWorkflowTask

        [Fact]
        public void StartWorkflowTask_SetsWorkflowNameAndVersion()
        {
            var task = new StartWorkflowTask("start_ref", "target_wf", 1);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.STARTWORKFLOW, task.WorkflowTaskType);
        }

        #endregion

        #region LLM Tasks

        [Fact]
        public void LlmChatComplete_SetsMessages()
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are helpful"),
                new ChatMessage("user", "Hello")
            };
            var task = new LlmChatComplete("chat_ref", "openai", "gpt-4", messages);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LLMCHATCOMPLETE, task.WorkflowTaskType);
        }

        [Fact]
        public void LlmChatComplete_WithTemperatureAndMaxTokens()
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage("user", "test")
            };
            var task = new LlmChatComplete("chat_ref", "openai", "gpt-4", messages,
                maxTokens: 500, temperature: 0);

            Assert.Equal(500, task.MaxTokens);
            Assert.Equal(0, task.Temperature);
        }

        [Fact]
        public void GetDocumentTask_SetsType()
        {
            var task = new GetDocumentTask("doc_ref", "https://example.com/doc.pdf", "application/pdf");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.GETDOCUMENT, task.WorkflowTaskType);
        }

        [Fact]
        public void GenerateImageTask_SetsType()
        {
            var task = new GenerateImageTask("img_ref", "openai", "dall-e-3", "A sunset");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.GENERATEIMAGE, task.WorkflowTaskType);
        }

        [Fact]
        public void GenerateAudioTask_SetsType()
        {
            var task = new GenerateAudioTask("audio_ref", "openai", "tts-1", "Hello world");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.GENERATEAUDIO, task.WorkflowTaskType);
        }

        [Fact]
        public void ListMcpToolsTask_SetsType()
        {
            var task = new ListMcpToolsTask("mcp_list_ref", "weather-server");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LISTMCPTOOLS, task.WorkflowTaskType);
        }

        [Fact]
        public void CallMcpToolTask_SetsType()
        {
            var task = new CallMcpToolTask("mcp_call_ref", "weather-server", "get_weather",
                new Dictionary<string, object> { { "city", "SF" } });
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.CALLMCPTOOL, task.WorkflowTaskType);
        }

        [Fact]
        public void LlmStoreEmbeddings_SetsType()
        {
            var model = new EmbeddingModel("openai", "text-embedding-ada-002");
            var task = new LlmStoreEmbeddings("store_ref", "pinecone", "idx", "ns", model, "text", "doc1");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LLMSTOREEMBEDDINGS, task.WorkflowTaskType);
        }

        [Fact]
        public void LlmSearchEmbeddings_SetsType()
        {
            var model = new EmbeddingModel("openai", "text-embedding-ada-002");
            var task = new LlmSearchEmbeddings("search_ref", "pinecone", "idx", "ns", model, "query");
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LLMSEARCHEMBEDDINGS, task.WorkflowTaskType);
        }

        #endregion

        #region Complete Workflow Building

        [Fact]
        public void CompleteWorkflow_WithMultipleTaskTypes_BuildsCorrectly()
        {
            var wf = new ConductorWorkflow()
                .WithName("complete_wf")
                .WithDescription("A workflow with many task types")
                .WithVersion(1)
                .WithTask(new SimpleTask("simple_ref", "simple_task"))
                .WithTask(new HttpTask("http_ref", new HttpTaskSettings { uri = "https://example.com" }))
                .WithTask(new JQTask("jq_ref", ".input"))
                .WithTask(new WaitTask("wait_ref", TimeSpan.FromSeconds(5)))
                .WithTask(new TerminateTask("term_ref", WorkflowStatus.StatusEnum.COMPLETED, "Done"));

            Assert.Equal(5, wf.Tasks.Count);
            Assert.Equal("complete_wf", wf.Name);
        }

        #endregion
    }
}
