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
using Conductor.Client.Models;
using System.Collections.Generic;

namespace Conductor.Definition.TaskType.LlmTasks
{
    /// <summary>
    /// ChatMessage
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets or Sets Role
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or Sets Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage" /> class
        /// </summary>
        /// <param name="role"></param>
        /// <param name="message"></param>
        public ChatMessage(string role, string message)
        {
            Role = role;
            Message = message;
        }
    }

    /// <summary>
    /// LlmChatComplete
    /// </summary>
    public class LlmChatComplete : Task
    {
        /// <summary>
        /// Gets or Sets TaskRefName
        /// </summary>
        public string TaskRefName { get; set; }

        /// <summary>
        /// Gets or Sets LlmProvider
        /// </summary>
        public string LlmProvider { get; set; }

        /// <summary>
        /// Gets or Sets Model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or Sets Messages
        /// </summary>
        public List<ChatMessage> Messages { get; set; }

        /// <summary>
        /// Gets or Sets StopWords
        /// </summary>
        public List<string> StopWords { get; set; }

        /// <summary>
        /// Gets or Sets MaxTokens
        /// </summary>
        public int MaxTokens { get; set; }

        /// <summary>
        /// Gets or Sets Temperature
        /// </summary>
        public int Temperature { get; set; }

        /// <summary>
        /// Gets or Sets TopP
        /// </summary>
        public int TopP { get; set; }

        /// <summary>
        /// Gets or Sets InstructionTemplate
        /// </summary>
        public string InstructionsTemplate { get; set; }

        /// <summary>
        /// Gets or Sets TemplateVariables
        /// </summary>
        public Dictionary<string, object> TemplateVariables { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmChatComplete" /> class
        /// </summary>
        /// <param name="taskReferenceName"></param>
        /// <param name="llmProvider"></param>
        /// <param name="model"></param>
        /// <param name="messages"></param>
        /// <param name="stopWords"></param>
        /// <param name="maxTokens"></param>
        /// <param name="temperature"></param>
        /// <param name="topP"></param>
        /// <param name="instructionsTemplate"></param>
        /// <param name="templateVariables"></param>
        public LlmChatComplete(string taskReferenceName, string llmProvider, string model, List<ChatMessage> messages,
            List<string> stopWords = null, int maxTokens = 100, int temperature = 0, int topP = 1,
            string instructionsTemplate = null, Dictionary<string, object> templateVariables = null) : base(taskReferenceName, WorkflowTask.WorkflowTaskTypeEnum.LLMCHATCOMPLETE)
        {
            TaskRefName = taskReferenceName;
            LlmProvider = llmProvider;
            Model = model;
            Messages = messages;
            StopWords = stopWords ?? new List<string>();
            MaxTokens = maxTokens;
            Temperature = temperature;
            TopP = topP;
            InstructionsTemplate = instructionsTemplate;
            TemplateVariables = templateVariables ?? new Dictionary<string, object>();

            InitializeInputs();
        }

        /// <summary>
        /// Adding PromptVariables to InputParams
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public LlmChatComplete PromptVariables(Dictionary<string, object> variables)
        {
            foreach (var variable in variables)
            {
                TemplateVariables[variable.Key] = variable.Value;
            }
            WithInput(Constants.PROMPTVARIABLES, TemplateVariables);
            return this;
        }

        /// <summary>
        /// Adding PromptVariable to InputParams
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public LlmChatComplete PromptVariable(string variable, object value)
        {
            TemplateVariables[variable] = value;
            WithInput(Constants.PROMPTVARIABLES, TemplateVariables);
            return this;
        }

        /// <summary>
        /// Populates inputParams dictionary with LlmChatComplete attributes.
        /// </summary>
        private void InitializeInputs()
        {
            WithInput(Constants.LLMPROVIDER, LlmProvider);
            WithInput(Constants.MODEL, Model);
            WithInput(Constants.PROMPTVARIABLES, TemplateVariables);
            WithInput(Constants.TEMPERATURE, Temperature);
            WithInput(Constants.TOPP, TopP);
            WithInput(Constants.INSTRUCTIONTEMPLATE, InstructionsTemplate);
            WithInput(Constants.MESSAGES, Messages);
            WithInput(Constants.MAXTOKENS, MaxTokens);
            WithInput(Constants.STOPWORDS, StopWords);
            WithInput(Constants.PROMPTVARIABLES, TemplateVariables);
        }
    }
}