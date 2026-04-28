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
using RestSharp;

namespace Conductor.Client
{
    /// <summary>
    /// Interceptor interface for hooking into HTTP request/response lifecycle.
    /// Register implementations via <see cref="Configuration.Interceptors"/>.
    ///
    /// Example use cases: custom headers, request/response logging, metrics, retry logic.
    /// </summary>
    public interface IConductorInterceptor
    {
        /// <summary>
        /// Called before each HTTP request is sent. Can modify the request (e.g., add headers).
        /// </summary>
        void BeforeRequest(RestRequest request);

        /// <summary>
        /// Called after each HTTP response is received. Can inspect status codes, log, etc.
        /// </summary>
        void AfterResponse(RestRequest request, RestResponse response);
    }
}
