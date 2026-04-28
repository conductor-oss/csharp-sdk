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
using Conductor.Definition.TaskType;
using Conductor.Definition.TaskType.LlmTasks.Utils;
using System.Collections.Generic;

namespace Conductor.Definition.TaskType.LlmTasks
{
    /// <summary>
    /// LlmIndexText
    /// </summary>
    public class LlmIndexText : Task
    {
        /// <summary>
        /// Gets or Sets TaskName
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or Sets NameSpace
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// Gets or Sets VectorDB
        /// </summary>
        public string VectorDB { get; set; }

        /// <summary>
        /// Gets or Sets Index
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Gets or Sets EmbeddingModel
        /// </summary>
        public EmbeddingModel EmbeddingModel { get; set; }

        /// <summary>
        /// Gets or Sets Text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or Sets DocId
        /// </summary>
        public string DocId { get; set; }

        /// <summary>
        /// Gets or Sets MetaData
        /// </summary>
        public Dictionary<string, object> MetaData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmIndexText" /> class
        /// </summary>
        /// <param name="taskReferenceName"></param>
        /// <param name="vectorDB"></param>
        /// <param name="index"></param>
        /// <param name="embeddingModel"></param>
        /// <param name="text"></param>
        /// <param name="docid"></param>
        /// <param name="nameSpace"></param>
        /// <param name="taskName"></param>
        /// <param name="metaData"></param>
        public LlmIndexText(string taskReferenceName, string vectorDB, string index, EmbeddingModel embeddingModel,
                            string text, string docid, string nameSpace = null, string taskName = null, Dictionary<string, object> metaData = null) : base(taskReferenceName, WorkflowTaskTypeEnum.LLMINDEXTEXT)
        {
            TaskName = taskName ?? Constants.LLM_INDEX_TEXT_TASKNAME;
            NameSpace = nameSpace;
            VectorDB = vectorDB;
            Index = index;
            EmbeddingModel = embeddingModel;
            Text = text;
            DocId = docid;
            MetaData = metaData;

            InitializeInputs();
        }

        /// <summary>
        /// Populates inputParams dictionary with LlmIndexText attributes.
        /// </summary>
        private void InitializeInputs()
        {
            if (NameSpace != null)
            {
                WithInput(Constants.NAMESPACE, NameSpace);
            }
            WithInput(Constants.VECTORDB, VectorDB);
            WithInput(Constants.INDEX, Index);
            WithInput(Constants.EMBEDDING_MODEL_PROVIDER, EmbeddingModel.Provider);
            WithInput(Constants.EMBEDDING_MODEL, EmbeddingModel.Model);
            WithInput(Constants.TEXT, Text);
            WithInput(Constants.DOCID, DocId);
            WithInput(Constants.METADATA, MetaData);
        }
    }
}
