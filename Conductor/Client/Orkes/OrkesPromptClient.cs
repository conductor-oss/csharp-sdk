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

using Conductor.Api;
using conductor_csharp.Api;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using System.Collections.Generic;
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Client.Orkes
{
    public class OrkesPromptClient : IPromptClient
    {
        private readonly IPromptResourceApi _promptApi;

        public OrkesPromptClient(Configuration configuration)
        {
            _promptApi = configuration.GetClient<PromptResourceApi>();
        }

        public OrkesPromptClient(IPromptResourceApi promptApi)
        {
            _promptApi = promptApi;
        }

        public void SaveMessageTemplate(string name, string template, string description, List<string> models = null)
            => _promptApi.SaveMessageTemplate(template, description, name, models);
        public ThreadTask.Task SaveMessageTemplateAsync(string name, string template, string description, List<string> models = null)
            => _promptApi.SaveMessageTemplateAsync(template, description, name, models);

        public MessageTemplate GetMessageTemplate(string name) => _promptApi.GetMessageTemplate(name);
        public ThreadTask.Task<MessageTemplate> GetMessageTemplateAsync(string name) => _promptApi.GetMessageTemplateAsync(name);

        public List<MessageTemplate> GetMessageTemplates() => _promptApi.GetMessageTemplates();
        public ThreadTask.Task<List<MessageTemplate>> GetMessageTemplatesAsync() => _promptApi.GetMessageTemplatesAsync();

        public void DeleteMessageTemplate(string name) => _promptApi.DeleteMessageTemplate(name);
        public ThreadTask.Task DeleteMessageTemplateAsync(string name) => _promptApi.DeleteMessageTemplateAsync(name);

        public string TestMessageTemplate(PromptTemplateTestRequest testRequest) => _promptApi.TestMessageTemplate(testRequest);
        public ThreadTask.Task<string> TestMessageTemplateAsync(PromptTemplateTestRequest testRequest) => _promptApi.TestMessageTemplateAsync(testRequest);

        public List<Tag> GetTagsForPromptTemplate(string name) => _promptApi.GetTagsForPromptTemplate(name);
        public void PutTagForPromptTemplate(List<Tag> tags, string name) => _promptApi.PutTagForPromptTemplate(tags, name);
        public void DeleteTagForPromptTemplate(List<Tag> tags, string name) => _promptApi.DeleteTagForPromptTemplate(tags, name);
    }
}
