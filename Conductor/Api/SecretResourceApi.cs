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
    public partial class SecretResourceApi : ISecretResourceApi
    {
        private Conductor.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretResourceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public SecretResourceApi(String basePath)
        {
            this.Configuration = new Conductor.Client.Configuration { BasePath = basePath };

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretResourceApi"/> class
        /// </summary>
        /// <returns></returns>
        public SecretResourceApi()
        {
            this.Configuration = Conductor.Client.Configuration.Default;

            ExceptionFactory = Conductor.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretResourceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public SecretResourceApi(Conductor.Client.Configuration configuration = null)
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
        /// Delete a secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public Object DeleteSecret(string key)
        {
            ApiResponse<Object> localVarResponse = DeleteSecretWithHttpInfo(key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Delete a secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> DeleteSecretAsync(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->DeleteSecret");

            var localVarPath = "/secrets/{key}";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "DeleteSecret")).Data;
        }

        /// <summary>
        /// Delete a secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> DeleteSecretWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->DeleteSecret");

            var localVarPath = "/secrets/{key}";
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

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Delete, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "DeleteSecret");
        }

        /// <summary>
        /// Delete tags of the secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public void DeleteTagForSecret(List<TagObject> body, string key)
        {
            DeleteTagForSecretWithHttpInfo(body, key);
        }

        /// <summary>
        /// Asynchronous Delete tags of the secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async ThreadTask.Task DeleteTagForSecretAsync(List<TagObject> body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling SecretResourceApi->DeleteTagForSecret");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->DeleteTagForSecret");

            var localVarPath = "/secrets/{key}/tags";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                ExceptionFactory, "DeleteTagForSecret");
        }

        /// <summary>
        /// Delete tags of the secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> DeleteTagForSecretWithHttpInfo(List<TagObject> body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling SecretResourceApi->DeleteTagForSecret");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->DeleteTagForSecret");

            var localVarPath = "/secrets/{key}/tags";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                ExceptionFactory, "DeleteTagForSecret");
        }

        /// <summary>
        /// Get secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public Object GetSecret(string key)
        {
            ApiResponse<Object> localVarResponse = GetSecretWithHttpInfo(key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> GetSecretAsync(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->GetSecret");

            var localVarPath = "/secrets/{key}";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetSecret")).Data;
        }

        /// <summary>
        /// Get secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> GetSecretWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->GetSecret");

            var localVarPath = "/secrets/{key}";
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

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetSecret");
        }

        /// <summary>
        /// Get tags by secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        public List<TagObject> GetTags(string key)
        {
            ApiResponse<List<TagObject>> localVarResponse = GetTagsWithHttpInfo(key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Get tags by secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>List&lt;TagObject&gt;</returns>
        public async ThreadTask.Task<List<TagObject>> GetTagsAsync(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->GetTags");

            var localVarPath = "/secrets/{key}/tags";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<List<TagObject>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetTags")).Data;
        }

        /// <summary>
        /// Get tags by secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>ApiResponse of List&lt;TagObject&gt;</returns>
        public ApiResponse<List<TagObject>> GetTagsWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->GetTags");

            var localVarPath = "/secrets/{key}/tags";
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

            return this.Configuration.ApiClient.Execute<List<TagObject>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "GetTags");
        }

        /// <summary>
        /// List all secret names 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        public Object ListAllSecretNames()
        {
            ApiResponse<Object> localVarResponse = ListAllSecretNamesWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous List all secret names 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> ListAllSecretNamesAsync()
        {
            var localVarPath = "/secrets";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListAllSecretNames")).Data;
        }
        /// <summary>
        /// List all secret names 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> ListAllSecretNamesWithHttpInfo()
        {

            var localVarPath = "/secrets";
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

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListAllSecretNames");
        }

        /// <summary>
        /// List all secret names user can grant access to 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;string&gt;</returns>
        public List<string> ListSecretsThatUserCanGrantAccessTo()
        {
            ApiResponse<List<string>> localVarResponse = ListSecretsThatUserCanGrantAccessToWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous List all secret names 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Object</returns>
        public async ThreadTask.Task<List<string>> ListSecretsThatUserCanGrantAccessToAsync()
        {
            var localVarPath = "/secrets";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<List<string>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListSecretsThatUserCanGrantAccessTo")).Data;
        }

        /// <summary>
        /// List all secret names user can grant access to 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;string&gt;</returns>
        public ApiResponse<List<string>> ListSecretsThatUserCanGrantAccessToWithHttpInfo()
        {

            var localVarPath = "/secrets";
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

            return this.Configuration.ApiClient.Execute<List<string>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListSecretsThatUserCanGrantAccessTo");
        }

        /// <summary>
        /// List all secret names along with tags user can grant access to 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;ExtendedSecret&gt;</returns>
        public List<ExtendedSecret> ListSecretsWithTagsThatUserCanGrantAccessTo()
        {
            ApiResponse<List<ExtendedSecret>> localVarResponse = ListSecretsWithTagsThatUserCanGrantAccessToWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous List all secret names along with tags user can grant access to 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;ExtendedSecret&gt;</returns>
        public async ThreadTask.Task<List<ExtendedSecret>> ListSecretsWithTagsThatUserCanGrantAccessToAsync()
        {
            var localVarPath = "/secrets-v2";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<List<ExtendedSecret>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListSecretsWithTagsThatUserCanGrantAccessTo")).Data;
        }

        /// <summary>
        /// List all secret names along with tags user can grant access to 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;ExtendedSecret&gt;</returns>
        public ApiResponse<List<ExtendedSecret>> ListSecretsWithTagsThatUserCanGrantAccessToWithHttpInfo()
        {

            var localVarPath = "/secrets-v2";
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

            return this.Configuration.ApiClient.Execute<List<ExtendedSecret>>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "ListSecretsWithTagsThatUserCanGrantAccessTo");
        }

        /// <summary>
        /// Put a secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public Object PutSecret(string body, string key)
        {
            ApiResponse<Object> localVarResponse = PutSecretWithHttpInfo(body, key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Put a secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> PutSecretAsync(string body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling SecretResourceApi->PutSecret");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->PutSecret");

            var localVarPath = "/secrets/{key}";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                ExceptionFactory, "PutSecret")).Data;
        }

        /// <summary>
        /// Put a secret value by key 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> PutSecretWithHttpInfo(string body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling SecretResourceApi->PutSecret");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->PutSecret");

            var localVarPath = "/secrets/{key}";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                ExceptionFactory, "PutSecret");
        }

        /// <summary>
        /// Tag a secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public void PutTagForSecret(List<TagObject> body, string key)
        {
            PutTagForSecretWithHttpInfo(body, key);
        }

        /// <summary>
        /// Asynchronous Tag a secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async ThreadTask.Task PutTagForSecretAsync(List<TagObject> body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling SecretResourceApi->PutTagForSecret");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->PutTagForSecret");

            var localVarPath = "/secrets/{key}/tags";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                ExceptionFactory, "PutTagForSecret");
        }

        /// <summary>
        /// Tag a secret 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<Object> PutTagForSecretWithHttpInfo(List<TagObject> body, string key)
        {
            // verify the required parameter 'body' is set
            if (body == null)
                throw new ApiException(400, "Missing required parameter 'body' when calling SecretResourceApi->PutTagForSecret");
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->PutTagForSecret");

            var localVarPath = "/secrets/{key}/tags";
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

            if (key != null) localVarPathParams.Add("key", this.Configuration.ApiClient.ParameterToString(key)); // path parameter
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
                ExceptionFactory, "PutTagForSecret");
        }

        /// <summary>
        /// Check if secret exists 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public Object SecretExists(string key)
        {
            ApiResponse<Object> localVarResponse = SecretExistsWithHttpInfo(key);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Asynchronous Check if secret exists 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>Object</returns>
        public async ThreadTask.Task<Object> SecretExistsAsync(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->SecretExists");

            var localVarPath = "/secrets/{key}/exists";
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

            return (await this.Configuration.ApiClient.ExecuteAsync<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "SecretExists")).Data;
        }

        /// <summary>
        /// Check if secret exists 
        /// </summary>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="key"></param>
        /// <returns>ApiResponse of Object</returns>
        public ApiResponse<Object> SecretExistsWithHttpInfo(string key)
        {
            // verify the required parameter 'key' is set
            if (key == null)
                throw new ApiException(400, "Missing required parameter 'key' when calling SecretResourceApi->SecretExists");

            var localVarPath = "/secrets/{key}/exists";
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

            return this.Configuration.ApiClient.Execute<Object>(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams, localVarPathParams, localVarHttpContentType, this.Configuration,
                ExceptionFactory, "SecretExists");
        }
    }
}
