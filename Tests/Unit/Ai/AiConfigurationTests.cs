using Conductor.Client.Ai;
using System.Runtime.Serialization;
using Xunit;
using AiConfig = Conductor.Client.Ai.Configuration;

namespace Tests.Unit.Ai
{
    public class AiConfigurationTests
    {
        // LLM Provider enum tests
        [Fact]
        public void LLMProviderEnum_HasAllProviders()
        {
            Assert.Equal(1, (int)AiConfig.LLMProviderEnum.AZURE_OPEN_AI);
            Assert.Equal(2, (int)AiConfig.LLMProviderEnum.OPEN_AI);
            Assert.Equal(3, (int)AiConfig.LLMProviderEnum.GCP_VERTEX_AI);
            Assert.Equal(4, (int)AiConfig.LLMProviderEnum.HUGGING_FACE);
            Assert.Equal(5, (int)AiConfig.LLMProviderEnum.ANTHROPIC);
            Assert.Equal(6, (int)AiConfig.LLMProviderEnum.AWS_BEDROCK);
            Assert.Equal(7, (int)AiConfig.LLMProviderEnum.COHERE);
            Assert.Equal(8, (int)AiConfig.LLMProviderEnum.GROK);
            Assert.Equal(9, (int)AiConfig.LLMProviderEnum.MISTRAL);
            Assert.Equal(10, (int)AiConfig.LLMProviderEnum.OLLAMA);
            Assert.Equal(11, (int)AiConfig.LLMProviderEnum.PERPLEXITY);
        }

        [Fact]
        public void VectorDBEnum_HasAllDatabases()
        {
            Assert.Equal(1, (int)AiConfig.VectorDBEnum.PINECONE_DB);
            Assert.Equal(2, (int)AiConfig.VectorDBEnum.WEAVIATE_DB);
            Assert.Equal(3, (int)AiConfig.VectorDBEnum.POSTGRES_DB);
            Assert.Equal(4, (int)AiConfig.VectorDBEnum.MONGO_DB);
        }

