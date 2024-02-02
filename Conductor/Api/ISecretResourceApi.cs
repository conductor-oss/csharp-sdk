﻿using Conductor.Client.Models;
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
        /// <returns>Object</returns>
        Object GetSecret(string key);

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
        void DeleteTagForSecretAsync(List<TagObject> body, string key);

        /// <summary>
        /// Asynchronous Get secret value by key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> GetSecretAsync(string key);

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
        void PutTagForSecretAsync(List<TagObject> body, string key);

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
