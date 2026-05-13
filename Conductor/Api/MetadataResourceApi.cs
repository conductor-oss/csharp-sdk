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
using Conductor.Client;
using Conductor.Client.Models;
using conductor_csharp.Api;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Api
{


    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class MetadataResourceApi : IMetadataResourceApi
    {
        private Conductor.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataResourceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public MetadataResourceApi(String basePath)
        {
            this.Configuration = new Conductor.Client.Configuration { BasePath = basePath };

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataResourceApi"/> class
        /// </summary>
        /// <returns></returns>
        public MetadataResourceApi()
        {
            this.Configuration = Conductor.Client.Configuration.Default;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataResourceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public MetadataResourceApi(Conductor.Client.Configuration configuration = null)
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
        /// Create a new workflow definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to false)</param>
        /// <returns>Object</returns>
        public Object Create(WorkflowDef body, bool? overwrite = null)
        {
            ApiResponse<Object> localVarResponse = CreateWithHttpInfo(body, overwrite);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Create a new workflow definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to false)</param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> CreateAsync(WorkflowDef body, bool? overwrite = null)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->Create");

            var localVarPath = "/metadata/workflow";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (overwrite != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "overwrite", overwrite)); // query parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "Create")).Data;
        }

        /// <summary>
        /// Create a new workflow definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to false)</param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> CreateWithHttpInfo(WorkflowDef body, bool? overwrite = null)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->Create");

            var localVarPath = "/metadata/workflow";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (overwrite != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "overwrite", overwrite)); // query parameter
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
                ExceptionFactory, "Create");
        }

        /// <summary>
        /// Retrieves workflow definition along with blueprint 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>WorkflowDef</returns>
        public WorkflowDef Get(string name, int? version = null, bool? metadata = null)
        {
            ApiResponse<WorkflowDef> localVarResponse = GetWithHttpInfo(name, version, metadata);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Retrieves workflow definition along with blueprint 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>WorkflowDef</returns>
        public async ThreadTask.Task<WorkflowDef> GetAsync(string name, int? version = null, bool? metadata = null)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling MetadataResourceApi->Get");

            var localVarPath = "/metadata/workflow/{name}";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (name != null) localVarPathParams.Add("name", this.Configuration.ApiClient.ParameterToString(name)); // path parameter
            if (version != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "version", version)); // query parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<WorkflowDef>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "Get")).Data;
        }

        /// <summary>
        /// Retrieves workflow definition along with blueprint 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"> (optional)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>ApiResponse of WorkflowDef</returns>
        public ApiResponse<WorkflowDef> GetWithHttpInfo(string name, int? version = null, bool? metadata = null)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling MetadataResourceApi->Get");

            var localVarPath = "/metadata/workflow/{name}";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (name != null) localVarPathParams.Add("name", this.Configuration.ApiClient.ParameterToString(name)); // path parameter
            if (version != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "version", version)); // query parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter

            return this.Configuration.ApiClient.Execute<WorkflowDef>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "Get");
        }

        /// <summary>
        /// Retrieves all workflow definition along with blueprint 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <param name="_short"> (optional, default to false)</param>
        /// <returns>List&lt;WorkflowDef&gt;</returns>
        public List<WorkflowDef> GetAllWorkflows(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null, bool? _short = null)
        {
            ApiResponse<List<WorkflowDef>> localVarResponse = GetAllWorkflowsWithHttpInfo(access, metadata, tagKey, tagValue, _short);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Retrieves all workflow definition along with blueprint 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <param name="_short"> (optional, default to false)</param>
        /// <returns>List&lt;WorkflowDef&gt;</returns>
        public async ThreadTask.Task<List<WorkflowDef>> GetAllWorkflowsAsync(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null, bool? _short = null)
        {
            var localVarPath = "/metadata/workflow";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (access != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "access", access)); // query parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter
            if (tagKey != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagKey", tagKey)); // query parameter
            if (tagValue != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagValue", tagValue)); // query parameter
            if (_short != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "short", _short)); // query parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<List<WorkflowDef>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetAllWorkflows")).Data;
        }
        /// <summary>
        /// Retrieves all workflow definition along with blueprint 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <param name="_short"> (optional, default to false)</param>
        /// <returns>ApiResponse of List&lt;WorkflowDef&gt;</returns>
        public ApiResponse<List<WorkflowDef>> GetAllWorkflowsWithHttpInfo(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null, bool? _short = null)
        {

            var localVarPath = "/metadata/workflow";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (access != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "access", access)); // query parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter
            if (tagKey != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagKey", tagKey)); // query parameter
            if (tagValue != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagValue", tagValue)); // query parameter
            if (_short != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "short", _short)); // query parameter

            return this.Configuration.ApiClient.Execute<List<WorkflowDef>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetAllWorkflows");
        }

        /// <summary>
        /// Gets the task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>Object</returns>
        public TaskDef GetTaskDef(string tasktype, bool? metadata = null)
        {
            ApiResponse<TaskDef> localVarResponse = GetTaskDefWithHttpInfo(tasktype, metadata);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Gets the task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<TaskDef> GetTaskDefAsync(string tasktype, bool? metadata = null)
        {
            // verify the required parameter 'tasktype' is set
            if (tasktype == null)
                throw new ApiException(400, "Missing required parameter 'tasktype' when calling MetadataResourceApi->GetTaskDef");

            var localVarPath = "/metadata/taskdefs/{tasktype}";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (tasktype != null) localVarPathParams.Add("tasktype", this.Configuration.ApiClient.ParameterToString(tasktype)); // path parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<TaskDef>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetTaskDef")).Data;
        }

        /// <summary>
        /// Gets the task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<TaskDef> GetTaskDefWithHttpInfo(string tasktype, bool? metadata = null)
        {
            // verify the required parameter 'tasktype' is set
            if (tasktype == null)
                throw new ApiException(400, "Missing required parameter 'tasktype' when calling MetadataResourceApi->GetTaskDef");

            var localVarPath = "/metadata/taskdefs/{tasktype}";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (tasktype != null) localVarPathParams.Add("tasktype", this.Configuration.ApiClient.ParameterToString(tasktype)); // path parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter

            return this.Configuration.ApiClient.Execute<TaskDef>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetTaskDef");
        }

        /// <summary>
        /// Gets all task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <returns>List&lt;TaskDef&gt;</returns>
        public List<TaskDef> GetTaskDefs(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null)
        {
            ApiResponse<List<TaskDef>> localVarResponse = GetTaskDefsWithHttpInfo(access, metadata, tagKey, tagValue);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Gets all task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <returns>List&lt;TaskDef&gt;</returns>
        public async ThreadTask.Task<List<TaskDef>> GetTaskDefsAsync(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null)
        {
            var localVarPath = "/metadata/taskdefs";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (access != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "access", access)); // query parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter
            if (tagKey != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagKey", tagKey)); // query parameter
            if (tagValue != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagValue", tagValue)); // query parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<List<TaskDef>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetTaskDefs")).Data;
        }

        /// <summary>
        /// Gets all task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="access"> (optional, default to READ)</param>
        /// <param name="metadata"> (optional, default to false)</param>
        /// <param name="tagKey"> (optional)</param>
        /// <param name="tagValue"> (optional)</param>
        /// <returns>ApiResponse of List&lt;TaskDef&gt;</returns>
        public ApiResponse<List<TaskDef>> GetTaskDefsWithHttpInfo(string access = null, bool? metadata = null, string tagKey = null, string tagValue = null)
        {

            var localVarPath = "/metadata/taskdefs";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (access != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "access", access)); // query parameter
            if (metadata != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "metadata", metadata)); // query parameter
            if (tagKey != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagKey", tagKey)); // query parameter
            if (tagValue != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "tagValue", tagValue)); // query parameter

            return this.Configuration.ApiClient.Execute<List<TaskDef>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetTaskDefs");
        }

        /// <summary>
        /// Create or update task definition(s) 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        public Object RegisterTaskDef(List<TaskDef> body)
        {
            ApiResponse<Object> localVarResponse = RegisterTaskDefWithHttpInfo(body);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Create or update task definition(s) 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> RegisterTaskDefAsync(List<TaskDef> body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->RegisterTaskDef");

            var localVarPath = "/metadata/taskdefs";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RegisterTaskDef")).Data;
        }

        /// <summary>
        /// Create or update task definition(s) 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> RegisterTaskDefWithHttpInfo(List<TaskDef> body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->RegisterTaskDef");

            var localVarPath = "/metadata/taskdefs";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

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
                ExceptionFactory, "RegisterTaskDef");
        }

        /// <summary>
        /// Remove a task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        public void UnregisterTaskDef(string tasktype)
        {
            UnregisterTaskDefWithHttpInfo(tasktype);
        }

        /// <summary>
        /// Asynchronous Remove a task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        public async ThreadTask.Task UnregisterTaskDefAsync(string tasktype)
        {
            // verify the required parameter 'tasktype' is set
            if (tasktype == null)
                throw new ApiException(400, "Missing required parameter 'tasktype' when calling MetadataResourceApi->UnregisterTaskDef");

            var localVarPath = "/metadata/taskdefs/{tasktype}";
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
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (tasktype != null) localVarPathParams.Add("tasktype", this.Configuration.ApiClient.ParameterToString(tasktype)); // path parameter

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UnregisterTaskDef");
        }

        /// <summary>
        /// Remove a task definition 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> UnregisterTaskDefWithHttpInfo(string tasktype)
        {
            // verify the required parameter 'tasktype' is set
            if (tasktype == null)
                throw new ApiException(400, "Missing required parameter 'tasktype' when calling MetadataResourceApi->UnregisterTaskDef");

            var localVarPath = "/metadata/taskdefs/{tasktype}";
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
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (tasktype != null) localVarPathParams.Add("tasktype", this.Configuration.ApiClient.ParameterToString(tasktype)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UnregisterTaskDef");
        }

        /// <summary>
        /// Removes workflow definition. It does not remove workflows associated with the definition. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public void UnregisterWorkflowDef(string name, int? version)
        {
            UnregisterWorkflowDefWithHttpInfo(name, version);
        }

        /// <summary>
        /// Asynchronous Removes workflow definition. It does not remove workflows associated with the definition. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async ThreadTask.Task UnregisterWorkflowDefAsync(string name, int? version)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling MetadataResourceApi->UnregisterWorkflowDef");
            // verify the required parameter 'version' is set
            if (version == null)
                throw new ApiException(400, "Missing required parameter 'version' when calling MetadataResourceApi->UnregisterWorkflowDef");

            var localVarPath = "/metadata/workflow/{name}/{version}";
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
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (name != null) localVarPathParams.Add("name", this.Configuration.ApiClient.ParameterToString(name)); // path parameter
            if (version != null) localVarPathParams.Add("version", this.Configuration.ApiClient.ParameterToString(version)); // path parameter

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UnregisterWorkflowDef");
        }

        /// <summary>
        /// Removes workflow definition. It does not remove workflows associated with the definition. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> UnregisterWorkflowDefWithHttpInfo(string name, int? version)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling MetadataResourceApi->UnregisterWorkflowDef");
            // verify the required parameter 'version' is set
            if (version == null)
                throw new ApiException(400, "Missing required parameter 'version' when calling MetadataResourceApi->UnregisterWorkflowDef");

            var localVarPath = "/metadata/workflow/{name}/{version}";
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
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (name != null) localVarPathParams.Add("name", this.Configuration.ApiClient.ParameterToString(name)); // path parameter
            if (version != null) localVarPathParams.Add("version", this.Configuration.ApiClient.ParameterToString(version)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UnregisterWorkflowDef");
        }

        /// <summary>
        /// Create or update workflow definition(s) 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to true)</param>
        /// <returns>Object</returns>
        public Object UpdateWorkflowDefinitions(List<WorkflowDef> body, bool? overwrite = null)
        {
            ApiResponse<Object> localVarResponse = UpdateWithHttpInfo(body, overwrite);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Create or update workflow definition(s) 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to true)</param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> UpdateWorkflowDefinitionsAsync(List<WorkflowDef> body, bool? overwrite = null)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->Update");

            var localVarPath = "/metadata/workflow";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (overwrite != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "overwrite", overwrite)); // query parameter
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
                ExceptionFactory, "Update")).Data;
        }

        /// <summary>
        /// Create or update workflow definition(s) 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="overwrite"> (optional, default to true)</param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> UpdateWithHttpInfo(List<WorkflowDef> body, bool? overwrite = null)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->Update");

            var localVarPath = "/metadata/workflow";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (overwrite != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "overwrite", overwrite)); // query parameter
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
                ExceptionFactory, "Update");
        }

        /// <summary>
        /// Update an existing task 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        public Object UpdateTaskDef(TaskDef body)
        {
            ApiResponse<Object> localVarResponse = UpdateTaskDefWithHttpInfo(body);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Update an existing task 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> UpdateTaskDefAsync(TaskDef body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->UpdateTaskDef");

            var localVarPath = "/metadata/taskdefs";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

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
                ExceptionFactory, "UpdateTaskDef")).Data;
        }

        /// <summary>
        /// Update an existing task 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> UpdateTaskDefWithHttpInfo(TaskDef body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling MetadataResourceApi->UpdateTaskDef");

            var localVarPath = "/metadata/taskdefs";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

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
                ExceptionFactory, "UpdateTaskDef");
        }

        /// <summary>
        /// Upload all workflows and tasks definitions to S3 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        public Object UploadWorkflowsAndTasksDefinitionsToS3()
        {
            ApiResponse<Object> localVarResponse = UploadWorkflowsAndTasksDefinitionsToS3WithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Upload all workflows and tasks definitions to S3 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> UploadWorkflowsAndTasksDefinitionsToS3Async()
        {
            var localVarPath = "/metadata/workflow-task-defs/upload";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UploadWorkflowsAndTasksDefinitionsToS3")).Data;
        }

        /// <summary>
        /// Upload all workflows and tasks definitions to S3 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> UploadWorkflowsAndTasksDefinitionsToS3WithHttpInfo()
        {

            var localVarPath = "/metadata/workflow-task-defs/upload";
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
      "*/*"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UploadWorkflowsAndTasksDefinitionsToS3");
        }

    }
}
