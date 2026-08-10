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
using ThreadTask = System.Threading.Tasks;
using System.Text;

namespace conductor_csharp.Api
{
    public interface IApplicationResourceApi
    {
        #region Synchronous Operations
        /// <summary>
        /// Add Role to Application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="role"></param>
        /// <returns>Object</returns>
        Object AddRoleToApplicationUser(string applicationId, string role);


        /// <summary>
        /// Create an access key for an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object CreateAccessKey(string id);

        /// <summary>
        /// Create an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        Object CreateApplication(CreateOrUpdateApplicationRequest body);

        /// <summary>
        /// Delete an access key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="keyId"></param>
        /// <returns>Object</returns>
        Object DeleteAccessKey(string applicationId, string keyId);

        /// <summary>
        /// Delete an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object DeleteApplication(string id);

        /// <summary>
        /// Delete a tag for application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        void DeleteTagForApplication(List<TagObject> body, string id);

        /// <summary>
        /// Get application&#x27;s access keys
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object GetAccessKeys(string id);

        /// <summary>
        /// Get application id by access key id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accessKeyId"></param>
        /// <returns>Object</returns>
        Object GetAppByAccessKeyId(string accessKeyId);

        /// <summary>
        /// Get an application by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object GetApplication(string id);

        /// <summary>
        /// Get tags by application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        List<TagObject> GetTagsForApplication(string id);

        /// <summary>
        /// Get all applications
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;ExtendedConductorApplication&gt;</returns>
        List<ExtendedConductorApplication> ListApplications();

        /// <summary>
        /// Put a tag to application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        void PutTagForApplication(List<TagObject> body, string id);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="role"></param>
        /// <returns>Object</returns>
        Object RemoveRoleFromApplicationUser(string applicationId, string role);

        /// <summary>
        /// Toggle the status of an access key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="keyId"></param>
        /// <returns>Object</returns>
        Object ToggleAccessKeyStatus(string applicationId, string keyId);

        /// <summary>
        /// Update an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object UpdateApplication(CreateOrUpdateApplicationRequest body, string id);

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Add Role to Application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="role"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> AddRoleToApplicationUserAsync(string applicationId, string role);


        /// <summary>
        /// Asynchronous Create an access key for an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> CreateAccessKeyAsync(string id);

        /// <summary>
        /// Asynchronous Create an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> CreateApplicationAsync(CreateOrUpdateApplicationRequest body);

        /// <summary>
        /// Asynchronous Delete an access key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="keyId"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> DeleteAccessKeyAsync(string applicationId, string keyId);

        /// <summary>
        /// Asynchronous Delete an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> DeleteApplicationAsync(string id);

        /// <summary>
        /// Asynchronous Delete a tag for application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        ThreadTask.Task DeleteTagForApplicationAsync(List<TagObject> body, string id);

        /// <summary>
        /// Asynchronous Get application&#x27;s access keys
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> GetAccessKeysAsync(string id);

        /// <summary>
        /// Asynchronous Get application id by access key id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="accessKeyId"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> GetAppByAccessKeyIdAsync(string accessKeyId);

        /// <summary>
        /// Asynchronous Get an application by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> GetApplicationAsync(string id);

        /// <summary>
        /// Asynchronous Get tags by application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        ThreadTask.Task<List<TagObject>> GetTagsForApplicationAsync(string id);

        /// <summary>
        /// Asynchronous Get all applications
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;ExtendedConductorApplication&gt;</returns>
        ThreadTask.Task<List<ExtendedConductorApplication>> ListApplicationsAsync();

        /// <summary>
        /// Asynchronous Put a tag to application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        ThreadTask.Task PutTagForApplicationAsync(List<TagObject> body, string id);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="role"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> RemoveRoleFromApplicationUserAsync(string applicationId, string role);

        /// <summary>
        /// Asynchronous Toggle the status of an access key
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="applicationId"></param>
        /// <param name="keyId"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> ToggleAccessKeyStatusAsync(string applicationId, string keyId);

        /// <summary>
        /// Asynchronous Update an application
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<object> UpdateApplicationAsync(CreateOrUpdateApplicationRequest body, string id);
        #endregion Asynchronous Operations
    }
}

