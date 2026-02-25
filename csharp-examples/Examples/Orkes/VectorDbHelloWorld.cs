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
using conductor.Examples;
using Conductor.Api;
using Conductor.Client;
using Conductor.Client.Ai;
using Conductor.Client.Extensions;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Conductor.Definition;
using Conductor.Definition.TaskType.LlmTasks;
using Conductor.Definition.TaskType.LlmTasks.Utils;
using Conductor.Executor;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Conductor.Examples.Orkes
{
    [WorkerTask]
    public class VectorDBHelloWorld
    {
        private readonly Conductor.Client.Configuration _configuration;
        private readonly Orchestrator _orchestrator;
        private readonly OpenAIConfig _openAIConfig;
        private readonly WorkflowExecutor _workflowExecutor;
        private readonly WorkflowResourceApi _workflowClient;
        private readonly ILogger logger;
        private const string WorlfowName = "test_vector_db";
        private const string Description = "Test-ai_vectorDB";

        public VectorDBHelloWorld()
        {
            _configuration = new Conductor.Client.Configuration();
            _openAIConfig = new OpenAIConfig();
            _orchestrator = new Orchestrator(_configuration);
            _workflowExecutor = new WorkflowExecutor(_configuration);
            _workflowClient = ApiExtensions.GetClient<WorkflowResourceApi>();

            //For local testing
            //var _orkesApiClient = new OrkesApiClient(_configuration, new OrkesAuthenticationSettings(Constants.KEY_ID, Constants.KEY_SECRET));
            //_workflowClient = _orkesApiClient.GetClient<WorkflowResourceApi>();
        }

        [WorkerTask(taskType: "get_friends_name", batchSize: 5, pollIntervalMs: 200, workerId: "workerId")]
        public string GetFriendName()
        {
            string name = Environment.UserName;
            return (string.IsNullOrEmpty(name.Trim())) ? "anonymous" : name;
        }

        public void VectorDBHelloWoroldTest()
        {
            string vectorDB = ExampleConstants.OPEN_AI_PINECONE + Environment.UserName;
            string llmProvider = ExampleConstants.OPENAITEXT + Environment.UserName;
            string embeddingModel = ExampleConstants.VECTOR_DB_EMBEDDING_MODEL;
            string textCompleteModel = ExampleConstants.VECTOR_DB_TEXT_COMPLETE_MODEL;
            string chatCompleteModel = ExampleConstants.OPENAIGPT;
            var openAIConfig = new OpenAIConfig();

            _orchestrator.AddVectorStore(dbIntegrationName: vectorDB, provider: Conductor.Client.Ai.Configuration.VectorDBEnum.PINECONE_DB,
                indices: new List<string> { "hello_world" }, description: "pinecone db", config: new PineconeConfig());

            var promptName = ExampleConstants.VECTOR_DB_PROMPT_NAME;
            var promptText = ExampleConstants.VECTOR_DB_PROMPT_TEXT;

            _orchestrator.AddPromptTemplate(name: promptName, promptTemplate: promptText, description: ExampleConstants.VECTOR_DB_PROMPT_NAME);
            _orchestrator.AssociatePromptTemplate(PromptName: promptName, integrationProvider: llmProvider, aiModels: new List<string> { textCompleteModel });

            ConductorWorkflow workflow = new ConductorWorkflow()
                .WithName(WorlfowName)
                .WithDescription(Description)
                .WithVersion(1);
            _workflowExecutor.RegisterWorkflow(workflow, true);

            var indexText = new LlmIndexText(taskReferenceName: ExampleConstants.VECTOR_DB_INDEX_REFNAME, vectorDB: vectorDB, index: ExampleConstants.VECTOR_DB_INDEX, nameSpace: ExampleConstants.HELLO, text: ExampleConstants.VECTOR_DB_TEXT,
                embeddingModel: new EmbeddingModel(provider: llmProvider, model: embeddingModel), docid: ExampleConstants.DOCID,
                metaData: new Dictionary<string, object>()
                {
                    {
                        "doctype","testing only"
                    }

                });

            EmbeddingModel model = new EmbeddingModel(provider: llmProvider, model: embeddingModel);
            var indexDocument = new LlmIndexDocuments(taskReferenceName: ExampleConstants.VECTOR_DB_INDEX_REFNAME, vectorDB: vectorDB, index: ExampleConstants.VECTOR_DB_INDEX, nameSpace: ExampleConstants.VECTOR_DB_US_NAMESPACE,
                embeddingModel: model,
                embeddingModelProvider: model.Provider,
                url: "https://constitutioncenter.org/media/files/constitution.pdf",
                mediaType: "application/pdf",
                docId: ExampleConstants.VECTOR_DB_US_NAMESPACE,
                metaData:
                new Dictionary<string, object>()
                {
                    {
                        "doc_url","https://constitutioncenter.org/media/files/constitution.pdf"
                    }
                });

            var generateEmbeddings = new LlmGenerateEmbeddings(taskReferenceName: ExampleConstants.LLM_GENERATE_EMBEDDINGS_TASKREF, llmProvider: llmProvider, model: embeddingModel, text: "xxxxxxxx");
            string outputString = generateEmbeddings.Output("result[0]");
            List<int> embeddings = new List<int>();
            if (int.TryParse(outputString, out int outputInt))
            {
                embeddings.Add(outputInt);
            }

            var queryIndex = new LlmQueryEmbeddings(taskReferenceName: ExampleConstants.LLM_QUERY_EMBEDDINGS_TASKREF, vectorDB: vectorDB, index: ExampleConstants.VECTOR_DB_INDEX, nameSpace: ExampleConstants.VECTOR_DB_US_NAMESPACE, embeddings: embeddings);

            var searchIndex = new LlmSearchIndex(taskReferenceName: ExampleConstants.LLM_SEARCH_INDEX_TASKREF, vectorDB: vectorDB, nameSpace: ExampleConstants.VECTOR_DB_US_NAMESPACE, index: ExampleConstants.VECTOR_DB_INDEX, embeddingModel: embeddingModel,
                embeddingModelProvider: llmProvider, query: ExampleConstants.VECTOR_DB_QUESTION, MaxResults: 2);

            var textComplete = new LlmTextComplete(taskRefName: ExampleConstants.VECTOR_DB_PROMPT_NAME, llmProvider: llmProvider, model: textCompleteModel, promptName: promptName);

            var chatComplete = new LlmChatComplete(taskReferenceName: ExampleConstants.CHATCOMPLETEREF, llmProvider: llmProvider, model: chatCompleteModel, instructionsTemplate: promptName,
                messages: new List<ChatMessage> { new ChatMessage(role: "user", message: ExampleConstants.VECTOR_DB_QUESTION) });

            chatComplete.PromptVariable(variable: "text", value: searchIndex.Output("result..text"));
            chatComplete.PromptVariable("question", ExampleConstants.VECTOR_DB_QUESTION);

            textComplete.PromptVariable(variable: "text", value: searchIndex.Output("result..text"));
            textComplete.PromptVariable("question", ExampleConstants.VECTOR_DB_QUESTION);

            workflow.WithTask(searchIndex);
            workflow.WithTask(chatComplete);

            _workflowExecutor.RegisterWorkflow(workflow, true);

            var waitHandle = new ManualResetEvent(false);
            var backgroundTask = System.Threading.Tasks.Task.Run(async () => await Utils.WorkerUtil.StartBackGroundTask(waitHandle));
            waitHandle.WaitOne();
            StartWorkflowRequest startWorkflow = new StartWorkflowRequest()
            {
                Name = workflow.Name,
                Version = workflow.Version,
                WorkflowDef = workflow,
                CreatedBy = Constants.OWNER_EMAIL
            };

            var workflowRun = _workflowClient.ExecuteWorkflow(startWorkflow, "1234", startWorkflow.Name, 1);
            logger.LogInformation($"{ExampleConstants.OPENAI_LOGMESSAGE} {workflowRun.Output["result"]}\n\n");
        }
    }
}