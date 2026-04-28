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
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Conductor.Client.Ai
{
    public class Configuration
    {
        /// <summary>
        /// Defines LLMProvider
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum LLMProviderEnum
        {
            /// <summary>
            /// Enum AZURE_OPEN_AI for value: azure_openai
            /// </summary>
            [EnumMember(Value = "azure_openai")]
            AZURE_OPEN_AI = 1,

            /// <summary>
            /// Enum OPEN_AI for value: openai
            /// </summary>
            [EnumMember(Value = "openai")]
            OPEN_AI = 2,

            /// <summary>
            /// Enum GCP_VERTEX_AI for value: vertex_ai
            /// </summary>
            [EnumMember(Value = "vertex_ai")]
            GCP_VERTEX_AI = 3,

            /// <summary>
            /// Enum HUGGING_FACE for value: huggingface
            /// </summary>
            [EnumMember(Value = "huggingface")]
            HUGGING_FACE = 4,

            /// <summary>
            /// Enum ANTHROPIC for value: anthropic
            /// </summary>
            [EnumMember(Value = "anthropic")]
            ANTHROPIC = 5,

            /// <summary>
            /// Enum AWS_BEDROCK for value: aws_bedrock
            /// </summary>
            [EnumMember(Value = "aws_bedrock")]
            AWS_BEDROCK = 6,

            /// <summary>
            /// Enum COHERE for value: cohere
            /// </summary>
            [EnumMember(Value = "cohere")]
            COHERE = 7,

            /// <summary>
            /// Enum GROK for value: grok
            /// </summary>
            [EnumMember(Value = "grok")]
            GROK = 8,

            /// <summary>
            /// Enum MISTRAL for value: mistral
            /// </summary>
            [EnumMember(Value = "mistral")]
            MISTRAL = 9,

            /// <summary>
            /// Enum OLLAMA for value: ollama
            /// </summary>
            [EnumMember(Value = "ollama")]
            OLLAMA = 10,

            /// <summary>
            /// Enum PERPLEXITY for value: perplexity
            /// </summary>
            [EnumMember(Value = "perplexity")]
            PERPLEXITY = 11,
        }

        /// <summary>
        /// Defines VectorDB
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum VectorDBEnum
        {
            /// <summary>
            /// Enum PINECONE_DB for value: pineconedb
            /// </summary>
            [EnumMember(Value = "pineconedb")]
            PINECONE_DB = 1,

            /// <summary>
            /// Enum WEAVIATE_DB for value: weaviatedb
            /// </summary>
            [EnumMember(Value = "weaviatedb")]
            WEAVIATE_DB = 2,

            /// <summary>
            /// Enum POSTGRES_DB for value: postgresdb
            /// </summary>
            [EnumMember(Value = "postgresdb")]
            POSTGRES_DB = 3,

            /// <summary>
            /// Enum MONGO_DB for value: mongodb
            /// </summary>
            [EnumMember(Value = "mongodb")]
            MONGO_DB = 4,
        }
    }
}