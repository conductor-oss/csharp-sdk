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

using Conductor.Client.Models;
using System.Collections.Generic;
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Client.Interfaces
{
    public interface IPromptClient
    {
        void SaveMessageTemplate(string name, string template, string description, List<string> models = null);
        ThreadTask.Task SaveMessageTemplateAsync(string name, string template, string description, List<string> models = null);

        MessageTemplate GetMessageTemplate(string name);
        ThreadTask.Task<MessageTemplate> GetMessageTemplateAsync(string name);

        List<MessageTemplate> GetMessageTemplates();
        ThreadTask.Task<List<MessageTemplate>> GetMessageTemplatesAsync();

        void DeleteMessageTemplate(string name);
        ThreadTask.Task DeleteMessageTemplateAsync(string name);

        string TestMessageTemplate(PromptTemplateTestRequest testRequest);
        ThreadTask.Task<string> TestMessageTemplateAsync(PromptTemplateTestRequest testRequest);

        List<Tag> GetTagsForPromptTemplate(string name);
        void PutTagForPromptTemplate(List<Tag> tags, string name);
        void DeleteTagForPromptTemplate(List<Tag> tags, string name);
    }
}
