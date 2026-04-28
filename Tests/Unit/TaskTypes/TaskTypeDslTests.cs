using Conductor.Client.Models;
using Conductor.Definition.TaskType;
using Conductor.Definition.TaskType.LlmTasks;
using Conductor.Definition.TaskType.LlmTasks.Utils;
using System.Collections.Generic;
using Xunit;

namespace Tests.Unit.TaskTypes
{
    public class TaskTypeDslTests
    {
        [Fact]
        public void HttpPollTask_SetsCorrectTypeAndInputs()
        {
            var settings = new HttpTaskSettings
            {
                uri = "https://example.com/api/status"
            };
            var task = new HttpPollTask("http_poll_ref", settings);

            Assert.Equal("http_poll_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.HTTPPOLL, task.WorkflowTaskType);
            Assert.NotNull(task.InputParameters);
            Assert.True(task.InputParameters.ContainsKey("http_request"));
        }

        [Fact]
        public void KafkaPublishTask_SetsCorrectTypeAndInputs()
        {
            var task = new KafkaPublishTask("kafka_ref", "bootstrap:9092", "my-topic", "test-value");

            Assert.Equal("kafka_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.KAFKAPUBLISH, task.WorkflowTaskType);
            Assert.True(task.InputParameters.ContainsKey("kafka_request"));
            var kafkaRequest = task.InputParameters["kafka_request"] as Dictionary<string, object>;
            Assert.NotNull(kafkaRequest);
            Assert.Equal("my-topic", kafkaRequest["topic"]);
            Assert.Equal("bootstrap:9092", kafkaRequest["bootStrapServers"]);
            Assert.Equal("test-value", kafkaRequest["value"]);
        }

        [Fact]
        public void KafkaPublishTask_WithOptionalKey()
        {
            var task = new KafkaPublishTask("kafka_ref", "bootstrap:9092", "my-topic", "test-value", key: "my-key");

            var kafkaRequest = task.InputParameters["kafka_request"] as Dictionary<string, object>;
            Assert.NotNull(kafkaRequest);
            Assert.Equal("my-key", kafkaRequest["key"]);
        }

        [Fact]
        public void KafkaPublishTask_WithOptionalHeaders()
        {
            var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            var task = new KafkaPublishTask("kafka_ref", "bootstrap:9092", "my-topic", "test-value", headers: headers);

            var kafkaRequest = task.InputParameters["kafka_request"] as Dictionary<string, object>;
            Assert.NotNull(kafkaRequest);
            Assert.Equal(headers, kafkaRequest["headers"]);
        }

        [Fact]
        public void StartWorkflowTask_SetsCorrectTypeAndInputs()
        {
            var task = new StartWorkflowTask("start_wf_ref", "child_workflow", 1);

            Assert.Equal("start_wf_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.STARTWORKFLOW, task.WorkflowTaskType);
            Assert.True(task.InputParameters.ContainsKey("startWorkflow"));
            var startWf = task.InputParameters["startWorkflow"] as Dictionary<string, object>;
            Assert.NotNull(startWf);
            Assert.Equal("child_workflow", startWf["name"]);
            Assert.Equal(1, startWf["version"]);
        }

        [Fact]
        public void StartWorkflowTask_WithInputAndCorrelationId()
        {
            var input = new Dictionary<string, object> { { "key1", "value1" } };
            var task = new StartWorkflowTask("start_wf_ref", "child_workflow", 1, input: input, correlationId: "corr-123");

            var startWf = task.InputParameters["startWorkflow"] as Dictionary<string, object>;
            Assert.NotNull(startWf);
            Assert.Equal(input, startWf["input"]);
            Assert.Equal("corr-123", startWf["correlationId"]);
        }

        [Fact]
        public void InlineTask_SetsCorrectTypeAndInputs()
        {
            var task = new InlineTask("inline_ref", "function() { return true; }");

            Assert.Equal("inline_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.INLINE, task.WorkflowTaskType);
            Assert.True(task.InputParameters.ContainsKey("expression"));
            Assert.True(task.InputParameters.ContainsKey("evaluatorType"));
            Assert.Equal("function() { return true; }", task.InputParameters["expression"]);
            Assert.Equal("javascript", task.InputParameters["evaluatorType"]);
        }

        [Fact]
        public void InlineTask_WithCustomEvaluator()
        {
            var task = new InlineTask("inline_ref", "1 + 1", "python");

            Assert.Equal("python", task.InputParameters["evaluatorType"]);
        }

        [Fact]
        public void LlmStoreEmbeddings_SetsCorrectTypeAndInputs()
        {
            var embeddingModel = new EmbeddingModel("openai", "text-embedding-ada-002");
            var task = new LlmStoreEmbeddings("store_emb_ref", "pinecone", "my-index", "my-ns", embeddingModel, "test text", "doc-1");

            Assert.Equal("store_emb_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LLMSTOREEMBEDDINGS, task.WorkflowTaskType);
            Assert.Equal("pinecone", task.InputParameters["vectorDB"]);
            Assert.Equal("my-index", task.InputParameters["index"]);
            Assert.Equal("my-ns", task.InputParameters["nameSpace"]);
            Assert.Equal("openai", task.InputParameters["embeddingModelProvider"]);
            Assert.Equal("text-embedding-ada-002", task.InputParameters["embeddingModel"]);
            Assert.Equal("test text", task.InputParameters["text"]);
            Assert.Equal("doc-1", task.InputParameters["docId"]);
        }

        [Fact]
        public void LlmSearchEmbeddings_SetsCorrectTypeAndInputs()
        {
            var embeddingModel = new EmbeddingModel("openai", "text-embedding-ada-002");
            var task = new LlmSearchEmbeddings("search_emb_ref", "pinecone", "my-index", "my-ns", embeddingModel, "search query", 10);

            Assert.Equal("search_emb_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LLMSEARCHEMBEDDINGS, task.WorkflowTaskType);
            Assert.Equal("pinecone", task.InputParameters["vectorDB"]);
            Assert.Equal("search query", task.InputParameters["query"]);
            Assert.Equal(10, task.InputParameters["maxResults"]);
        }

        [Fact]
        public void LlmSearchEmbeddings_DefaultMaxResults()
        {
            var embeddingModel = new EmbeddingModel("openai", "text-embedding-ada-002");
            var task = new LlmSearchEmbeddings("search_emb_ref", "pinecone", "my-index", "my-ns", embeddingModel, "search query");

            Assert.Equal(5, task.InputParameters["maxResults"]);
        }

        [Fact]
        public void GetDocumentTask_SetsCorrectTypeAndInputs()
        {
            var task = new GetDocumentTask("get_doc_ref", "https://example.com/doc.pdf");

            Assert.Equal("get_doc_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.GETDOCUMENT, task.WorkflowTaskType);
            Assert.Equal("https://example.com/doc.pdf", task.InputParameters["url"]);
            Assert.Equal("application/pdf", task.InputParameters["mediaType"]);
        }

        [Fact]
        public void GetDocumentTask_CustomMediaType()
        {
            var task = new GetDocumentTask("get_doc_ref", "https://example.com/doc.html", "text/html");

            Assert.Equal("text/html", task.InputParameters["mediaType"]);
        }

        [Fact]
        public void GenerateImageTask_SetsCorrectTypeAndInputs()
        {
            var task = new GenerateImageTask("gen_img_ref", "openai", "dall-e-3", "a cat wearing a hat");

            Assert.Equal("gen_img_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.GENERATEIMAGE, task.WorkflowTaskType);
            Assert.Equal("openai", task.InputParameters["llmProvider"]);
            Assert.Equal("dall-e-3", task.InputParameters["model"]);
            Assert.Equal("a cat wearing a hat", task.InputParameters["prompt"]);
        }

        [Fact]
        public void GenerateImageTask_WithOptionalParams()
        {
            var task = new GenerateImageTask("gen_img_ref", "openai", "dall-e-3", "a prompt", size: "1024x1024", count: 2);

            Assert.Equal("1024x1024", task.InputParameters["size"]);
            Assert.Equal(2, task.InputParameters["count"]);
        }

        [Fact]
        public void GenerateAudioTask_SetsCorrectTypeAndInputs()
        {
            var task = new GenerateAudioTask("gen_audio_ref", "openai", "tts-1", "Hello world");

            Assert.Equal("gen_audio_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.GENERATEAUDIO, task.WorkflowTaskType);
            Assert.Equal("openai", task.InputParameters["llmProvider"]);
            Assert.Equal("tts-1", task.InputParameters["model"]);
            Assert.Equal("Hello world", task.InputParameters["text"]);
        }

        [Fact]
        public void GenerateAudioTask_WithOptionalVoice()
        {
            var task = new GenerateAudioTask("gen_audio_ref", "openai", "tts-1", "text", voice: "alloy");

            Assert.Equal("alloy", task.InputParameters["voice"]);
        }

        [Fact]
        public void ListMcpToolsTask_SetsCorrectTypeAndInputs()
        {
            var task = new ListMcpToolsTask("list_mcp_ref", "weather-server");

            Assert.Equal("list_mcp_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.LISTMCPTOOLS, task.WorkflowTaskType);
            Assert.Equal("weather-server", task.InputParameters["mcpServerName"]);
        }

        [Fact]
        public void CallMcpToolTask_SetsCorrectTypeAndInputs()
        {
            var toolInput = new Dictionary<string, object> { { "city", "San Francisco" } };
            var task = new CallMcpToolTask("call_mcp_ref", "weather-server", "get_weather", toolInput);

            Assert.Equal("call_mcp_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.CALLMCPTOOL, task.WorkflowTaskType);
            Assert.Equal("weather-server", task.InputParameters["mcpServerName"]);
            Assert.Equal("get_weather", task.InputParameters["toolName"]);
            Assert.Equal(toolInput, task.InputParameters["toolInput"]);
        }

        [Fact]
        public void CallMcpToolTask_WithoutToolInput()
        {
            var task = new CallMcpToolTask("call_mcp_ref", "weather-server", "list_cities");

            Assert.False(task.InputParameters.ContainsKey("toolInput"));
        }

        [Fact]
        public void ToolCallTask_SetsCorrectTypeAndInputs()
        {
            var toolInput = new Dictionary<string, object> { { "param1", "value1" } };
            var task = new ToolCallTask("tool_call_ref", "my_tool", toolInput);

            Assert.Equal("tool_call_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.TOOLCALL, task.WorkflowTaskType);
            Assert.Equal("my_tool", task.InputParameters["toolName"]);
            Assert.Equal(toolInput, task.InputParameters["toolInput"]);
        }

        [Fact]
        public void ToolSpecTask_SetsCorrectTypeAndInputs()
        {
            var inputSchema = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "name", new Dictionary<string, object> { { "type", "string" } } }
                    }
                }
            };
            var task = new ToolSpecTask("tool_spec_ref", "my_tool", "A test tool", inputSchema);

            Assert.Equal("tool_spec_ref", task.TaskReferenceName);
            Assert.Equal(WorkflowTask.WorkflowTaskTypeEnum.TOOLSPEC, task.WorkflowTaskType);
            Assert.Equal("my_tool", task.InputParameters["name"]);
            Assert.Equal("A test tool", task.InputParameters["description"]);
            Assert.Equal(inputSchema, task.InputParameters["inputSchema"]);
        }

        [Fact]
        public void ToolSpecTask_WithoutInputSchema()
        {
            var task = new ToolSpecTask("tool_spec_ref", "my_tool", "A test tool");

            Assert.False(task.InputParameters.ContainsKey("inputSchema"));
        }

        // Phase 1 regression tests for enum values
        [Fact]
        public void WorkflowTaskTypeEnum_HasAllNewValues()
        {
            Assert.Equal(31, (int)WorkflowTask.WorkflowTaskTypeEnum.HTTPPOLL);
            Assert.Equal(32, (int)WorkflowTask.WorkflowTaskTypeEnum.LLMSTOREEMBEDDINGS);
            Assert.Equal(33, (int)WorkflowTask.WorkflowTaskTypeEnum.LLMSEARCHEMBEDDINGS);
            Assert.Equal(34, (int)WorkflowTask.WorkflowTaskTypeEnum.GETDOCUMENT);
            Assert.Equal(35, (int)WorkflowTask.WorkflowTaskTypeEnum.GENERATEIMAGE);
            Assert.Equal(36, (int)WorkflowTask.WorkflowTaskTypeEnum.GENERATEAUDIO);
            Assert.Equal(37, (int)WorkflowTask.WorkflowTaskTypeEnum.LISTMCPTOOLS);
            Assert.Equal(38, (int)WorkflowTask.WorkflowTaskTypeEnum.CALLMCPTOOL);
            Assert.Equal(39, (int)WorkflowTask.WorkflowTaskTypeEnum.TOOLCALL);
            Assert.Equal(40, (int)WorkflowTask.WorkflowTaskTypeEnum.TOOLSPEC);
        }
    }
}
