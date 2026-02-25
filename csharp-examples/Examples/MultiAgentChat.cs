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
    /// Demonstrates a multi-agent chat workflow where two LLM agents collaborate:
    /// 1. Research Agent gathers information
    /// 2. Writing Agent composes a response based on research
    /// 3. Review Agent checks quality
    /// </summary>
    public class MultiAgentChat
    {
        private readonly WorkflowExecutor _workflowExecutor;

        public MultiAgentChat()
        {
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        public ConductorWorkflow CreateMultiAgentWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("multi_agent_chat_csharp")
                .WithDescription("Multi-agent collaboration workflow")
                .WithVersion(1);

            // Agent 1: Research Agent - gathers information about the topic
            var researchMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are a research assistant. Gather key facts and information about the given topic. Be thorough but concise. Output structured bullet points."),
                new ChatMessage("user", "${workflow.input.topic}")
            };
            var researchAgent = new LlmChatComplete(
                taskReferenceName: "research_agent_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: researchMessages,
                maxTokens: 500,
                temperature: 0
            );

            // Agent 2: Writing Agent - composes a response using research
            var writingMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are an expert writer. Using the research provided, compose a well-structured, engaging response. Use clear headings and paragraphs."),
                new ChatMessage("user", "Topic: ${workflow.input.topic}\n\nResearch:\n${research_agent_ref.output.result}")
            };
            var writingAgent = new LlmChatComplete(
                taskReferenceName: "writing_agent_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: writingMessages,
                maxTokens: 1000,
                temperature: 1
            );

            // Agent 3: Review Agent - checks quality and provides final output
            var reviewMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are a quality reviewer. Review the draft for accuracy, clarity, and completeness. If the draft is good, output it as-is. If it needs improvements, make them and output the improved version."),
                new ChatMessage("user", "Draft to review:\n${writing_agent_ref.output.result}")
            };
            var reviewAgent = new LlmChatComplete(
                taskReferenceName: "review_agent_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: reviewMessages,
                maxTokens: 1000,
                temperature: 0
            );

            workflow
                .WithTask(researchAgent)
                .WithTask(writingAgent)
                .WithTask(reviewAgent);

            return workflow;
        }

        public void RegisterAndRun()
        {
            var workflow = CreateMultiAgentWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);

            var input = new Dictionary<string, object>
            {
                { "llm_provider", "openai" },
                { "llm_model", "gpt-4" },
                { "topic", "The impact of workflow orchestration on modern microservices architecture" }
            };

            Console.WriteLine("Starting Multi-Agent Chat workflow...");
            var workflowId = _workflowExecutor.StartWorkflow(
                new StartWorkflowRequest(name: workflow.Name, version: workflow.Version, input: input));
            Console.WriteLine($"Multi-Agent workflow started. WorkflowId: {workflowId}");
        }
    }
}
