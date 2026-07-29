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
// RAG Agent — vector search + document indexing.
//
// Demonstrates:
//   - RagTools.Index() to populate a vector database with documents
//   - RagTools.Search() to query the indexed documents
//
// Requirements:
//   - Conductor server with RAG system tasks enabled
//   - A configured vector database (e.g., pgvector)
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api in environment
//   - CONDUCTOR_AGENT_LLM_MODEL set in environment

using Conductor.AI;
using Conductor.AI.Examples;

const string VectorDb = "pgvectordb";
const string IndexName = "conductor_docs_56";
const string EmbedProvider = "openai";
const string EmbedModel = "text-embedding-3-small";

// ── Knowledge base documents ──────────────────────────────────

var documents = new[]
{
    ("auth-guide",    "API Authentication Guide. To authenticate API requests, include an Authorization header with a Bearer token. Tokens expire after 30 days and must be rotated."),
    ("workflow-tasks","Workflow Task Types. Conductor supports: SIMPLE, HTTP, INLINE, SUB_WORKFLOW, FORK_JOIN_DYNAMIC, SWITCH, and WAIT task types."),
    ("error-handling","Error Handling and Retries. Set retryCount for retry attempts. retryLogic can be FIXED, EXPONENTIAL_BACKOFF, or LINEAR_BACKOFF."),
    ("agent-config",  "Agent Configuration. Agents are defined with a name, model, instructions, and tools. The model field uses 'provider/model_name' format."),
};

// ── RAG tools (server-side, no worker needed) ─────────────────

var indexTool = RagTools.Index(
    name: "index_document",
    description: "Index a document into the knowledge base.",
    vectorDb: VectorDb,
    index: IndexName,
    embeddingModelProvider: EmbedProvider,
    embeddingModel: EmbedModel);

var searchTool = RagTools.Search(
    name: "search_knowledge_base",
    description: "Search the knowledge base for relevant documents.",
    vectorDb: VectorDb,
    index: IndexName,
    embeddingModelProvider: EmbedProvider,
    embeddingModel: EmbedModel,
    maxResults: 3);

// ── Indexer agent ──────────────────────────────────────────────

var indexer = new Agent("doc_indexer_56")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a document indexer. Index each document given to you " +
        "using the index_document tool. Index ALL provided documents.",
    Tools = [indexTool],
};

// ── Q&A agent ─────────────────────────────────────────────────

var qaAgent = new Agent("doc_qa_56")
{
    Model = Settings.LlmModel,
    Instructions =
        "You are a documentation assistant. Use search_knowledge_base to " +
        "find relevant information and answer the user's question.",
    Tools = [searchTool],
};

// ── Pipeline: index → answer ───────────────────────────────────

var pipeline = indexer >> qaAgent;

// ── Run ───────────────────────────────────────────────────────

await using var runtime = new AgentRuntime();

// First index all documents, then search
var docsText = string.Join("\n", documents.Select((d, i) => $"Document {i + 1} — ID: {d.Item1}\n{d.Item2}"));

var result = await runtime.RunAsync(
    pipeline,
    $"First, index these documents:\n{docsText}\n\n" +
    "Then answer: How do I authenticate API requests?");

result.PrintResult();