        [Fact]
        public void LLMProviderEnum_HasCorrectEnumMemberValues()
        {
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.ANTHROPIC, "anthropic");
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.AWS_BEDROCK, "aws_bedrock");
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.COHERE, "cohere");
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.GROK, "grok");
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.MISTRAL, "mistral");
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.OLLAMA, "ollama");
            AssertEnumMemberValue(AiConfig.LLMProviderEnum.PERPLEXITY, "perplexity");
        }

        [Fact]
        public void VectorDBEnum_HasCorrectEnumMemberValues()
        {
            AssertEnumMemberValue(AiConfig.VectorDBEnum.POSTGRES_DB, "postgresdb");
            AssertEnumMemberValue(AiConfig.VectorDBEnum.MONGO_DB, "mongodb");
        }

        // Integration config tests
        [Fact]
        public void AnthropicConfig_SetApiKey()
        {
            var config = new AnthropicConfig("test-key");
            Assert.Equal("test-key", config.ApiKey);
            var dict = config.ToDictionary();
            Assert.Equal("test-key", dict["api_key"]);
        }

        [Fact]
        public void AwsBedrockConfig_SetsAllParams()
        {
            var config = new AwsBedrockConfig("access-key", "secret-key", "us-west-2");
            Assert.Equal("access-key", config.AccessKeyId);
            Assert.Equal("secret-key", config.SecretAccessKey);
            Assert.Equal("us-west-2", config.Region);

            var dict = config.ToDictionary();
            Assert.Equal("access-key", dict["accessKeyId"]);
            Assert.Equal("secret-key", dict["secretAccessKey"]);
            Assert.Equal("us-west-2", dict["region"]);
        }

        [Fact]
        public void AwsBedrockConfig_DefaultRegion()
        {
            var config = new AwsBedrockConfig("ak", "sk");
            Assert.Equal("us-east-1", config.Region);
        }

        [Fact]
        public void CohereConfig_SetApiKey()
        {
            var config = new CohereConfig("test-key");
            Assert.Equal("test-key", config.ApiKey);
            var dict = config.ToDictionary();
            Assert.Equal("test-key", dict["api_key"]);
        }

        [Fact]
        public void GrokConfig_SetApiKey()
        {
            var config = new GrokConfig("test-key");
            Assert.Equal("test-key", config.ApiKey);
            var dict = config.ToDictionary();
            Assert.Equal("test-key", dict["api_key"]);
        }

        [Fact]
        public void MistralConfig_SetApiKey()
        {
            var config = new MistralConfig("test-key");
            Assert.Equal("test-key", config.ApiKey);
            var dict = config.ToDictionary();
            Assert.Equal("test-key", dict["api_key"]);
        }

        [Fact]
        public void OllamaConfig_DefaultEndpoint()
        {
            var config = new OllamaConfig();
            Assert.Equal("http://localhost:11434", config.Endpoint);
        }

        [Fact]
        public void OllamaConfig_CustomEndpoint()
        {
            var config = new OllamaConfig("http://myhost:11434");
            Assert.Equal("http://myhost:11434", config.Endpoint);
            var dict = config.ToDictionary();
            Assert.Equal("http://myhost:11434", dict["endpoint"]);
        }

        [Fact]
        public void PerplexityConfig_SetApiKey()
        {
            var config = new PerplexityConfig("test-key");
            Assert.Equal("test-key", config.ApiKey);
            var dict = config.ToDictionary();
            Assert.Equal("test-key", dict["api_key"]);
        }

        [Fact]
        public void PostgresConfig_SetConnectionString()
        {
            var config = new PostgresConfig("Host=localhost;Database=test");
            Assert.Equal("Host=localhost;Database=test", config.ConnectionString);
            var dict = config.ToDictionary();
            Assert.Equal("Host=localhost;Database=test", dict["connectionString"]);
        }

        [Fact]
        public void MongoDbConfig_SetAllParams()
        {
            var config = new MongoDbConfig("mongodb://localhost", "testdb", "testcol");
            Assert.Equal("mongodb://localhost", config.ConnectionString);
            Assert.Equal("testdb", config.DatabaseName);
            Assert.Equal("testcol", config.CollectionName);

            var dict = config.ToDictionary();
            Assert.Equal("mongodb://localhost", dict["connectionString"]);
            Assert.Equal("testdb", dict["databaseName"]);
            Assert.Equal("testcol", dict["collectionName"]);
        }

        [Fact]
        public void MongoDbConfig_OptionalFieldsOmittedWhenNull()
        {
            var config = new MongoDbConfig("mongodb://localhost");

            var dict = config.ToDictionary();
            Assert.True(dict.ContainsKey("connectionString"));
            Assert.False(dict.ContainsKey("databaseName"));
            Assert.False(dict.ContainsKey("collectionName"));
        }

        // Existing config tests
        [Fact]
        public void OpenAIConfig_SetApiKey()
        {
            var config = new OpenAIConfig("sk-test-key");
            Assert.Equal("sk-test-key", config.ApiKey);
            var dict = config.ToDictionary();
            Assert.Equal("sk-test-key", dict["api_key"]);
        }

        [Fact]
        public void AzureOpenAIConfig_SetsAllParams()
        {
            var config = new AzureOpenAIConfig("api-key", "https://myendpoint.openai.azure.com");
            Assert.Equal("api-key", config.ApiKey);
            Assert.Equal("https://myendpoint.openai.azure.com", config.Endpoint);

            var dict = config.ToDictionary();
            Assert.Equal("api-key", dict["api_key"]);
            Assert.Equal("https://myendpoint.openai.azure.com", dict["endpoint"]);
        }

        [Fact]
        public void WeaviateConfig_SetsAllParams()
        {
            var config = new WeaviateConfig("api-key", "https://weaviate.example.com", "MyClass");
            Assert.Equal("api-key", config.ApiKey);
            Assert.Equal("https://weaviate.example.com", config.Endpoint);
            Assert.Equal("MyClass", config.ClassName);

            var dict = config.ToDictionary();
            Assert.Equal("api-key", dict["api_key"]);
            Assert.Equal("https://weaviate.example.com", dict["endpoint"]);
        }

        [Fact]
        public void PineconeConfig_SetsAllParams()
        {
            var config = new PineconeConfig("api-key", "https://pinecone.example.com", "us-east-1", "myproject");
            Assert.Equal("api-key", config.ApiKey);
            Assert.Equal("https://pinecone.example.com", config.Endpoint);
            Assert.Equal("us-east-1", config.Environment);
            Assert.Equal("myproject", config.ProjectName);
        }

        private void AssertEnumMemberValue<T>(T enumValue, string expectedValue) where T : System.Enum
        {
            var memberInfo = typeof(T).GetMember(enumValue.ToString());
            Assert.NotEmpty(memberInfo);
            var attr = (EnumMemberAttribute)System.Attribute.GetCustomAttribute(memberInfo[0], typeof(EnumMemberAttribute));
            Assert.NotNull(attr);
            Assert.Equal(expectedValue, attr.Value);
        }
    }
}
