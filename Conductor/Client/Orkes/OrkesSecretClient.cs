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
    public class OrkesSecretClient : ISecretClient
    {
        private readonly ISecretResourceApi _secretApi;

        public OrkesSecretClient(Configuration configuration)
        {
            _secretApi = configuration.GetClient<SecretResourceApi>();
        }

        public OrkesSecretClient(ISecretResourceApi secretApi)
        {
            _secretApi = secretApi;
        }

        public void PutSecret(string key, string value) => _secretApi.PutSecret(value, key);
        public ThreadTask.Task PutSecretAsync(string key, string value) => _secretApi.PutSecretAsync(value, key);

        public string GetSecret(string key) => _secretApi.GetSecret(key)?.ToString();
        public async ThreadTask.Task<string> GetSecretAsync(string key)
        {
            var result = await _secretApi.GetSecretAsync(key);
            return result?.ToString();
        }

        public bool SecretExists(string key)
        {
            var result = _secretApi.SecretExists(key);
            return result != null && bool.TryParse(result.ToString(), out var exists) && exists;
        }

        public async ThreadTask.Task<bool> SecretExistsAsync(string key)
        {
            var result = await _secretApi.SecretExistsAsync(key);
            return result != null && bool.TryParse(result.ToString(), out var exists) && exists;
        }

        public void DeleteSecret(string key) => _secretApi.DeleteSecret(key);
        public ThreadTask.Task DeleteSecretAsync(string key) => _secretApi.DeleteSecretAsync(key);

        public List<string> ListSecretNames()
        {
            var result = _secretApi.ListAllSecretNames();
            return result as List<string> ?? new List<string>();
        }

        public async ThreadTask.Task<List<string>> ListSecretNamesAsync()
        {
            var result = await _secretApi.ListAllSecretNamesAsync();
            return result as List<string> ?? new List<string>();
        }

        public List<string> ListSecretsThatUserCanGrantAccessTo() => _secretApi.ListSecretsThatUserCanGrantAccessTo();
        public ThreadTask.Task<List<string>> ListSecretsThatUserCanGrantAccessToAsync() => _secretApi.ListSecretsThatUserCanGrantAccessToAsync();

        public List<TagObject> GetTags(string key) => _secretApi.GetTags(key);
        public void PutTagForSecret(List<TagObject> tags, string key) => _secretApi.PutTagForSecret(tags, key);
        public void DeleteTagForSecret(List<TagObject> tags, string key) => _secretApi.DeleteTagForSecret(tags, key);
    }
}
