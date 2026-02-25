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
    public class OrkesIntegrationClient : IIntegrationClient
    {
        private readonly IIntegrationResourceApi _integrationApi;

        public OrkesIntegrationClient(Configuration configuration)
        {
            _integrationApi = configuration.GetClient<IntegrationResourceApi>();
        }

        public OrkesIntegrationClient(IIntegrationResourceApi integrationApi)
        {
            _integrationApi = integrationApi;
        }

        // Integration Providers
        public void SaveIntegrationProvider(IntegrationUpdate body, string name) => _integrationApi.SaveIntegrationProvider(body, name);
        public ThreadTask.Task SaveIntegrationProviderAsync(IntegrationUpdate body, string name) => _integrationApi.SaveIntegrationProviderAsync(body, name);

        public Integration GetIntegrationProvider(string name) => _integrationApi.GetIntegrationProvider(name);
        public ThreadTask.Task<Integration> GetIntegrationProviderAsync(string name) => _integrationApi.GetIntegrationProviderAsync(name);

        public List<Integration> GetIntegrationProviders(string category = null, bool? activeOnly = null) => _integrationApi.GetIntegrationProviders(category, activeOnly);
        public ThreadTask.Task<List<Integration>> GetIntegrationProvidersAsync(string category = null, bool? activeOnly = null) => _integrationApi.GetIntegrationProvidersAsync(category, activeOnly);

        public void DeleteIntegrationProvider(string name) => _integrationApi.DeleteIntegrationProvider(name);
        public ThreadTask.Task DeleteIntegrationProviderAsync(string name) => _integrationApi.DeleteIntegrationProviderAsync(name);

        // Integration APIs
        public void SaveIntegrationApi(IntegrationApiUpdate body, string integrationProvider, string integrationName) => _integrationApi.SaveIntegrationApi(body, integrationProvider, integrationName);
        public ThreadTask.Task SaveIntegrationApiAsync(IntegrationApiUpdate body, string integrationProvider, string integrationName) => _integrationApi.SaveIntegrationApiAsync(body, integrationProvider, integrationName);

        public IntegrationApi GetIntegrationApi(string integrationProvider, string integrationName) => _integrationApi.GetIntegrationApi(integrationProvider, integrationName);
        public ThreadTask.Task<IntegrationApi> GetIntegrationApiAsync(string integrationProvider, string integrationName) => _integrationApi.GetIntegrationApiAsync(integrationProvider, integrationName);

        public List<IntegrationApi> GetIntegrationApis(string integrationProvider, bool? activeOnly = null) => _integrationApi.GetIntegrationApis(integrationProvider, activeOnly);
        public ThreadTask.Task<List<IntegrationApi>> GetIntegrationApisAsync(string integrationProvider, bool? activeOnly = null) => _integrationApi.GetIntegrationApisAsync(integrationProvider, activeOnly);

        public List<string> GetIntegrationAvailableApis(string integrationProvider) => _integrationApi.GetIntegrationAvailableApis(integrationProvider);
        public ThreadTask.Task<List<string>> GetIntegrationAvailableApisAsync(string integrationProvider) => _integrationApi.GetIntegrationAvailableApisAsync(integrationProvider);

        public void DeleteIntegrationApi(string integrationProvider, string integrationName) => _integrationApi.DeleteIntegrationApi(integrationProvider, integrationName);
        public ThreadTask.Task DeleteIntegrationApiAsync(string integrationProvider, string integrationName) => _integrationApi.DeleteIntegrationApiAsync(integrationProvider, integrationName);

        // Prompt associations
        public void AssociatePromptWithIntegration(string integrationProvider, string integrationName, string promptName)
            => _integrationApi.AssociatePromptWithIntegration(integrationProvider, integrationName, promptName);
        public ThreadTask.Task AssociatePromptWithIntegrationAsync(string integrationProvider, string integrationName, string promptName)
            => _integrationApi.AssociatePromptWithIntegrationAsync(integrationProvider, integrationName, promptName);

        public List<MessageTemplate> GetPromptsWithIntegration(string integrationProvider, string integrationName)
            => _integrationApi.GetPromptsWithIntegration(integrationProvider, integrationName);
        public ThreadTask.Task<List<MessageTemplate>> GetPromptsWithIntegrationAsync(string integrationProvider, string integrationName)
            => _integrationApi.GetPromptsWithIntegrationAsync(integrationProvider, integrationName);

        // Token usage
        public int? GetTokenUsageForIntegration(string integrationProvider, string integrationName) => _integrationApi.GetTokenUsageForIntegration(integrationProvider, integrationName);
        public ThreadTask.Task<int?> GetTokenUsageForIntegrationAsync(string integrationProvider, string integrationName) => _integrationApi.GetTokenUsageForIntegrationAsync(integrationProvider, integrationName);

        public Dictionary<string, string> GetTokenUsageForIntegrationProvider(string name) => _integrationApi.GetTokenUsageForIntegrationProvider(name);
        public ThreadTask.Task<Dictionary<string, string>> GetTokenUsageForIntegrationProviderAsync(string name) => _integrationApi.GetTokenUsageForIntegrationProviderAsync(name);

        public void RegisterTokenUsage(int? body, string integrationProvider, string integrationName) => _integrationApi.RegisterTokenUsage(body, integrationProvider, integrationName);
        public ThreadTask.Task RegisterTokenUsageAsync(int? body, string integrationProvider, string integrationName) => _integrationApi.RegisterTokenUsageAsync(body, integrationProvider, integrationName);

        // Tags
        public List<Tag> GetTagsForIntegration(string integrationProvider, string integrationName) => _integrationApi.GetTagsForIntegration(integrationProvider, integrationName);
        public void PutTagForIntegration(List<Tag> tags, string integrationProvider, string integrationName) => _integrationApi.PutTagForIntegration(tags, integrationProvider, integrationName);
        public void DeleteTagForIntegration(List<Tag> tags, string integrationProvider, string integrationName) => _integrationApi.DeleteTagForIntegration(tags, integrationProvider, integrationName);

        public List<Tag> GetTagsForIntegrationProvider(string name) => _integrationApi.GetTagsForIntegrationProvider(name);
        public void PutTagForIntegrationProvider(List<Tag> tags, string name) => _integrationApi.PutTagForIntegrationProvider(tags, name);
        public void DeleteTagForIntegrationProvider(List<Tag> tags, string name) => _integrationApi.DeleteTagForIntegrationProvider(tags, name);
    }
}
