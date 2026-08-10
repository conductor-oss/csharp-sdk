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
using Conductor.Client;
using Conductor.Client.Authentication;
using Xunit;

namespace Tests.Client
{
    public class OrkesApiClientTest
    {
        // BasePath is required because the Configuration constructor initializes a
        // RestClient that validates the URI, but no HTTP calls are made in these tests.
        private const string FakeBasePath = "http://localhost:8080/api";

        [Fact]
        public void GetClient_ReturnsSameConfigurationInstance()
        {
            var configuration = new Configuration { BasePath = FakeBasePath };
            var orkesApiClient = new OrkesApiClient(configuration, null);

            var client = orkesApiClient.GetClient<WorkflowResourceApi>();

            Assert.Same(configuration, client.Configuration);
        }

        [Fact]
        public void GetClient_DifferentApiTypes_AllShareSameConfiguration()
        {
            var configuration = new Configuration { BasePath = FakeBasePath };
            var orkesApiClient = new OrkesApiClient(configuration, null);

            var workflowClient = orkesApiClient.GetClient<WorkflowResourceApi>();
            var metadataClient = orkesApiClient.GetClient<MetadataResourceApi>();
            var taskClient = orkesApiClient.GetClient<TaskResourceApi>();

            Assert.Same(configuration, workflowClient.Configuration);
            Assert.Same(configuration, metadataClient.Configuration);
            Assert.Same(configuration, taskClient.Configuration);
        }

        [Fact]
        public void Constructor_WithAuthSettings_SetsOnConfiguration()
        {
            var configuration = new Configuration { BasePath = FakeBasePath };
            var authSettings = new OrkesAuthenticationSettings("fake-key", "fake-secret");

            _ = new OrkesApiClient(configuration, authSettings);

            Assert.Same(authSettings, configuration.AuthenticationSettings);
        }

        [Fact]
        public void Constructor_WithNullAuthSettings_DoesNotOverwriteExisting()
        {
            var existingAuth = new OrkesAuthenticationSettings("existing-key", "existing-secret");
            var configuration = new Configuration
            {
                BasePath = FakeBasePath,
                AuthenticationSettings = existingAuth
            };

            _ = new OrkesApiClient(configuration, null);

            Assert.Same(existingAuth, configuration.AuthenticationSettings);
        }
    }
}