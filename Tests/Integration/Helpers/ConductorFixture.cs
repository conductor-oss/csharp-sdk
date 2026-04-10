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
using Conductor.Client.Authentication;
using Xunit;
using SysEnv = System.Environment;

namespace Tests.Integration.Helpers
{
    [CollectionDefinition("Integration")]
    public class IntegrationCollection : ICollectionFixture<ConductorFixture> { }

    public class ConductorFixture
    {
        public Configuration Configuration { get; }

        public ConductorFixture()
        {
            var serverUrl = SysEnv.GetEnvironmentVariable("CONDUCTOR_SERVER_URL")
                ?? throw new System.InvalidOperationException("CONDUCTOR_SERVER_URL environment variable is not set.");

            var keyId = SysEnv.GetEnvironmentVariable("CONDUCTOR_AUTH_KEY");
            var secret = SysEnv.GetEnvironmentVariable("CONDUCTOR_AUTH_SECRET");

            Configuration = new Configuration
            {
                BasePath = serverUrl,
                AuthenticationSettings = !string.IsNullOrEmpty(keyId) && !string.IsNullOrEmpty(secret)
                    ? new OrkesAuthenticationSettings(keyId, secret)
                    : null
            };
        }
    }
}
