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
    public interface ISecretClient
    {
        void PutSecret(string key, string value);
        ThreadTask.Task PutSecretAsync(string key, string value);

        string GetSecret(string key);
        ThreadTask.Task<string> GetSecretAsync(string key);

        bool SecretExists(string key);
        ThreadTask.Task<bool> SecretExistsAsync(string key);

        void DeleteSecret(string key);
        ThreadTask.Task DeleteSecretAsync(string key);

        List<string> ListSecretNames();
        ThreadTask.Task<List<string>> ListSecretNamesAsync();

        List<string> ListSecretsThatUserCanGrantAccessTo();
        ThreadTask.Task<List<string>> ListSecretsThatUserCanGrantAccessToAsync();

        List<TagObject> GetTags(string key);
        void PutTagForSecret(List<TagObject> tags, string key);
        void DeleteTagForSecret(List<TagObject> tags, string key);
    }
}
