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
using Conductor.Api;
using Conductor.Client.Authentication;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System;
using RestSharp;
using Newtonsoft.Json;
using System.Linq;

namespace Conductor.Client
{
    /// <summary>
    /// Represents a set of configuration settings
    /// </summary>
    public class Configuration : IReadableConfiguration
    {
        #region Constants

        /// <summary>
        /// Identifier for ISO 8601 DateTime Format
        /// </summary>
        /// <remarks>See https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8 for more information.</remarks>
        // ReSharper disable once InconsistentNaming
        public const string ISO8601_DATETIME_FORMAT = "o";

        #endregion Constants

        #region Static Members

        private static readonly object GlobalConfigSync = new { };
        private static Configuration _globalConfiguration;

        /// <summary>
        /// Default creation of exceptions for a given method name and response object
        /// </summary>
        public static readonly ExceptionFactory DefaultExceptionFactory = (methodName, response) =>
        {
            var status = (int)response.StatusCode;
            if (status >= 400)
            {
                return new ApiException(status,
                    string.Format("Error calling {0}: {1}", methodName, response.Content),
                    response.Content);
            }
            if (status == 0)
            {
                return new ApiException(status,
                    string.Format("Error calling {0}: {1}", methodName, response.ErrorMessage), response.ErrorMessage);
            }
            return null;
        };

        /// <summary>
        /// Gets or sets the default Configuration.
        /// </summary>
        /// <value>Configuration.</value>
        public static Configuration Default
        {
            get { return _globalConfiguration; }
            set
            {
                lock (GlobalConfigSync)
                {
                    _globalConfiguration = value;
                }
            }
        }

        #endregion Static Members

        #region Private Members

        private string _dateTimeFormat = ISO8601_DATETIME_FORMAT;
        private string _tempFolderPath = Path.GetTempPath();

        private readonly TokenHandler _tokenHandler = new TokenHandler();

        private TokenResourceApi _tokenClient = null;
        private int Timeout = 30000;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class
        /// </summary>
        public Configuration(int? timeOut = null)
        {
            Timeout = timeOut ?? Timeout;
            ApiClient = new ApiClient(Timeout);
            BasePath = "https://play.orkes.io/api";
            DefaultHeader = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class
        /// </summary>
        /// <param name="serializerSettings">JsonSerializerSettings setting custom serialization properties</param>
        /// <param name="timeOut">Optional rest client request time out</param>
        public Configuration(JsonSerializerSettings serializerSettings, int? timeOut = null)
        {
            Timeout = timeOut ?? Timeout;
            ApiClient = new ApiClient(Timeout);
            ApiClient.serializerSettings = serializerSettings;
            BasePath = "https://play.orkes.io/api";
            DefaultHeader = new ConcurrentDictionary<string, string>();
        }

        #endregion Constructors

        #region Properties

        public readonly ApiClient ApiClient;

        public OrkesAuthenticationSettings AuthenticationSettings { get; set; }

        /// <summary>
        /// Interceptors called before each HTTP request and after each response.
        /// Add implementations of <see cref="IConductorInterceptor"/> to hook into the request lifecycle.
        /// </summary>
        public List<IConductorInterceptor> Interceptors { get; } = new List<IConductorInterceptor>();

        /// <summary>
        /// Gets or sets the base path for API access.
        /// </summary>
        public string BasePath
        {
            get
            {
                return ApiClient.RestClient.Options.BaseUrl.ToString();
            }
            set
            {
                ApiClient.RestClient = new RestClient(new RestClientOptions() { BaseUrl = new Uri(value), MaxTimeout = Timeout });
            }
        }

        /// <summary>
        /// Gets or sets the default header.
        /// </summary>
        public IDictionary<string, string> DefaultHeader { get; set; } = null;

        public string AccessToken
        {
            get
            {
                if (AuthenticationSettings is null)
                {
                    return null;
                }

                if (_tokenClient == null)
                {
                    _tokenClient = GetClient<TokenResourceApi>();
                }
                return _tokenHandler.GetToken(AuthenticationSettings, _tokenClient);
            }
        }

        /// <summary>
        /// Gets or sets the temporary folder path to store the files downloaded from the server.
        /// </summary>
        /// <value>Folder path.</value>
        public virtual string TempFolderPath
        {
            get { return _tempFolderPath; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _tempFolderPath = Path.GetTempPath();
                    return;
                }

                // create the directory if it does not exist
                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }

                // check if the path contains directory separator at the end
                if (value[value.Length - 1] == Path.DirectorySeparatorChar)
                {
                    _tempFolderPath = value;
                }
                else
                {
                    _tempFolderPath = value + Path.DirectorySeparatorChar;
                }
            }
        }

        /// <summary>
        /// Gets or sets the the date time format used when serializing in the ApiClient
        /// By default, it's set to ISO 8601 - "o", for others see:
        /// https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
        /// and https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
        /// No validation is done to ensure that the string you're providing is valid
        /// </summary>
        /// <value>The DateTimeFormat string</value>
        public virtual string DateTimeFormat
        {
            get { return _dateTimeFormat; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    // Never allow a blank or null string, go back to the default
                    _dateTimeFormat = ISO8601_DATETIME_FORMAT;
                    return;
                }

                // Caution, no validation when you choose date time format other than ISO 8601
                // Take a look at the above links
                _dateTimeFormat = value;
            }
        }


        #endregion Properties

        #region Methods

        /// <summary>
        /// Add default header.
        /// </summary>
        /// <param name="key">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <returns></returns>
        public void AddDefaultHeader(string key, string value)
        {
            DefaultHeader[key] = value;
        }

        public T GetClient<T>() where T : IApiAccessor, new()
        {
            T client = new T();
            client.Configuration = this;
            return client;
        }

        /// <summary>
        /// Get the refresh token
        /// </summary>
        /// <returns> new token</returns>
        public string GetRefreshToken()
        {
            _tokenClient = GetClient<TokenResourceApi>();
            return _tokenHandler.RefreshToken(AuthenticationSettings, _tokenClient);
        }

        #endregion Methods
    }
}
