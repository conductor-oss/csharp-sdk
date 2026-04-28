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
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Orkes;
using System;
using System.Collections.Generic;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates prompt template operations: save, get, test, delete, tagging.
    /// </summary>
    public class PromptJourney
    {
        private readonly IPromptClient _promptClient;
        private const string TEST_PROMPT = "csharp_test_prompt";
        private const string INTEGRATION_NAME = "openai";
        private const string MODEL_NAME = "gpt-4";

        public PromptJourney()
        {
            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            _promptClient = clients.GetPromptClient();
        }

        public void Run()
        {
            Console.WriteLine("=== Prompt Journey ===\n");

            // 1. Save a prompt template
            Console.WriteLine("1. Saving prompt template...");
            var promptTemplate = "You are a helpful assistant. The user's name is ${name}. Please help them with: ${question}";
            _promptClient.SaveMessageTemplate(TEST_PROMPT, promptTemplate, "A test prompt template for C# SDK");
            Console.WriteLine($"   Prompt '{TEST_PROMPT}' saved.");

            // 2. Get the prompt template
            Console.WriteLine("2. Getting prompt template...");
            var fetched = _promptClient.GetMessageTemplate(TEST_PROMPT);
            Console.WriteLine($"   Template: {fetched?.Template}");

            // 3. Get all prompt templates
            Console.WriteLine("3. Getting all prompt templates...");
            var allPrompts = _promptClient.GetMessageTemplates();
            Console.WriteLine($"   Total prompts: {allPrompts?.Count ?? 0}");

            // 4. Test the prompt template
            Console.WriteLine("4. Testing prompt template...");
            var testRequest = new PromptTemplateTestRequest(
                prompt: promptTemplate,
                llmProvider: INTEGRATION_NAME,
                model: MODEL_NAME,
                promptVariables: new Dictionary<string, object>
                {
                    { "name", "Alice" },
                    { "question", "How do I use Conductor workflows?" }
                }
            );
            var testResult = _promptClient.TestMessageTemplate(testRequest);
            Console.WriteLine($"   Test result: {testResult}");

            // 5. Delete the prompt template
            Console.WriteLine("5. Deleting prompt template...");
            _promptClient.DeleteMessageTemplate(TEST_PROMPT);
            Console.WriteLine($"   Prompt '{TEST_PROMPT}' deleted.");

            Console.WriteLine("\nPrompt Journey completed!");
        }
    }
}
