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
using Conductor.Client.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace conductor_csharp.test.Api
{
    [Trait("Category", "CloudIntegration")]
    public class EnvironmentResourceApiTest
    {
        private readonly OrkesApiClient _orkesApiClient;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly EnvironmentResourceApi _environmentResourceApi;
        public EnvironmentResourceApiTest(ITestOutputHelper testOutputHelper)
        {
            // dev local testing
            //_orkesApiClient = new OrkesApiClient(new Configuration(), new OrkesAuthenticationSettings(Conductor.Client.Constants.KEY_ID, Conductor.Client.Constants.KEY_SECRET));
            //_environmentResourceApi = _orkesApiClient.GetClient<EnvironmentResourceApi>();

            _testOutputHelper = testOutputHelper;
            _environmentResourceApi = ApiExtensions.GetClient<EnvironmentResourceApi>();
        }

        [Fact]
        public void GetAllEnvironmentVariableTest()
        {
            var result = _environmentResourceApi.GetAll();
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnvironmentVariableByKeyTest()
        {
            CreateEnvironmentVariable(Helper.TestConstants.BODY, Helper.TestConstants.KEY);
            var result = _environmentResourceApi.Get1(Helper.TestConstants.KEY);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            DeleteEnvironmentVariable(Helper.TestConstants.KEY);
        }

        [Fact]
        public void DeleteEnvironmentVariableByKeyTest()
        {
            CreateEnvironmentVariable(Helper.TestConstants.BODY, Helper.TestConstants.KEY);
            DeleteEnvironmentVariable(Helper.TestConstants.KEY);
        }

        [Fact]
        public void CreateOrUpdateEnvVariable()
        {
            CreateEnvironmentVariable(Helper.TestConstants.BODY, Helper.TestConstants.KEY);
            DeleteEnvironmentVariable(Helper.TestConstants.KEY);
        }
        private void CreateEnvironmentVariable(string body, string key)
        {
            _environmentResourceApi.CreateOrUpdateEnvVariable(body, key);
        }

        private void DeleteEnvironmentVariable(string key)
        {
            _environmentResourceApi.DeleteEnvVariable(key);
        }
    }
}
