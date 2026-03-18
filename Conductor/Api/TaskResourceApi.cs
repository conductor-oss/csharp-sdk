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
    public partial class TaskResourceApi : ITaskResourceApi
    {
        private Conductor.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResourceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public TaskResourceApi(String basePath)
        {
            this.Configuration = new Conductor.Client.Configuration { BasePath = basePath };

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResourceApi"/> class
        /// </summary>
        /// <returns></returns>
        public TaskResourceApi()
        {
            this.Configuration = Conductor.Client.Configuration.Default;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResourceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public TaskResourceApi(Conductor.Client.Configuration configuration = null)
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
        /// Get the details about each queue 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, long?&gt;</returns>
        public Dictionary<string, long?> All()
        {
            ApiResponse<Dictionary<string, long?>> localVarResponse = AllWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get the details about each queue 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, long?&gt;</returns>
        public async ThreadTask.Task<Dictionary<string, long?>> AllAsync()
        {
            ApiResponse<Dictionary<string, long?>> localVarResponse = await ThreadTask.Task.FromResult(AllWithHttpInfo());
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the details about each queue 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Dictionary&lt;string, long?&gt;</returns>
        public ApiResponse<Dictionary<string, long?>> AllWithHttpInfo()
        {

            var localVarPath = "/tasks/queue/all";
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

            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("All", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Dictionary<string, long?>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (Dictionary<string, long?>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Dictionary<string, long?>)));
        }

        /// <summary>
        /// Get the details about each queue 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Dictionary&lt;string, Dictionary&lt;string, long?&gt;&gt;&gt;</returns>
        public Dictionary<string, Dictionary<string, Dictionary<string, long?>>> AllVerbose()
        {
            ApiResponse<Dictionary<string, Dictionary<string, Dictionary<string, long?>>>> localVarResponse = AllVerboseWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get the details about each queue 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, Dictionary&lt;string, Dictionary&lt;string, long?&gt;&gt;&gt;</returns>
        public async ThreadTask.Task<Dictionary<string, Dictionary<string, Dictionary<string, long?>>>> AllVerboseAsync()
        {
            ApiResponse<Dictionary<string, Dictionary<string, Dictionary<string, long?>>>> localVarResponse = await ThreadTask.Task.FromResult(AllVerboseWithHttpInfo());
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the details about each queue 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Dictionary&lt;string, Dictionary&lt;string, Dictionary&lt;string, long?&gt;&gt;&gt;</returns>
        public ApiResponse<Dictionary<string, Dictionary<string, Dictionary<string, long?>>>> AllVerboseWithHttpInfo()
        {

            var localVarPath = "/tasks/queue/all/verbose";
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

            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("AllVerbose", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Dictionary<string, Dictionary<string, Dictionary<string, long?>>>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (Dictionary<string, Dictionary<string, Dictionary<string, long?>>>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Dictionary<string, Dictionary<string, Dictionary<string, long?>>>)));
        }

        /// <summary>
        /// Batch poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <param name="count"> (optional, default to 1)</param>
        /// <param name="timeout"> (optional, default to 100)</param>
        /// <returns>List&lt;Task&gt;</returns>
        public List<Task> BatchPoll(string tasktype, string workerid = null, string domain = null, int? count = null, int? timeout = null)
        {
            ApiResponse<List<Task>> localVarResponse = BatchPollWithHttpInfo(tasktype, workerid, domain, count, timeout);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Batch poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <param name="count"> (optional, default to 1)</param>
        /// <param name="timeout"> (optional, default to 100)</param>
        /// <returns>List&lt;Task&gt;</returns>
        public async ThreadTask.Task<List<Task>> BatchPollAsync(string tasktype, string workerid = null, string domain = null, int? count = null, int? timeout = null)
        {
            ApiResponse<List<Task>> localVarResponse = await ThreadTask.Task.FromResult(BatchPollWithHttpInfo(tasktype, workerid, domain, count, timeout));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Batch poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <param name="count"> (optional, default to 1)</param>
        /// <param name="timeout"> (optional, default to 100)</param>
        /// <returns>ApiResponse of List&lt;Task&gt;</returns>
        public ApiResponse<List<Task>> BatchPollWithHttpInfo(string tasktype, string workerid = null, string domain = null, int? count = null, int? timeout = null)
        {
            // verify the required parameter 'tasktype' is set
            if (tasktype == null)
                throw new ApiException(400, "Missing required parameter 'tasktype' when calling TaskResourceApi->BatchPoll");

            var localVarPath = "/tasks/poll/batch/{tasktype}";
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
            if (workerid != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerid", workerid)); // query parameter
            if (domain != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "domain", domain)); // query parameter
            if (count != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "count", count)); // query parameter
            if (timeout != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "timeout", timeout)); // query parameter
                                                                                                                                              // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("BatchPoll", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<List<Task>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (List<Task>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<Task>)));
        }

        /// <summary>
        /// Batch poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <param name="count"> (optional, default to 1)</param>
        /// <param name="timeout"> (optional, default to 100)</param>
        /// <returns>Task of List&lt;Task&gt;</returns>

        /// <summary>
        /// Get the last poll data for all task types 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workerSize"> (optional)</param>
        /// <param name="workerOpt"> (optional)</param>
        /// <param name="queueSize"> (optional)</param>
        /// <param name="queueOpt"> (optional)</param>
        /// <param name="lastPollTimeSize"> (optional)</param>
        /// <param name="lastPollTimeOpt"> (optional)</param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        public Dictionary<string, Object> GetAllPollData(long? workerSize = null, string workerOpt = null, long? queueSize = null, string queueOpt = null, long? lastPollTimeSize = null, string lastPollTimeOpt = null)
        {
            ApiResponse<Dictionary<string, Object>> localVarResponse = GetAllPollDataWithHttpInfo(workerSize, workerOpt, queueSize, queueOpt, lastPollTimeSize, lastPollTimeOpt);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get the last poll data for all task types 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workerSize"> (optional)</param>
        /// <param name="workerOpt"> (optional)</param>
        /// <param name="queueSize"> (optional)</param>
        /// <param name="queueOpt"> (optional)</param>
        /// <param name="lastPollTimeSize"> (optional)</param>
        /// <param name="lastPollTimeOpt"> (optional)</param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        public async ThreadTask.Task<Dictionary<string, Object>> GetAllPollDataAsync(long? workerSize = null, string workerOpt = null, long? queueSize = null, string queueOpt = null, long? lastPollTimeSize = null, string lastPollTimeOpt = null)
        {
            ApiResponse<Dictionary<string, Object>> localVarResponse = await ThreadTask.Task.FromResult(GetAllPollDataWithHttpInfo(workerSize, workerOpt, queueSize, queueOpt, lastPollTimeSize, lastPollTimeOpt));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the last poll data for all task types 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="workerSize"> (optional)</param>
        /// <param name="workerOpt"> (optional)</param>
        /// <param name="queueSize"> (optional)</param>
        /// <param name="queueOpt"> (optional)</param>
        /// <param name="lastPollTimeSize"> (optional)</param>
        /// <param name="lastPollTimeOpt"> (optional)</param>
        /// <returns>ApiResponse of Dictionary&lt;string, Object&gt;</returns>
        public ApiResponse<Dictionary<string, Object>> GetAllPollDataWithHttpInfo(long? workerSize = null, string workerOpt = null, long? queueSize = null, string queueOpt = null, long? lastPollTimeSize = null, string lastPollTimeOpt = null)
        {

            var localVarPath = "/tasks/queue/polldata/all";
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

            if (workerSize != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerSize", workerSize)); // query parameter
            if (workerOpt != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerOpt", workerOpt)); // query parameter
            if (queueSize != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "queueSize", queueSize)); // query parameter
            if (queueOpt != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "queueOpt", queueOpt)); // query parameter
            if (lastPollTimeSize != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "lastPollTimeSize", lastPollTimeSize)); // query parameter
            if (lastPollTimeOpt != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "lastPollTimeOpt", lastPollTimeOpt)); // query parameter
                                                                                                                                                                      // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetAllPollData", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Dictionary<string, Object>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (Dictionary<string, Object>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Dictionary<string, Object>)));
        }

        /// <summary>
        /// Get the external uri where the task payload is to be stored 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        /// <param name="payloadType"></param>
        /// <returns>ExternalStorageLocation</returns>
        public ExternalStorageLocation GetExternalStorageLocation(string path, string operation, string payloadType)
        {
            ApiResponse<ExternalStorageLocation> localVarResponse = GetExternalStorageLocationWithHttpInfo(path, operation, payloadType);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get the external uri where the task payload is to be stored 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        /// <param name="payloadType"></param>
        /// <returns>ExternalStorageLocation</returns>
        public async ThreadTask.Task<ExternalStorageLocation> GetExternalStorageLocationAsync(string path, string operation, string payloadType)
        {
            ApiResponse<ExternalStorageLocation> localVarResponse = await ThreadTask.Task.FromResult(GetExternalStorageLocationWithHttpInfo(path, operation, payloadType));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the external uri where the task payload is to be stored 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        /// <param name="payloadType"></param>
        /// <returns>ApiResponse of ExternalStorageLocation</returns>
        public ApiResponse<ExternalStorageLocation> GetExternalStorageLocationWithHttpInfo(string path, string operation, string payloadType)
        {
            // verify the required parameter 'path' is set
            if (path == null)
                throw new ApiException(400, "Missing required parameter 'path' when calling TaskResourceApi->GetExternalStorageLocation");
            // verify the required parameter 'operation' is set
            if (operation == null)
                throw new ApiException(400, "Missing required parameter 'operation' when calling TaskResourceApi->GetExternalStorageLocation");
            // verify the required parameter 'payloadType' is set
            if (payloadType == null)
                throw new ApiException(400, "Missing required parameter 'payloadType' when calling TaskResourceApi->GetExternalStorageLocation");

            var localVarPath = "/tasks/externalstoragelocation";
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

            if (path != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "path", path)); // query parameter
            if (operation != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "operation", operation)); // query parameter
            if (payloadType != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "payloadType", payloadType)); // query parameter
                                                                                                                                                          // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetExternalStorageLocation", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<ExternalStorageLocation>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (ExternalStorageLocation)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(ExternalStorageLocation)));
        }

        /// <summary>
        /// Get the last poll data for a given task type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>List&lt;PollData&gt;</returns>
        public List<PollData> GetPollData(string taskType)
        {
            ApiResponse<List<PollData>> localVarResponse = GetPollDataWithHttpInfo(taskType);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get the last poll data for a given task type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>List&lt;PollData&gt;</returns>
        public async ThreadTask.Task<List<PollData>> GetPollDataAsync(string taskType)
        {
            ApiResponse<List<PollData>> localVarResponse = await ThreadTask.Task.FromResult(GetPollDataWithHttpInfo(taskType));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the last poll data for a given task type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>ApiResponse of List&lt;PollData&gt;</returns>
        public ApiResponse<List<PollData>> GetPollDataWithHttpInfo(string taskType)
        {
            // verify the required parameter 'taskType' is set
            if (taskType == null)
                throw new ApiException(400, "Missing required parameter 'taskType' when calling TaskResourceApi->GetPollData");

            var localVarPath = "/tasks/queue/polldata";
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

            if (taskType != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "taskType", taskType)); // query parameter
                                                                                                                                                 // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetPollData", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<List<PollData>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (List<PollData>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<PollData>)));
        }

        /// <summary>
        /// Get task by Id 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>Task</returns>
        public Task GetTask(string taskId)
        {
            ApiResponse<Task> localVarResponse = GetTaskWithHttpInfo(taskId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get task by Id 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>Task</returns>
        public async ThreadTask.Task<Task> GetTaskAsync(string taskId)
        {
            ApiResponse<Task> localVarResponse = await ThreadTask.Task.FromResult(GetTaskWithHttpInfo(taskId));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get task by Id 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>ApiResponse of Task</returns>
        public ApiResponse<Task> GetTaskWithHttpInfo(string taskId)
        {
            // verify the required parameter 'taskId' is set
            if (taskId == null)
                throw new ApiException(400, "Missing required parameter 'taskId' when calling TaskResourceApi->GetTask");

            var localVarPath = "/tasks/{taskId}";
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

            if (taskId != null) localVarPathParams.Add("taskId", this.Configuration.ApiClient.ParameterToString(taskId)); // path parameter
                                                                                                                          // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetTask", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Task>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (Task)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Task)));
        }

        /// <summary>
        /// Get Task Execution Logs 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>List&lt;TaskExecLog&gt;</returns>
        public List<TaskExecLog> GetTaskLogs(string taskId)
        {
            ApiResponse<List<TaskExecLog>> localVarResponse = GetTaskLogsWithHttpInfo(taskId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get Task Execution Logs 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>List&lt;TaskExecLog&gt;</returns>
        public async ThreadTask.Task<List<TaskExecLog>> GetTaskLogsAsync(string taskId)
        {
            ApiResponse<List<TaskExecLog>> localVarResponse = await ThreadTask.Task.FromResult(GetTaskLogsWithHttpInfo(taskId));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Task Execution Logs 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskId"></param>
        /// <returns>ApiResponse of List&lt;TaskExecLog&gt;</returns>
        public ApiResponse<List<TaskExecLog>> GetTaskLogsWithHttpInfo(string taskId)
        {
            // verify the required parameter 'taskId' is set
            if (taskId == null)
                throw new ApiException(400, "Missing required parameter 'taskId' when calling TaskResourceApi->GetTaskLogs");

            var localVarPath = "/tasks/{taskId}/log";
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

            if (taskId != null) localVarPathParams.Add("taskId", this.Configuration.ApiClient.ParameterToString(taskId)); // path parameter
                                                                                                                          // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetTaskLogs", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<List<TaskExecLog>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (List<TaskExecLog>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<TaskExecLog>)));
        }

        /// <summary>
        /// Log Task Execution Details 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public void Log(string body, string taskId)
        {
            LogWithHttpInfo(body, taskId);
        }

        /// <summary>
        /// Asynchronous Log Task Execution Details 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async void LogAsync(string body, string taskId)
        {
            await ThreadTask.Task.FromResult(LogWithHttpInfo(body, taskId));
        }

        /// <summary>
        /// Log Task Execution Details 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="taskId"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> LogWithHttpInfo(string body, string taskId)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling TaskResourceApi->Log");
            // verify the required parameter 'taskId' is set
            if (taskId == null)
                throw new ApiException(400, "Missing required parameter 'taskId' when calling TaskResourceApi->Log");

            var localVarPath = "/tasks/{taskId}/log";
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

            if (taskId != null) localVarPathParams.Add("taskId", this.Configuration.ApiClient.ParameterToString(taskId)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }
            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("Log", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Object>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              null);
        }

        /// <summary>
        /// Poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <returns>Task</returns>
        public Task Poll(string tasktype, string workerid = null, string domain = null)
        {
            ApiResponse<Task> localVarResponse = PollWithHttpInfo(tasktype, workerid, domain);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <returns>Task</returns>
        public async ThreadTask.Task<Task> PollAsync(string tasktype, string workerid = null, string domain = null)
        {
            ApiResponse<Task> localVarResponse = await ThreadTask.Task.FromResult(PollWithHttpInfo(tasktype, workerid, domain));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Poll for a task of a certain type 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="tasktype"></param>
        /// <param name="workerid"> (optional)</param>
        /// <param name="domain"> (optional)</param>
        /// <returns>ApiResponse of Task</returns>
        public ApiResponse<Task> PollWithHttpInfo(string tasktype, string workerid = null, string domain = null)
        {
            // verify the required parameter 'tasktype' is set
            if (tasktype == null)
                throw new ApiException(400, "Missing required parameter 'tasktype' when calling TaskResourceApi->Poll");

            var localVarPath = "/tasks/poll/{tasktype}";
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
            if (workerid != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerid", workerid)); // query parameter
            if (domain != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "domain", domain)); // query parameter
                                                                                                                                           // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("Poll", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Task>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (Task)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Task)));
        }

        /// <summary>
        /// Requeue pending tasks 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>string</returns>
        public string RequeuePendingTask(string taskType)
        {
            ApiResponse<string> localVarResponse = RequeuePendingTaskWithHttpInfo(taskType);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Requeue pending tasks 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>string</returns>
        public async ThreadTask.Task<string> RequeuePendingTaskAsync(string taskType)
        {
            ApiResponse<string> localVarResponse = await ThreadTask.Task.FromResult(RequeuePendingTaskWithHttpInfo(taskType));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Requeue pending tasks 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> RequeuePendingTaskWithHttpInfo(string taskType)
        {
            // verify the required parameter 'taskType' is set
            if (taskType == null)
                throw new ApiException(400, "Missing required parameter 'taskType' when calling TaskResourceApi->RequeuePendingTask");

            var localVarPath = "/tasks/queue/requeue/{taskType}";
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
      "text/plain"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (taskType != null) localVarPathParams.Add("taskType", this.Configuration.ApiClient.ParameterToString(taskType)); // path parameter
                                                                                                                                // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("RequeuePendingTask", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// Search for tasks based in payload and other parameters use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTaskSummary</returns>
        public SearchResultTaskSummary Search(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
        {
            ApiResponse<SearchResultTaskSummary> localVarResponse = SearchWithHttpInfo(start, size, sort, freeText, query);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Search for tasks based in payload and other parameters use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTaskSummary</returns>
        public async ThreadTask.Task<SearchResultTaskSummary> SearchAsync(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
        {
            ApiResponse<SearchResultTaskSummary> localVarResponse = await ThreadTask.Task.FromResult(SearchWithHttpInfo(start, size, sort, freeText, query));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Search for tasks based in payload and other parameters use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>ApiResponse of SearchResultTaskSummary</returns>
        public ApiResponse<SearchResultTaskSummary> SearchWithHttpInfo(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
        {

            var localVarPath = "/tasks/search";
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

            if (start != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "start", start)); // query parameter
            if (size != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "size", size)); // query parameter
            if (sort != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "sort", sort)); // query parameter
            if (freeText != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "freeText", freeText)); // query parameter
            if (query != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "query", query)); // query parameter
                                                                                                                                        // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("Search", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<SearchResultTaskSummary>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (SearchResultTaskSummary)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(SearchResultTaskSummary)));
        }

        /// <summary>
        /// Search for tasks based in payload and other parameters use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTask</returns>
        public SearchResultTask SearchV2(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
        {
            ApiResponse<SearchResultTask> localVarResponse = SearchV2WithHttpInfo(start, size, sort, freeText, query);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Search for tasks based in payload and other parameters use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>SearchResultTask</returns>
        public async ThreadTask.Task<SearchResultTask> SearchV2Async(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
        {
            ApiResponse<SearchResultTask> localVarResponse = await ThreadTask.Task.FromResult(SearchV2WithHttpInfo(start, size, sort, freeText, query));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Search for tasks based in payload and other parameters use sort options as sort&#x3D;&lt;field&gt;:ASC|DESC e.g. sort&#x3D;name&amp;sort&#x3D;workflowId:DESC. If order is not specified, defaults to ASC
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="start"> (optional, default to 0)</param>
        /// <param name="size"> (optional, default to 100)</param>
        /// <param name="sort"> (optional)</param>
        /// <param name="freeText"> (optional, default to *)</param>
        /// <param name="query"> (optional)</param>
        /// <returns>ApiResponse of SearchResultTask</returns>
        public ApiResponse<SearchResultTask> SearchV2WithHttpInfo(int? start = null, int? size = null, string sort = null, string freeText = null, string query = null)
        {

            var localVarPath = "/tasks/search-v2";
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

            if (start != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "start", start)); // query parameter
            if (size != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "size", size)); // query parameter
            if (sort != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "sort", sort)); // query parameter
            if (freeText != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "freeText", freeText)); // query parameter
            if (query != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "query", query)); // query parameter
                                                                                                                                        // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SearchV2", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<SearchResultTask>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (SearchResultTask)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(SearchResultTask)));
        }

        /// <summary>
        /// Get Task type queue sizes 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"> (optional)</param>
        /// <returns>Dictionary&lt;string, int?&gt;</returns>
        public Dictionary<string, int?> Size(List<string> taskType = null)
        {
            ApiResponse<Dictionary<string, int?>> localVarResponse = SizeWithHttpInfo(taskType);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get Task type queue sizes 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"> (optional)</param>
        /// <returns>Dictionary&lt;string, int?&gt;</returns>
        public async ThreadTask.Task<Dictionary<string, int?>> SizeAsync(List<string> taskType = null)
        {
            ApiResponse<Dictionary<string, int?>> localVarResponse = await ThreadTask.Task.FromResult(SizeWithHttpInfo(taskType));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get Task type queue sizes 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="taskType"> (optional)</param>
        /// <returns>ApiResponse of Dictionary&lt;string, int?&gt;</returns>
        public ApiResponse<Dictionary<string, int?>> SizeWithHttpInfo(List<string> taskType = null)
        {

            var localVarPath = "/tasks/queue/sizes";
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

            if (taskType != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("multi", "taskType", taskType)); // query parameter
                                                                                                                                                      // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("Size", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Dictionary<string, int?>>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (Dictionary<string, int?>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Dictionary<string, int?>)));
        }

        /// <summary>
        /// Update a task 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>string</returns>
        public string UpdateTask(TaskResult body)
        {
            ApiResponse<string> localVarResponse = UpdateTaskWithHttpInfo(body);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Update a task 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>string</returns>
        public async ThreadTask.Task<string> UpdateTaskAsync(TaskResult body)
        {
            ApiResponse<string> localVarResponse = await ThreadTask.Task.FromResult(UpdateTaskWithHttpInfo(body));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Update a task 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> UpdateTaskWithHttpInfo(TaskResult body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling TaskResourceApi->UpdateTask");

            var localVarPath = "/tasks";
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
      "text/plain"
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
            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("UpdateTask", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// Update a task (v2) — submits the result and returns the next available task in one call.
        /// Uses PUT /tasks/{taskId} which is supported on newer Conductor servers.
        /// Returns null if no next task is available or the server returns a non-task response.
        /// Throws ApiException with ErrorCode 404/405 on older servers (caller should fall back to v1).
        /// </summary>
        public Task UpdateTaskV2(TaskResult body, string workerid = null)
        {
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling TaskResourceApi->UpdateTaskV2");

            var localVarPath = "/tasks/{taskId}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();

            localVarPathParams["taskId"] = this.Configuration.ApiClient.ParameterToString(body.TaskId);
            if (workerid != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerid", workerid));

            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(new[] { "application/json" });
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(new[] { "application/json" });
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            localVarHeaderParams["Content-Type"] = localVarHttpContentType;
            Object localVarPostBody = this.Configuration.ApiClient.Serialize(body);

            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;

            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(
                localVarPath, Method.Put, localVarQueryParams, localVarPostBody,
                localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("UpdateTaskV2", localVarResponse);
                if (exception != null) throw exception;
            }

            if (string.IsNullOrWhiteSpace(localVarResponse.Content) || localVarResponse.Content == "null")
                return null;

            return (Task)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Task));
        }

        public async ThreadTask.Task<Task> UpdateTaskV2Async(TaskResult body, string workerid = null)
        {
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling TaskResourceApi->UpdateTaskV2Async");

            var localVarPath = "/tasks/{taskId}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();

            localVarPathParams["taskId"] = this.Configuration.ApiClient.ParameterToString(body.TaskId);
            if (workerid != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerid", workerid));

            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(new[] { "application/json" });
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(new[] { "application/json" });
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            localVarHeaderParams["Content-Type"] = localVarHttpContentType;
            Object localVarPostBody = this.Configuration.ApiClient.Serialize(body);

            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;

            RestResponse localVarResponse = (RestResponse)await this.Configuration.ApiClient.CallApiAsync(
                localVarPath, Method.Put, localVarQueryParams, localVarPostBody,
                localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("UpdateTaskV2Async", localVarResponse);
                if (exception != null) throw exception;
            }

            if (string.IsNullOrWhiteSpace(localVarResponse.Content) || localVarResponse.Content == "null")
                return null;

            return (Task)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Task));
        }

        /// <summary>
        /// Update a task By Ref Name
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>string</returns>
        public string UpdateTask(Dictionary<string, Object> body, string workflowId, string taskRefName, string status, string workerid = null)
        {
            ApiResponse<string> localVarResponse = UpdateTaskWithHttpInfo(body, workflowId, taskRefName, status, workerid);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Update a task By Ref Name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>string</returns>
        public async ThreadTask.Task<string> UpdateTaskAsync(Dictionary<string, Object> body, string workflowId, string taskRefName, string status, string workerid = null)
        {
            ApiResponse<string> localVarResponse = await ThreadTask.Task.FromResult(UpdateTaskWithHttpInfo(body, workflowId, taskRefName, status, workerid));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Update a task By Ref Name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> UpdateTaskWithHttpInfo(Dictionary<string, Object> body, string workflowId, string taskRefName, string status, string workerid = null)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling TaskResourceApi->UpdateTask");
            // verify the required parameter 'workflowId' is set
            if (workflowId == null)
                throw new ApiException(400, "Missing required parameter 'workflowId' when calling TaskResourceApi->UpdateTask");
            // verify the required parameter 'taskRefName' is set
            if (taskRefName == null)
                throw new ApiException(400, "Missing required parameter 'taskRefName' when calling TaskResourceApi->UpdateTask");
            // verify the required parameter 'status' is set
            if (status == null)
                throw new ApiException(400, "Missing required parameter 'status' when calling TaskResourceApi->UpdateTask");

            var localVarPath = "/tasks/{workflowId}/{taskRefName}/{status}";
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
      "text/plain"
    };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (workerid == null)
            {
                workerid = Environment.MachineName;
            }

            if (workflowId != null) localVarPathParams.Add("workflowId", this.Configuration.ApiClient.ParameterToString(workflowId)); // path parameter
            if (taskRefName != null) localVarPathParams.Add("taskRefName", this.Configuration.ApiClient.ParameterToString(taskRefName)); // path parameter
            if (status != null) localVarPathParams.Add("status", this.Configuration.ApiClient.ParameterToString(status)); // path parameter
            if (workerid != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerid", workerid)); // query parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }
            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
              Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
              localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("UpdateTask", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
              localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
              (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// Update a task By Ref Name, evaluates the workflow and returns the updated workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="output"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>Workflow</returns>
        public Workflow UpdateTaskSync(Dictionary<string, Object> output, string workflowId, string taskRefName,
            TaskResult.StatusEnum status, string workerid = null)
        {
            var response = UpdateTaskSyncWithHttpInfo(output, workflowId, taskRefName, status, workerid);
            return response.Data;
        }

        /// <summary>
        /// Update a task By Ref Name, evaluates the workflow and returns the updated workflow
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="output"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <param name="workerid"> (optional)</param>
        /// <returns>Workflow</returns>
        public async ThreadTask.Task<Workflow> UpdateTaskSyncAsync(Dictionary<string, Object> output, string workflowId,
            string taskRefName, TaskResult.StatusEnum status, string workerid = null)
        {
            ApiResponse<Workflow> localVarResponse = await ThreadTask.Task.FromResult(UpdateTaskSyncWithHttpInfo(output, workflowId, taskRefName, status, workerid));
            return localVarResponse.Data;
        }

        /// <summary>
        /// Update a task By Ref Name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="output"></param>
        /// <param name="workflowId"></param>
        /// <param name="taskRefName"></param>
        /// <param name="status"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<Workflow> UpdateTaskSyncWithHttpInfo(Dictionary<string, Object> output, string workflowId, string taskRefName, TaskResult.StatusEnum status, string workerid = null)
        {
            // verify the required parameter 'body' is set
            if (output == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling TaskResourceApi->UpdateTask");
            // verify the required parameter 'workflowId' is set
            if (workflowId == null)
                throw new ApiException(400, "Missing required parameter 'workflowId' when calling TaskResourceApi->UpdateTask");
            // verify the required parameter 'taskRefName' is set
            if (taskRefName == null)
                throw new ApiException(400, "Missing required parameter 'taskRefName' when calling TaskResourceApi->UpdateTask");
            // verify the required parameter 'status' is set
            if (status == null)
                throw new ApiException(400, "Missing required parameter 'status' when calling TaskResourceApi->UpdateTask");

            var localVarPath = "/tasks/{workflowId}/{taskRefName}/{status}/sync";
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
                "text/plain"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (workerid == null)
            {
                workerid = Environment.MachineName;
            }

            if (workflowId != null) localVarPathParams.Add("workflowId", this.Configuration.ApiClient.ParameterToString(workflowId)); // path parameter
            if (taskRefName != null) localVarPathParams.Add("taskRefName", this.Configuration.ApiClient.ParameterToString(taskRefName)); // path parameter
            if (status != null) localVarPathParams.Add("status", this.Configuration.ApiClient.ParameterToString(status)); // path parameter
            if (workerid != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "workerid", workerid)); // query parameter
            if (output != null && output.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(output); // http body (model) parameter
            }
            else
            {
                localVarPostBody = output; // byte array
            }
            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("UpdateTask", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Workflow>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
                (Workflow)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Workflow)));
        }
    }
}
