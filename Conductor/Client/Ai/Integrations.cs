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
using System.Collections.Generic;
using EnvironmentInstance = System.Environment;

namespace Conductor.Client.Ai
{
    /// <summary>
    /// Integration configuration abstract base class.
    /// </summary>
    public abstract class IntegrationConfig
    {
        /// <summary>
        /// Converts the configuration to a dictionary.
        /// </summary>
        /// <returns>A dictionary representation of the configuration.</returns>
        public abstract Dictionary<string, object> ToDictionary();
    }

    /// <summary>
    /// Configuration class for Weaviate integration.
    /// </summary>
    public class WeaviateConfig : IntegrationConfig
    {
        /// <summary>
        /// Gets or Sets ApiKey
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or Sets Endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or Sets Class
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaviateConfig" /> class
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="endpoint"></param>
        /// <param name="className"></param>
        public WeaviateConfig(string apiKey, string endpoint, string className)
        {
            ApiKey = apiKey;
            Endpoint = endpoint;
            ClassName = className;
        }

        /// <summary>
        /// Inherited method
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey },
                { Constants.ENDPOINT, Endpoint }
            };
        }
    }

    /// <summary>
    /// Configuration class for OpenAIConfig integration.
    /// </summary>
    public class OpenAIConfig : IntegrationConfig
    {
        /// <summary>
        /// Gets or Sets ApiKey
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIConfig" /> class
        /// </summary>
        /// <param name="apiKey"></param>
        public OpenAIConfig(string apiKey = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable(Constants.OPENAIAPIKEY);
        }

        /// <summary>
        /// Inherited method
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey }
            };
        }
    }

    /// <summary>
    /// Configuration class for AzureOpenAIConfig integration.
    /// </summary>
    public class AzureOpenAIConfig : IntegrationConfig
    {
        /// <summary>
        /// Gets or Sets ApiKey
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or Sets Endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAIConfig" /> class
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="endpoint"></param>
        public AzureOpenAIConfig(string apiKey, string endpoint)
        {
            ApiKey = apiKey;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Inherited method
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey },
                { Constants.ENDPOINT, Endpoint }
            };
        }
    }

    /// <summary>
    /// Configuration class for PineconeConfig integration.
    /// </summary>
    public class PineconeConfig : IntegrationConfig
    {
        /// <summary>
        /// Gets or Sets ApiKey
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or Sets Endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or Sets Environment
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Gets or Sets ProjectName
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PineconeConfig" /> class
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="endpoint"></param>
        /// <param name="environment"></param>
        /// <param name="projectName"></param>
        public PineconeConfig(string apiKey = null, string endpoint = null, string environment = null, string projectName = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable(Constants.PINECONEAPIKEY);
            Endpoint = endpoint ?? EnvironmentInstance.GetEnvironmentVariable(Constants.PINECONEENDPOINT);
            Environment = environment ?? EnvironmentInstance.GetEnvironmentVariable(Constants.PINECONEENV);
            ProjectName = projectName ?? EnvironmentInstance.GetEnvironmentVariable(Constants.PINECONEPROJECT);
        }

        /// <summary>
        /// Inherited method
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey },
                { Constants.ENDPOINT, Endpoint },
                { Constants.PROJECTNAME, ProjectName },
                { Constants.ENVIRONMENT, Environment }
            };
        }
    }

    /// <summary>
    /// Configuration class for Anthropic integration.
    /// </summary>
    public class AnthropicConfig : IntegrationConfig
    {
        public string ApiKey { get; set; }

        public AnthropicConfig(string apiKey = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey }
            };
        }
    }

    /// <summary>
    /// Configuration class for AWS Bedrock integration.
    /// </summary>
    public class AwsBedrockConfig : IntegrationConfig
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Region { get; set; }

        public AwsBedrockConfig(string accessKeyId = null, string secretAccessKey = null, string region = null)
        {
            AccessKeyId = accessKeyId ?? EnvironmentInstance.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            SecretAccessKey = secretAccessKey ?? EnvironmentInstance.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            Region = region ?? EnvironmentInstance.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "accessKeyId", AccessKeyId },
                { "secretAccessKey", SecretAccessKey },
                { "region", Region }
            };
        }
    }

    /// <summary>
    /// Configuration class for Cohere integration.
    /// </summary>
    public class CohereConfig : IntegrationConfig
    {
        public string ApiKey { get; set; }

        public CohereConfig(string apiKey = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable("COHERE_API_KEY");
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey }
            };
        }
    }

    /// <summary>
    /// Configuration class for Grok integration.
    /// </summary>
    public class GrokConfig : IntegrationConfig
    {
        public string ApiKey { get; set; }

        public GrokConfig(string apiKey = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable("GROK_API_KEY");
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey }
            };
        }
    }

    /// <summary>
    /// Configuration class for Mistral integration.
    /// </summary>
    public class MistralConfig : IntegrationConfig
    {
        public string ApiKey { get; set; }

        public MistralConfig(string apiKey = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable("MISTRAL_API_KEY");
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey }
            };
        }
    }

    /// <summary>
    /// Configuration class for Ollama integration.
    /// </summary>
    public class OllamaConfig : IntegrationConfig
    {
        public string Endpoint { get; set; }

        public OllamaConfig(string endpoint = null)
        {
            Endpoint = endpoint ?? EnvironmentInstance.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.ENDPOINT, Endpoint }
            };
        }
    }

    /// <summary>
    /// Configuration class for Perplexity integration.
    /// </summary>
    public class PerplexityConfig : IntegrationConfig
    {
        public string ApiKey { get; set; }

        public PerplexityConfig(string apiKey = null)
        {
            ApiKey = apiKey ?? EnvironmentInstance.GetEnvironmentVariable("PERPLEXITY_API_KEY");
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { Constants.APIKEY, ApiKey }
            };
        }
    }

    /// <summary>
    /// Configuration class for PostgreSQL/pgvector integration.
    /// </summary>
    public class PostgresConfig : IntegrationConfig
    {
        public string ConnectionString { get; set; }

        public PostgresConfig(string connectionString = null)
        {
            ConnectionString = connectionString ?? EnvironmentInstance.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "connectionString", ConnectionString }
            };
        }
    }

    /// <summary>
    /// Configuration class for MongoDB vector DB integration.
    /// </summary>
    public class MongoDbConfig : IntegrationConfig
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }

        public MongoDbConfig(string connectionString = null, string databaseName = null, string collectionName = null)
        {
            ConnectionString = connectionString ?? EnvironmentInstance.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            DatabaseName = databaseName;
            CollectionName = collectionName;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>
            {
                { "connectionString", ConnectionString }
            };
            if (DatabaseName != null) dict["databaseName"] = DatabaseName;
            if (CollectionName != null) dict["collectionName"] = CollectionName;
            return dict;
        }
    }
}