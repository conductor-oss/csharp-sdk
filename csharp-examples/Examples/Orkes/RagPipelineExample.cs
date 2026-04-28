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
using Conductor.Definition.TaskType.LlmTasks.Utils;
using Conductor.Executor;
using System;
using System.Collections.Generic;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates a complete RAG (Retrieval-Augmented Generation) pipeline:
    /// 1. Get document from URL
    /// 2. Index document into vector DB with embeddings
    /// 3. Search embeddings based on user query
    /// 4. Generate response using retrieved context
    /// </summary>
    public class RagPipelineExample
    {
        private readonly WorkflowExecutor _workflowExecutor;

        public RagPipelineExample()
        {
            _workflowExecutor = ApiExtensions.GetWorkflowExecutor();
        }

        /// <summary>
        /// Creates the ingestion workflow that fetches a document and indexes it.
        /// </summary>
        public ConductorWorkflow CreateIngestionWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("rag_ingest_csharp")
                .WithDescription("RAG Pipeline - Document Ingestion")
                .WithVersion(1);

            var embeddingModel = new EmbeddingModel("openai", "text-embedding-ada-002");

            // Step 1: Fetch the document
            var getDoc = new GetDocumentTask("get_doc_ref", "${workflow.input.doc_url}", "${workflow.input.media_type}");

            // Step 2: Store embeddings into vector DB
            var storeEmbeddings = new LlmStoreEmbeddings(
                taskReferenceName: "store_emb_ref",
                vectorDB: "${workflow.input.vector_db}",
                index: "${workflow.input.index}",
                nameSpace: "${workflow.input.namespace}",
                embeddingModel: embeddingModel,
                text: "${get_doc_ref.output.result}",
                docId: "${workflow.input.doc_id}"
            );

            workflow
                .WithTask(getDoc)
                .WithTask(storeEmbeddings);

            return workflow;
        }

        /// <summary>
        /// Creates the query workflow that searches embeddings and generates a response.
        /// </summary>
        public ConductorWorkflow CreateQueryWorkflow()
        {
            var workflow = new ConductorWorkflow()
                .WithName("rag_query_csharp")
                .WithDescription("RAG Pipeline - Query and Generate")
                .WithVersion(1);

            var embeddingModel = new EmbeddingModel("openai", "text-embedding-ada-002");

            // Step 1: Search embeddings for relevant context
            var searchEmbeddings = new LlmSearchEmbeddings(
                taskReferenceName: "search_emb_ref",
                vectorDB: "${workflow.input.vector_db}",
                index: "${workflow.input.index}",
                nameSpace: "${workflow.input.namespace}",
                embeddingModel: embeddingModel,
                query: "${workflow.input.query}",
                maxResults: 5
            );

            // Step 2: Generate response using retrieved context + user query
            var chatMessages = new List<ChatMessage>
            {
                new ChatMessage("system", "Answer the user's question using only the provided context. If the context doesn't contain relevant information, say so.\n\nContext:\n${search_emb_ref.output.result}"),
                new ChatMessage("user", "${workflow.input.query}")
            };
            var generateResponse = new LlmChatComplete(
                taskReferenceName: "generate_ref",
                llmProvider: "${workflow.input.llm_provider}",
                model: "${workflow.input.llm_model}",
                messages: chatMessages,
                maxTokens: 500,
                temperature: 0
            );

            workflow
                .WithTask(searchEmbeddings)
                .WithTask(generateResponse);

            return workflow;
        }

        public void RunIngestion()
        {
            var workflow = CreateIngestionWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);

            var input = new Dictionary<string, object>
            {
                { "doc_url", "https://example.com/docs/getting-started.pdf" },
                { "media_type", "application/pdf" },
                { "vector_db", "pineconedb" },
                { "index", "my-docs" },
                { "namespace", "getting-started" },
                { "doc_id", "getting-started-v1" }
            };

            Console.WriteLine("Starting RAG ingestion...");
            var workflowId = _workflowExecutor.StartWorkflow(new StartWorkflowRequest(name: workflow.Name, version: workflow.Version, input: input));
            Console.WriteLine($"Ingestion started. WorkflowId: {workflowId}");
        }

        public void RunQuery()
        {
            var workflow = CreateQueryWorkflow();
            _workflowExecutor.RegisterWorkflow(workflow, true);

            var input = new Dictionary<string, object>
            {
                { "vector_db", "pineconedb" },
                { "index", "my-docs" },
                { "namespace", "getting-started" },
                { "query", "How do I create my first workflow?" },
                { "llm_provider", "openai" },
                { "llm_model", "gpt-4" }
            };

            Console.WriteLine("Starting RAG query...");
            var workflowId = _workflowExecutor.StartWorkflow(new StartWorkflowRequest(name: workflow.Name, version: workflow.Version, input: input));
            Console.WriteLine($"Query started. WorkflowId: {workflowId}");
        }
    }
}
