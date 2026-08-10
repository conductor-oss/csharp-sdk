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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using Conductor.Client;
using Conductor.Client.Models;
using ThreadTask = System.Threading.Tasks;
using conductor_csharp.Api;

namespace Conductor.Api
{


    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class GroupResourceApi : IGroupResourceApi
    {
        private Conductor.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupResourceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public GroupResourceApi(String basePath)
        {
            this.Configuration = new Conductor.Client.Configuration { BasePath = basePath };

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupResourceApi"/> class
        /// </summary>
        /// <returns></returns>
        public GroupResourceApi()
        {
            this.Configuration = Conductor.Client.Configuration.Default;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupResourceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public GroupResourceApi(Conductor.Client.Configuration configuration = null)
        {
            if (configuration == null) // use the default one in Configuration
                this.Configuration = Conductor.Client.Configuration.Default;
            else
                this.Configuration = configuration;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public String GetBasePath()
        {
            return this.Configuration.ApiClient.RestClient.Options.BaseUrl.ToString();
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Conductor.Client.Configuration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public Conductor.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        /// Add user to group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        public Object AddUserToGroup(string groupId, string userId)
        {
            ApiResponse<Object> localVarResponse = AddUserToGroupWithHttpInfo(groupId, userId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Add user to group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> AddUserToGroupAsync(string groupId, string userId)
        {
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->AddUserToGroup");
            // verify the required parameter 'userId' is set
            if (userId == null)
                throw new ApiException(400, "Missing required parameter 'userId' when calling GroupResourceApi->AddUserToGroup");

            var localVarPath = "/groups/{groupId}/users/{userId}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (userId != null) localVarPathParams.Add("userId", this.Configuration.ApiClient.ParameterToString(userId)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "AddUserToGroup")).Data;
        }

        /// <summary>
        /// Add user to group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> AddUserToGroupWithHttpInfo(string groupId, string userId)
        {
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->AddUserToGroup");
            // verify the required parameter 'userId' is set
            if (userId == null)
                throw new ApiException(400, "Missing required parameter 'userId' when calling GroupResourceApi->AddUserToGroup");

            var localVarPath = "/groups/{groupId}/users/{userId}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (userId != null) localVarPathParams.Add("userId", this.Configuration.ApiClient.ParameterToString(userId)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "AddUserToGroup");
        }

        /// <summary>
        /// Add users to group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public void AddUsersToGroup(List<string> body, string groupId)
        {
            AddUsersToGroupWithHttpInfo(body, groupId);
        }

        /// <summary>
        /// Asynchronous Add users to group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async ThreadTask.Task AddUsersToGroupAsync(List<string> body, string groupId)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling GroupResourceApi->AddUsersToGroup");
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->AddUsersToGroup");

            var localVarPath = "/groups/{groupId}/users";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
      "application/json"
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "AddUsersToGroup");
        }

        /// <summary>
        /// Add users to group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> AddUsersToGroupWithHttpInfo(List<string> body, string groupId)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling GroupResourceApi->AddUsersToGroup");
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->AddUsersToGroup");

            var localVarPath = "/groups/{groupId}/users";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
      "application/json"
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "AddUsersToGroup");
        }

        /// <summary>
        /// Delete a group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Response</returns>
        public Response DeleteGroup(string id)
        {
            ApiResponse<Response> localVarResponse = DeleteGroupWithHttpInfo(id);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Delete a group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Response</returns>
        public async ThreadTask.Task<Response> DeleteGroupAsync(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->DeleteGroup");

            var localVarPath = "/groups/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Response>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "DeleteGroup")).Data;
        }

        /// <summary>
        /// Delete a group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>ApiResponse of Response</returns>
        public ApiResponse<Response> DeleteGroupWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->DeleteGroup");

            var localVarPath = "/groups/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            return this.Configuration.ApiClient.Execute<Response>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "DeleteGroup");
        }

        /// <summary>
        /// Get the permissions this group has over workflows and tasks 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <returns>Object</returns>
        public Object GetGrantedPermissions(string groupId)
        {
            ApiResponse<Object> localVarResponse = GetGrantedPermissionsWithHttpInfo(groupId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get the permissions this group has over workflows and tasks 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> GetGrantedPermissionsAsync(string groupId)
        {
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->GetGrantedPermissions");

            var localVarPath = "/groups/{groupId}/permissions";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetGrantedPermissions")).Data;
        }

        /// <summary>
        /// Get the permissions this group has over workflows and tasks 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> GetGrantedPermissionsWithHttpInfo(string groupId)
        {
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->GetGrantedPermissions");

            var localVarPath = "/groups/{groupId}/permissions";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetGrantedPermissions");
        }

        /// <summary>
        /// Get a group by id 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        public Object GetGroup(string id)
        {
            ApiResponse<Object> localVarResponse = GetGroupWithHttpInfo(id);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get a group by id 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> GetGroupAsync(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->GetGroup");

            var localVarPath = "/groups/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetGroup")).Data;
        }

        /// <summary>
        /// Get a group by id 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> GetGroupWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->GetGroup");

            var localVarPath = "/groups/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetGroup");
        }

        /// <summary>
        /// Get all users in group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        public Object GetUsersInGroup(string id)
        {
            ApiResponse<Object> localVarResponse = GetUsersInGroupWithHttpInfo(id);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get all users in group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> GetUsersInGroupAsync(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->GetUsersInGroup");

            var localVarPath = "/groups/{id}/users";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetUsersInGroup")).Data;
        }

        /// <summary>
        /// Get all users in group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> GetUsersInGroupWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->GetUsersInGroup");

            var localVarPath = "/groups/{id}/users";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetUsersInGroup");
        }

        /// <summary>
        /// Get all groups 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Group&gt;</returns>
        public List<Group> ListGroups()
        {
            ApiResponse<List<Group>> localVarResponse = ListGroupsWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get all groups 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Group&gt;</returns>
        public async ThreadTask.Task<List<Group>> ListGroupsAsync()
        {
            var localVarPath = "/groups";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            return (await this.Configuration.ApiClient.ExecuteAsync<List<Group>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListGroups")).Data;
        }

        /// <summary>
        /// Get all groups 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Group&gt;</returns>
        public ApiResponse<List<Group>> ListGroupsWithHttpInfo()
        {

            var localVarPath = "/groups";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            return this.Configuration.ApiClient.Execute<List<Group>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListGroups");
        }

        /// <summary>
        /// Remove user from group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        public Object RemoveUserFromGroup(string groupId, string userId)
        {
            ApiResponse<Object> localVarResponse = RemoveUserFromGroupWithHttpInfo(groupId, userId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Remove user from group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> RemoveUserFromGroupAsync(string groupId, string userId)
        {
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->RemoveUserFromGroup");
            // verify the required parameter 'userId' is set
            if (userId == null)
                throw new ApiException(400, "Missing required parameter 'userId' when calling GroupResourceApi->RemoveUserFromGroup");

            var localVarPath = "/groups/{groupId}/users/{userId}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (userId != null) localVarPathParams.Add("userId", this.Configuration.ApiClient.ParameterToString(userId)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RemoveUserFromGroup")).Data;
        }

        /// <summary>
        /// Remove user from group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> RemoveUserFromGroupWithHttpInfo(string groupId, string userId)
        {
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->RemoveUserFromGroup");
            // verify the required parameter 'userId' is set
            if (userId == null)
                throw new ApiException(400, "Missing required parameter 'userId' when calling GroupResourceApi->RemoveUserFromGroup");

            var localVarPath = "/groups/{groupId}/users/{userId}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (userId != null) localVarPathParams.Add("userId", this.Configuration.ApiClient.ParameterToString(userId)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RemoveUserFromGroup");
        }

        /// <summary>
        /// Remove users from group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public void RemoveUsersFromGroup(List<string> body, string groupId)
        {
            RemoveUsersFromGroupWithHttpInfo(body, groupId);
        }

        /// <summary>
        /// Asynchronous Remove users from group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async ThreadTask.Task RemoveUsersFromGroupAsync(List<string> body, string groupId)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling GroupResourceApi->RemoveUsersFromGroup");
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->RemoveUsersFromGroup");

            var localVarPath = "/groups/{groupId}/users";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
      "application/json"
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RemoveUsersFromGroup");
        }

        /// <summary>
        /// Remove users from group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> RemoveUsersFromGroupWithHttpInfo(List<string> body, string groupId)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling GroupResourceApi->RemoveUsersFromGroup");
            // verify the required parameter 'groupId' is set
            if (groupId == null)
                throw new ApiException(400, "Missing required parameter 'groupId' when calling GroupResourceApi->RemoveUsersFromGroup");

            var localVarPath = "/groups/{groupId}/users";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
      "application/json"
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (groupId != null) localVarPathParams.Add("groupId", this.Configuration.ApiClient.ParameterToString(groupId)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RemoveUsersFromGroup");
        }

        /// <summary>
        /// Create or update a group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        public Object UpsertGroup(UpsertGroupRequest body, string id)
        {
            ApiResponse<Object> localVarResponse = UpsertGroupWithHttpInfo(body, id);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Create or update a group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> UpsertGroupAsync(UpsertGroupRequest body, string id)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling GroupResourceApi->UpsertGroup");
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->UpsertGroup");

            var localVarPath = "/groups/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
      "application/json"
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Put, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UpsertGroup")).Data;
        }

        /// <summary>
        /// Create or update a group 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> UpsertGroupWithHttpInfo(UpsertGroupRequest body, string id)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling GroupResourceApi->UpsertGroup");
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling GroupResourceApi->UpsertGroup");

            var localVarPath = "/groups/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
      "application/json"
    };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
      "application/json"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Put, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UpsertGroup");
        }
    }
}
