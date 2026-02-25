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
    /// Demonstrates a human-in-the-loop chat workflow:
    /// 1. User sends a question
    /// 2. LLM generates a response
    /// 3. Human reviews and approves/modifies
    /// 4. Loop continues with the next user question
    /// </summary>
    public class HumanInLoopChat
    {
        private readonly WorkflowExecutor _workflowExecutor;

        public HumanInLoopChat()
        {
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        public ConductorWorkflow CreateHumanInLoopChatWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("human_in_loop_chat_csharp")
                .WithDescription("Chat workflow with human review step")
                .WithVersion(1);

            // Step 1: LLM generates a response to the user's question
            var chatMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are a helpful assistant. Respond concisely."),
                new ChatMessage("user", "${workflow.input.user_question}")
            };
            var llmChat = new LlmChatComplete(
                taskReferenceName: "llm_chat_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: chatMessages
            );

            // Step 2: Human task - reviewer approves or modifies the LLM response
            var humanReview = new HumanTask(
                taskRefName: "human_review_ref",
                displayName: "Review AI Response",
                formTemplate: "ai_response_review",
                formVersion: 1,
                assignmentCompletionStrategy: HumanTask.AssignmentCompletionStrategyEnum.LEAVE_OPEN
            );
            humanReview.WithInput("ai_response", "${llm_chat_ref.output.result}");
            humanReview.WithInput("original_question", "${workflow.input.user_question}");

            // Step 3: Format final response based on human review outcome
            var finalMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "Format the following reviewed response for the user."),
                new ChatMessage("user", "Original AI response: ${llm_chat_ref.output.result}\nHuman feedback: ${human_review_ref.output.feedback}")
            };
            var formatResponse = new LlmChatComplete(
                taskReferenceName: "format_response_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: finalMessages
            );

            workflow
                .WithTask(llmChat)
                .WithTask(humanReview)
                .WithTask(formatResponse);

            return workflow;
        }

        public void RegisterAndRun()
        {
            var workflow = CreateHumanInLoopChatWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);

            var input = new Dictionary<string, object>
            {
                { "llm_provider", "openai" },
                { "llm_model", "gpt-4" },
                { "user_question", "Explain the benefits of microservices architecture." }
            };

            Console.WriteLine("Starting Human-in-the-Loop Chat workflow...");
            var workflowId = _workflowExecutor.StartWorkflow(
                new StartWorkflowRequest(name: workflow.Name, version: workflow.Version, input: input));
            Console.WriteLine($"Workflow started. WorkflowId: {workflowId}");
            Console.WriteLine("The workflow will pause at the Human Task for review.");
        }
    }
}
