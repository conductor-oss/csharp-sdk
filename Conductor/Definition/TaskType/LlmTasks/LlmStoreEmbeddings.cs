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
using Conductor.Definition.TaskType.LlmTasks.Utils;

namespace Conductor.Definition.TaskType.LlmTasks
{
    public class LlmStoreEmbeddings : Task
    {
        public LlmStoreEmbeddings(string taskReferenceName, string vectorDB, string index, string nameSpace, EmbeddingModel embeddingModel, string text, string docId) : base(taskReferenceName, WorkflowTask.WorkflowTaskTypeEnum.LLMSTOREEMBEDDINGS)
        {
            WithInput(Constants.VECTORDB, vectorDB);
            WithInput(Constants.INDEX, index);
            WithInput(Constants.NAMESPACE, nameSpace);
            WithInput(Constants.EMBEDDING_MODEL_PROVIDER, embeddingModel.Provider);
            WithInput(Constants.EMBEDDING_MODEL, embeddingModel.Model);
            WithInput(Constants.TEXT, text);
            WithInput(Constants.DOCID, docId);
        }
    }
}
