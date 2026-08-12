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
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conductor.Api
{
    public class EnvironmentResourceApi : IEnvironmentResourceApi
    {
        private Conductor.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentResourceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public EnvironmentResourceApi(String basePath)
        {
            this.Configuration = new Conductor.Client.Configuration { BasePath = basePath };

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentResourceApi"/> class
        /// </summary>
        /// <returns></returns>
        public EnvironmentResourceApi()
        {
            this.Configuration = Conductor.Client.Configuration.Default;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentResourceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public EnvironmentResourceApi(Conductor.Client.Configuration configuration = null)
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
        /// Create or update an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public void CreateOrUpdateEnvVariable(string body, string key)
        {
            CreateOrUpdateEnvVariableWithHttpInfo(body, key);
        }

        /// <summary>
        /// Create or update an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> CreateOrUpdateEnvVariableWithHttpInfo(string body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EnvironmentResourceApi->CreateOrUpdateEnvVariable");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling EnvironmentResourceApi->CreateOrUpdateEnvVariable");

            var localVarPath = "/environment/{key}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
"text/plain"
};
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
};
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
            Method.Put, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
            localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("CreateOrUpdateEnvVariable", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Object>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            null);
        }

        /// <summary>
        /// Create or update an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CreateOrUpdateEnvVariableAsync(string body, string key)
        {
            await CreateOrUpdateEnvVariableAsyncWithHttpInfo(body, key);

        }

        /// <summary>
        /// Create or update an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Object>> CreateOrUpdateEnvVariableAsyncWithHttpInfo(string body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling EnvironmentResourceApi->CreateOrUpdateEnvVariable");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling EnvironmentResourceApi->CreateOrUpdateEnvVariable");

            var localVarPath = "/environment/{key}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
"text/plain"
};
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
};
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
            RestResponse localVarResponse = (RestResponse)await this.Configuration.ApiClient.CallApiAsync(localVarPath,
            Method.Put, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
            localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("CreateOrUpdateEnvVariable", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Object>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            null);
        }

        /// <summary>
        /// Delete an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>string</returns>
        public string DeleteEnvVariable(string key)
        {
            ApiResponse<string> localVarResponse = DeleteEnvVariableWithHttpInfo(key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Delete an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> DeleteEnvVariableWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling EnvironmentResourceApi->DeleteEnvVariable");

            var localVarPath = "/environment/{key}";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
                                                                                                                 // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)this.Configuration.ApiClient.CallApi(localVarPath,
            Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
            localVarPathParams, localVarHttpContentType, this.Configuration);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("DeleteEnvVariable", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// Delete an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Task of string</returns>
        public async System.Threading.Tasks.Task<string> DeleteEnvVariableAsync(string key)
        {
            ApiResponse<string> localVarResponse = await DeleteEnvVariableAsyncWithHttpInfo(key);
            return localVarResponse.Data;

        }

        /// <summary>
        /// Delete an environment variable (requires metadata or admin role)
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<string>> DeleteEnvVariableAsyncWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling EnvironmentResourceApi->DeleteEnvVariable");

            var localVarPath = "/environment/{key}";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
                                                                                                                 // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)await this.Configuration.ApiClient.CallApiAsync(localVarPath,
            Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
            localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("DeleteEnvVariable", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// Get the environment value by key
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>string</returns>
        public string Get1(string key)
        {
            ApiResponse<string> localVarResponse = Get1WithHttpInfo(key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the environment value by key
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> Get1WithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling EnvironmentResourceApi->Get1");

            var localVarPath = "/environment/{key}";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                Exception exception = ExceptionFactory("Get1", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// Get the environment value by key
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Task of string</returns>
        public async System.Threading.Tasks.Task<string> Get1Async(string key)
        {
            ApiResponse<string> localVarResponse = await Get1AsyncWithHttpInfo(key);
            return localVarResponse.Data;

        }

        /// <summary>
        /// Get the environment value by key
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<string>> Get1AsyncWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling EnvironmentResourceApi->Get1");

            var localVarPath = "/environment/{key}";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
                                                                                                                 // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)await this.Configuration.ApiClient.CallApiAsync(localVarPath,
            Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
            localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("Get1", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<string>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            (string)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// List all the environment variables
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, string&gt;</returns>
        public Dictionary<string, string> GetAll()
        {
            ApiResponse<Dictionary<string, string>> localVarResponse = GetAllWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// List all the environment variables
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Dictionary&lt;string, string&gt;</returns>
        public ApiResponse<Dictionary<string, string>> GetAllWithHttpInfo()
        {

            var localVarPath = "/environment";
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
                Exception exception = ExceptionFactory("GetAll", localVarResponse);
                if (exception != null) throw exception;
            }

            var list = (List<EnvironmentVariable>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<EnvironmentVariable>));
            var dictionary = ToEnvironmentDictionary(list);
            return new ApiResponse<Dictionary<string, string>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
                dictionary);
        }

        /// <summary>
        /// List all the environment variables
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of Dictionary&lt;string, string&gt;</returns>
        public async System.Threading.Tasks.Task<Dictionary<string, string>> GetAllAsync()
        {
            ApiResponse<Dictionary<string, string>> localVarResponse = await GetAllAsyncWithHttpInfo();
            return localVarResponse.Data;

        }

        /// <summary>
        /// List all the environment variables
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (Dictionary&lt;string, string&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Dictionary<string, string>>> GetAllAsyncWithHttpInfo()
        {

            var localVarPath = "/environment";
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

            // authentication (api_key) required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["X-Authorization"] = this.Configuration.AccessToken;
            }

            // make the HTTP request
            RestResponse localVarResponse = (RestResponse)await this.Configuration.ApiClient.CallApiAsync(localVarPath,
            Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
            localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetAll", localVarResponse);
                if (exception != null) throw exception;
            }

            var list = (List<EnvironmentVariable>)this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<EnvironmentVariable>));
            var dictionary = ToEnvironmentDictionary(list);
            return new ApiResponse<Dictionary<string, string>>(localVarStatusCode,
            localVarResponse.Headers.ToDictionary(x => x.Name, x => string.Join(",", x.Value)),
            dictionary);
        }

        /// <summary>
        /// Projects a list of environment variables into a name/value dictionary,
        /// skipping any entries that are missing a name or value.
        /// </summary>
        internal static Dictionary<string, string> ToEnvironmentDictionary(List<EnvironmentVariable> variables)
        {
            return variables
                .Where(item => item.Name != null && item.Value != null)
                .ToDictionary(item => item.Name, item => item.Value);
        }
    }
}

