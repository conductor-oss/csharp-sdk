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
using Conductor.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ThreadTask = System.Threading.Tasks;

namespace conductor_csharp.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ISecretResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Delete a secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        Object DeleteSecret(string key);

        /// <summary>
        /// Delete tags of the secret
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        void DeleteTagForSecret(List<TagObject> body, string key);

        /// <summary>
        /// Get secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>string</returns>
        string GetSecret(string key);

        /// <summary>
        /// Get tags by secret
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        List<TagObject> GetTags(string key);

        /// <summary>
        /// List all secret names
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        Object ListAllSecretNames();

        /// <summary>
        /// List all secret names user can grant access to
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;string&gt;</returns>
        List<string> ListSecretsThatUserCanGrantAccessTo();

        /// <summary>
        /// List all secret names along with tags user can grant access to
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;ExtendedSecret&gt;</returns>
        List<ExtendedSecret> ListSecretsWithTagsThatUserCanGrantAccessTo();

        /// <summary>
        /// Put a secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        Object PutSecret(string body, string key);

        /// <summary>
        /// Tag a secret
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        void PutTagForSecret(List<TagObject> body, string key);

        /// <summary>
        /// Check if secret exists
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        Object SecretExists(string key);

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Delete a secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> DeleteSecretAsync(string key);

        /// <summary>
        /// Asynchronous Delete tags of the secret
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        ThreadTask.Task DeleteTagForSecretAsync(List<TagObject> body, string key);

        /// <summary>
        /// Asynchronous Get secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>string</returns>
        ThreadTask.Task<string> GetSecretAsync(string key);

        /// <summary>
        /// Asynchronous Get tags by secret
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        ThreadTask.Task<List<TagObject>> GetTagsAsync(string key);

        /// <summary>
        /// Asynchronous List all secret names
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> ListAllSecretNamesAsync();

        /// <summary>
        /// Asynchronous List all secret names user can grant access to
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;string&gt;</returns>
        ThreadTask.Task<List<string>> ListSecretsThatUserCanGrantAccessToAsync();

        /// <summary>
        /// Asynchronous List all secret names along with tags user can grant access to
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;ExtendedSecret&gt;</returns>
        ThreadTask.Task<List<ExtendedSecret>> ListSecretsWithTagsThatUserCanGrantAccessToAsync();

        /// <summary>
        /// Asynchronous Put a secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> PutSecretAsync(string body, string key);

        /// <summary>
        /// Asynchronous Tag a secret
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        ThreadTask.Task PutTagForSecretAsync(List<TagObject> body, string key);

        /// <summary>
        /// Asynchronous Check if secret exists
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> SecretExistsAsync(string key);
        #endregion Asynchronous Operations
    }
}
