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
    public interface IIntegrationClient
    {
        // Integration Providers
        void SaveIntegrationProvider(IntegrationUpdate body, string name);
        ThreadTask.Task SaveIntegrationProviderAsync(IntegrationUpdate body, string name);

        Integration GetIntegrationProvider(string name);
        ThreadTask.Task<Integration> GetIntegrationProviderAsync(string name);

        List<Integration> GetIntegrationProviders(string category = null, bool? activeOnly = null);
        ThreadTask.Task<List<Integration>> GetIntegrationProvidersAsync(string category = null, bool? activeOnly = null);

        void DeleteIntegrationProvider(string name);
        ThreadTask.Task DeleteIntegrationProviderAsync(string name);

        // Integration APIs
        void SaveIntegrationApi(IntegrationApiUpdate body, string integrationProvider, string integrationName);
        ThreadTask.Task SaveIntegrationApiAsync(IntegrationApiUpdate body, string integrationProvider, string integrationName);

        IntegrationApi GetIntegrationApi(string integrationProvider, string integrationName);
        ThreadTask.Task<IntegrationApi> GetIntegrationApiAsync(string integrationProvider, string integrationName);

        List<IntegrationApi> GetIntegrationApis(string integrationProvider, bool? activeOnly = null);
        ThreadTask.Task<List<IntegrationApi>> GetIntegrationApisAsync(string integrationProvider, bool? activeOnly = null);

        List<string> GetIntegrationAvailableApis(string integrationProvider);
        ThreadTask.Task<List<string>> GetIntegrationAvailableApisAsync(string integrationProvider);

        void DeleteIntegrationApi(string integrationProvider, string integrationName);
        ThreadTask.Task DeleteIntegrationApiAsync(string integrationProvider, string integrationName);

        // Prompt associations
        void AssociatePromptWithIntegration(string integrationProvider, string integrationName, string promptName);
        ThreadTask.Task AssociatePromptWithIntegrationAsync(string integrationProvider, string integrationName, string promptName);

        List<MessageTemplate> GetPromptsWithIntegration(string integrationProvider, string integrationName);
        ThreadTask.Task<List<MessageTemplate>> GetPromptsWithIntegrationAsync(string integrationProvider, string integrationName);

        // Token usage
        int? GetTokenUsageForIntegration(string integrationProvider, string integrationName);
        ThreadTask.Task<int?> GetTokenUsageForIntegrationAsync(string integrationProvider, string integrationName);

        Dictionary<string, string> GetTokenUsageForIntegrationProvider(string name);
        ThreadTask.Task<Dictionary<string, string>> GetTokenUsageForIntegrationProviderAsync(string name);

        void RegisterTokenUsage(int? body, string integrationProvider, string integrationName);
        ThreadTask.Task RegisterTokenUsageAsync(int? body, string integrationProvider, string integrationName);

        // Tags
        List<Tag> GetTagsForIntegration(string integrationProvider, string integrationName);
        void PutTagForIntegration(List<Tag> tags, string integrationProvider, string integrationName);
        void DeleteTagForIntegration(List<Tag> tags, string integrationProvider, string integrationName);

        List<Tag> GetTagsForIntegrationProvider(string name);
        void PutTagForIntegrationProvider(List<Tag> tags, string name);
        void DeleteTagForIntegrationProvider(List<Tag> tags, string name);
    }
}
