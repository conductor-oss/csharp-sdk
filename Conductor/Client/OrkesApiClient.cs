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
using Conductor.Client.Authentication;

namespace Conductor.Client
{
    public class OrkesApiClient
    {
        private Configuration _configuration = null;

        public OrkesApiClient(Configuration configuration, OrkesAuthenticationSettings orkesAuthenticationSettings)
        {
            _configuration = configuration;
            if (orkesAuthenticationSettings != null)
            {
                configuration.AuthenticationSettings = orkesAuthenticationSettings;
            }
        }

        /// <summary>
        /// The <see cref="Configuration"/> backing this client — exposed so
        /// extension points (e.g. the agent SDK's <c>GetAgentClient()</c>) can
        /// build additional domain clients that share this one's token cache.
        /// </summary>
        public Configuration Configuration => _configuration;

        public T GetClient<T>() where T : IApiAccessor, new()
        {
            return _configuration.GetClient<T>();
        }
    }
}
