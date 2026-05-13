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
using EventHandler = Conductor.Client.Models.EventHandler;
using ThreadTask = System.Threading.Tasks;
using conductor_csharp.Api;


namespace Conductor.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class EventResourceApi : IEventResourceApi
    {
        private Conductor.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventResourceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public EventResourceApi(String basePath)
        {
            this.Configuration = new Conductor.Client.Configuration { BasePath = basePath };

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventResourceApi"/> class
        /// </summary>
        /// <returns></returns>
        public EventResourceApi()
        {
            this.Configuration = Conductor.Client.Configuration.Default;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventResourceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public EventResourceApi(Conductor.Client.Configuration configuration = null)
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
        /// Add a new event handler. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        public void AddEventHandler(EventHandler body)
        {
            AddEventHandlerWithHttpInfo(body);
        }

        /// <summary>
        /// Asynchronous Add a new event handler. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        public async ThreadTask.Task AddEventHandlerAsync(EventHandler body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EventResourceApi->AddEventHandler");

            var localVarPath = "/event";
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
                ExceptionFactory, "AddEventHandler");
        }

        /// <summary>
        /// Add a new event handler. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> AddEventHandlerWithHttpInfo(EventHandler body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EventResourceApi->AddEventHandler");

            var localVarPath = "/event";
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
                ExceptionFactory, "AddEventHandler");
        }

        /// <summary>
        /// Delete queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public void DeleteQueueConfig(string queueType, string queueName)
        {
            DeleteQueueConfigWithHttpInfo(queueType, queueName);
        }

        /// <summary>
        /// Asynchronous Delete queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public async ThreadTask.Task DeleteQueueConfigAsync(string queueType, string queueName)
        {
            // verify the required parameter 'queueType' is set
            if (queueType == null)
                throw new ApiException(400, "Missing required parameter 'queueType' when calling EventResourceApi->DeleteQueueConfig");
            // verify the required parameter 'queueName' is set
            if (queueName == null)
                throw new ApiException(400, "Missing required parameter 'queueName' when calling EventResourceApi->DeleteQueueConfig");

            var localVarPath = "/event/queue/config/{queueType}/{queueName}";
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

            if (queueType != null) localVarPathParams.Add("queueType", this.Configuration.ApiClient.ParameterToString(queueType)); // path parameter
            if (queueName != null) localVarPathParams.Add("queueName", this.Configuration.ApiClient.ParameterToString(queueName)); // path parameter

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "DeleteQueueConfig");
        }

        /// <summary>
        /// Delete queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> DeleteQueueConfigWithHttpInfo(string queueType, string queueName)
        {
            // verify the required parameter 'queueType' is set
            if (queueType == null)
                throw new ApiException(400, "Missing required parameter 'queueType' when calling EventResourceApi->DeleteQueueConfig");
            // verify the required parameter 'queueName' is set
            if (queueName == null)
                throw new ApiException(400, "Missing required parameter 'queueName' when calling EventResourceApi->DeleteQueueConfig");

            var localVarPath = "/event/queue/config/{queueType}/{queueName}";
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

            if (queueType != null) localVarPathParams.Add("queueType", this.Configuration.ApiClient.ParameterToString(queueType)); // path parameter
            if (queueName != null) localVarPathParams.Add("queueName", this.Configuration.ApiClient.ParameterToString(queueName)); // path parameter

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "DeleteQueueConfig");
        }

        /// <summary>
        /// Get all the event handlers 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;EventHandler&gt;</returns>
        public List<EventHandler> GetEventHandlers()
        {
            ApiResponse<List<EventHandler>> localVarResponse = GetEventHandlersWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get all the event handlers 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;EventHandler&gt;</returns>
        public async ThreadTask.Task<List<EventHandler>> GetEventHandlersAsync()
        {
            var localVarPath = "/event";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<List<EventHandler>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetEventHandlers")).Data;
        }

        /// <summary>
        /// Get all the event handlers 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;EventHandler&gt;</returns>
        public ApiResponse<List<EventHandler>> GetEventHandlersWithHttpInfo()
        {

            var localVarPath = "/event";
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

            return this.Configuration.ApiClient.Execute<List<EventHandler>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetEventHandlers");
        }

        /// <summary>
        /// Get event handlers for a given event 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="_event"></param>
        /// <param name="activeOnly"> (optional, default to true)</param>
        /// <returns>List&lt;EventHandler&gt;</returns>
        public List<EventHandler> GetEventHandlersForEvent(string _event, bool? activeOnly = null)
        {
            ApiResponse<List<EventHandler>> localVarResponse = GetEventHandlersForEventWithHttpInfo(_event, activeOnly);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get event handlers for a given event 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="_event"></param>
        /// <param name="activeOnly"> (optional, default to true)</param>
        /// <returns>List&lt;EventHandler&gt;</returns>
        public async ThreadTask.Task<List<EventHandler>> GetEventHandlersForEventAsync(string _event, bool? activeOnly = null)
        {
            // verify the required parameter '_event' is set
            if (_event == null)
                throw new ApiException(400, "Missing required parameter '_event' when calling EventResourceApi->GetEventHandlersForEvent");

            var localVarPath = "/event/{event}";
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

            if (_event != null) localVarPathParams.Add("event", this.Configuration.ApiClient.ParameterToString(_event)); // path parameter
            if (activeOnly != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "activeOnly", activeOnly)); // query parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<List<EventHandler>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetEventHandlersForEvent")).Data;
        }

        /// <summary>
        /// Get event handlers for a given event 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="_event"></param>
        /// <param name="activeOnly"> (optional, default to true)</param>
        /// <returns>ApiResponse of List&lt;EventHandler&gt;</returns>
        public ApiResponse<List<EventHandler>> GetEventHandlersForEventWithHttpInfo(string _event, bool? activeOnly = null)
        {
            // verify the required parameter '_event' is set
            if (_event == null)
                throw new ApiException(400, "Missing required parameter '_event' when calling EventResourceApi->GetEventHandlersForEvent");

            var localVarPath = "/event/{event}";
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

            if (_event != null) localVarPathParams.Add("event", this.Configuration.ApiClient.ParameterToString(_event)); // path parameter
            if (activeOnly != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "activeOnly", activeOnly)); // query parameter

            return this.Configuration.ApiClient.Execute<List<EventHandler>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetEventHandlersForEvent");
        }

        /// <summary>
        /// Get queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        public Dictionary<string, Object> GetQueueConfig(string queueType, string queueName)
        {
            ApiResponse<Dictionary<string, Object>> localVarResponse = GetQueueConfigWithHttpInfo(queueType, queueName);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        public async ThreadTask.Task<Dictionary<string, Object>> GetQueueConfigAsync(string queueType, string queueName)
        {
            // verify the required parameter 'queueType' is set
            if (queueType == null)
                throw new ApiException(400, "Missing required parameter 'queueType' when calling EventResourceApi->GetQueueConfig");
            // verify the required parameter 'queueName' is set
            if (queueName == null)
                throw new ApiException(400, "Missing required parameter 'queueName' when calling EventResourceApi->GetQueueConfig");

            var localVarPath = "/event/queue/config/{queueType}/{queueName}";
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

            if (queueType != null) localVarPathParams.Add("queueType", this.Configuration.ApiClient.ParameterToString(queueType)); // path parameter
            if (queueName != null) localVarPathParams.Add("queueName", this.Configuration.ApiClient.ParameterToString(queueName)); // path parameter

            return (await this.Configuration.ApiClient.ExecuteAsync<Dictionary<string, Object>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetQueueConfig")).Data;
        }

        /// <summary>
        /// Get queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>ApiResponse of Dictionary&lt;string, Object&gt;</returns>
        public ApiResponse<Dictionary<string, Object>> GetQueueConfigWithHttpInfo(string queueType, string queueName)
        {
            // verify the required parameter 'queueType' is set
            if (queueType == null)
                throw new ApiException(400, "Missing required parameter 'queueType' when calling EventResourceApi->GetQueueConfig");
            // verify the required parameter 'queueName' is set
            if (queueName == null)
                throw new ApiException(400, "Missing required parameter 'queueName' when calling EventResourceApi->GetQueueConfig");

            var localVarPath = "/event/queue/config/{queueType}/{queueName}";
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

            if (queueType != null) localVarPathParams.Add("queueType", this.Configuration.ApiClient.ParameterToString(queueType)); // path parameter
            if (queueName != null) localVarPathParams.Add("queueName", this.Configuration.ApiClient.ParameterToString(queueName)); // path parameter

            return this.Configuration.ApiClient.Execute<Dictionary<string, Object>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetQueueConfig");
        }

        /// <summary>
        /// Get all queue configs 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, string&gt;</returns>
        public Dictionary<string, string> GetQueueNames()
        {
            ApiResponse<Dictionary<string, string>> localVarResponse = GetQueueNamesWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get all queue configs 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, string&gt;</returns>
        public async ThreadTask.Task<Dictionary<string, string>> GetQueueNamesAsync()
        {
            var localVarPath = "/event/queue/config";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<Dictionary<string, string>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetQueueNames")).Data;
        }

        /// <summary>
        /// Get all queue configs 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Dictionary&lt;string, string&gt;</returns>
        public ApiResponse<Dictionary<string, string>> GetQueueNamesWithHttpInfo()
        {

            var localVarPath = "/event/queue/config";
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

            return this.Configuration.ApiClient.Execute<Dictionary<string, string>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetQueueNames");
        }

        /// <summary>
        /// Create or update queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public void PutQueueConfig(string body, string queueType, string queueName)
        {
            PutQueueConfigWithHttpInfo(body, queueType, queueName);
        }

        /// <summary>
        /// Asynchronous Create or update queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public async ThreadTask.Task PutQueueConfigAsync(string body, string queueType, string queueName)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EventResourceApi->PutQueueConfig");
            // verify the required parameter 'queueType' is set
            if (queueType == null)
                throw new ApiException(400, "Missing required parameter 'queueType' when calling EventResourceApi->PutQueueConfig");
            // verify the required parameter 'queueName' is set
            if (queueName == null)
                throw new ApiException(400, "Missing required parameter 'queueName' when calling EventResourceApi->PutQueueConfig");

            var localVarPath = "/event/queue/config/{queueType}/{queueName}";
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

            if (queueType != null) localVarPathParams.Add("queueType", this.Configuration.ApiClient.ParameterToString(queueType)); // path parameter
            if (queueName != null) localVarPathParams.Add("queueName", this.Configuration.ApiClient.ParameterToString(queueName)); // path parameter
            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Put, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "PutQueueConfig");
        }

        /// <summary>
        /// Create or update queue config by name 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> PutQueueConfigWithHttpInfo(string body, string queueType, string queueName)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EventResourceApi->PutQueueConfig");
            // verify the required parameter 'queueType' is set
            if (queueType == null)
                throw new ApiException(400, "Missing required parameter 'queueType' when calling EventResourceApi->PutQueueConfig");
            // verify the required parameter 'queueName' is set
            if (queueName == null)
                throw new ApiException(400, "Missing required parameter 'queueName' when calling EventResourceApi->PutQueueConfig");

            var localVarPath = "/event/queue/config/{queueType}/{queueName}";
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

            if (queueType != null) localVarPathParams.Add("queueType", this.Configuration.ApiClient.ParameterToString(queueType)); // path parameter
            if (queueName != null) localVarPathParams.Add("queueName", this.Configuration.ApiClient.ParameterToString(queueName)); // path parameter
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
                ExceptionFactory, "PutQueueConfig");
        }

        /// <summary>
        /// Remove an event handler 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns></returns>
        public void RemoveEventHandlerStatus(string name)
        {
            RemoveEventHandlerStatusWithHttpInfo(name);
        }

        /// <summary>
        /// Asynchronous Remove an event handler 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns></returns>
        public async ThreadTask.Task RemoveEventHandlerStatusAsync(string name)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling EventResourceApi->RemoveEventHandlerStatus");

            var localVarPath = "/event/{name}";
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

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RemoveEventHandlerStatus");
        }

        /// <summary>
        /// Remove an event handler 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> RemoveEventHandlerStatusWithHttpInfo(string name)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling EventResourceApi->RemoveEventHandlerStatus");

            var localVarPath = "/event/{name}";
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

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "RemoveEventHandlerStatus");
        }

        /// <summary>
        /// Update an existing event handler. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        public void UpdateEventHandler(EventHandler body)
        {
            UpdateEventHandlerWithHttpInfo(body);
        }

        /// <summary>
        /// Asynchronous Update an existing event handler. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        public async ThreadTask.Task UpdateEventHandlerAsync(EventHandler body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EventResourceApi->UpdateEventHandler");

            var localVarPath = "/event";
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

            if (body != null && body.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(body); // http body (model) parameter
            }
            else
            {
                localVarPostBody = body; // byte array
            }

            await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Put, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "UpdateEventHandler");
        }

        /// <summary>
        /// Update an existing event handler. 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> UpdateEventHandlerWithHttpInfo(EventHandler body)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EventResourceApi->UpdateEventHandler");

            var localVarPath = "/event";
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
                ExceptionFactory, "UpdateEventHandler");
        }
    }
}
