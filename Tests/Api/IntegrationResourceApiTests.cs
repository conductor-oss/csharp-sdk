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
using Conductor.Client.Models;
using conductor_csharp.test.Extensions;
using conductor_csharp.test.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Conductor_csharp.test.Api
{
    /// <summary>
    /// Class for testing IntegrationResourceApi
    /// </summary>
    [Trait("Category", "CloudIntegration")]
    public class IntegrationResourceApiTests : IDisposable
    {
        private readonly IntegrationResourceApi _integrationResourceApi;
        private readonly OrkesApiClient _orkesApiClient;
        private readonly ITestOutputHelper _testOutputHelper;
        private bool _performCleanup = true;

        /// <summary>
        /// Init operation
        /// </summary>
        public IntegrationResourceApiTests(ITestOutputHelper testOutputHelper)
        {
            //dev local testing
            //_orkesApiClient = new OrkesApiClient(new Configuration(), new OrkesAuthenticationSettings(Constants.KEY_ID, Constants.KEY_SECRET));
            //_integrationResourceApi = _orkesApiClient.GetClient<IntegrationResourceApi>();

            _testOutputHelper = testOutputHelper;
            _integrationResourceApi = ApiExtensions.GetClient<IntegrationResourceApi>();
        }

        /// <summary>
        /// Test GetIntegrationProviders
        /// </summary>
        [Fact]
        public void GetIntegrationProvidersTest()
        {
            Setup();
            var response = _integrationResourceApi.GetIntegrationProviders();
            AssertExtensions.AssertModelResponse<List<Integration>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationProviderAsync
        /// </summary>
        [Fact]
        public async void GetIntegrationProvidersAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetIntegrationProvidersAsync();
            AssertExtensions.AssertModelResponse<List<Integration>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationProviderAsync
        /// </summary>
        [Fact]
        public async void GetIntegrationProvidersAsync_WithParametersTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetIntegrationProvidersAsync(TestConstants.Category, true);
            AssertExtensions.AssertModelResponse<List<Integration>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationProvider
        /// </summary>
        [Fact]
        public void GetIntegrationProviderTest()
        {
            Setup();
            var response = _integrationResourceApi.GetIntegrationProvider(TestConstants.IntegrationName);
            AssertExtensions.AssertModelResponse<Integration>(response, TestConstants.IntegrationName, response.Name);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationProviderAsync
        /// </summary>
        [Fact]
        public async void GetIntegrationProviderAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetIntegrationProviderAsync(TestConstants.IntegrationName);
            AssertExtensions.AssertModelResponse<Integration>(response, TestConstants.IntegrationName, response.Name);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationApi
        /// </summary>
        [Fact]
        public void GetIntegrationApiTest()
        {
            Setup();
            var response = _integrationResourceApi.GetIntegrationApi(TestConstants.IntegrationName, TestConstants.ModelName);
            AssertExtensions.AssertModelResponse<IntegrationApi>(response, TestConstants.IntegrationName, response.IntegrationName);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationApiAsync
        /// </summary>
        [Fact]
        public async void GetIntegrationApiAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetIntegrationApiAsync(TestConstants.IntegrationName, TestConstants.ModelName);
            AssertExtensions.AssertModelResponse<IntegrationApi>(response, TestConstants.IntegrationName, response.IntegrationName);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationApis
        /// </summary>
        [Fact]
        public void GetIntegrationApisTest()
        {
            Setup();
            var response = _integrationResourceApi.GetIntegrationApis(TestConstants.IntegrationName);
            AssertExtensions.AssertModelResponse<List<IntegrationApi>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetIntegrationApisAsync
        /// </summary>
        [Fact]
        public async void GetIntegrationApisAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetIntegrationApisAsync(TestConstants.IntegrationName);
            AssertExtensions.AssertModelResponse<List<IntegrationApi>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTokenUsageForIntegration
        /// </summary>
        [Fact]
        public void GetTokenUsageForIntegrationTest()
        {
            Setup();
            var response = _integrationResourceApi.GetTokenUsageForIntegration(TestConstants.IntegrationName, TestConstants.ModelName);
            Assert.IsType<int>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTokenUsageForIntegrationAsync
        /// </summary>
        [Fact]
        public async void GetTokenUsageForIntegrationAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetTokenUsageForIntegrationAsync(TestConstants.IntegrationName, TestConstants.ModelName);
            Assert.IsType<int>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTagsForIntegrationTest
        /// </summary>
        [Fact]
        public void GetTagsForIntegrationTest()
        {
            Setup();
            var response = _integrationResourceApi.GetTagsForIntegration(TestConstants.IntegrationName, TestConstants.ModelName);
            AssertExtensions.AssertModelResponse<List<Tag>>(response);
            _performCleanup = true;
        }

        [Fact]
        public async void GetTagsForIntegrationAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetTagsForIntegrationAsync(TestConstants.IntegrationName, TestConstants.ModelName);
            AssertExtensions.AssertModelResponse<List<Tag>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTagsForIntegrationProvider
        /// </summary>
        [Fact]
        public void GetTagsForIntegrationProviderTest()
        {
            Setup();
            var response = _integrationResourceApi.GetTagsForIntegrationProvider(TestConstants.IntegrationName);
            AssertExtensions.AssertModelResponse<List<Tag>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTagsForIntegrationProviderAsync
        /// </summary>
        [Fact]
        public async void GetTagsForIntegrationProviderAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetTagsForIntegrationProviderAsync(TestConstants.IntegrationName);
            AssertExtensions.AssertModelResponse<List<Tag>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test Delete an Integration
        /// </summary>
        [Fact]
        public void DeleteAnIntegrationTest()
        {
            Setup();
            Assert.Null(Record.Exception(() => _integrationResourceApi.DeleteIntegrationApi(TestConstants.IntegrationName, TestConstants.ModelName)));
            _performCleanup = false;
        }

        /// <summary>
        /// Test DeleteIntegrationApiAsync
        /// </summary>
        [Fact]
        public async void DeleteAnIntegrationAsyncTest()
        {
            Setup();
            var exception = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.DeleteIntegrationApiAsync(TestConstants.IntegrationName, TestConstants.ModelName));
            Assert.Null(exception);
            _performCleanup = false;
        }
        /// <summary>
        /// Test DeleteIntegrationProvider
        /// </summary>
        [Fact]
        public void DeleteIntegrationProviderTest()
        {
            Setup();
            Assert.Null(Record.Exception(() => _integrationResourceApi.DeleteIntegrationProvider(TestConstants.IntegrationName)));
            _performCleanup = false;
        }

        /// <summary>
        /// Test DeleteIntegrationProviderAsync
        /// </summary>
        [Fact]
        public async void DeleteIntegrationProviderAsyncTest()
        {
            Setup();
            var exception = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.DeleteIntegrationProviderAsync(TestConstants.IntegrationName));
            Assert.Null(exception);
            _performCleanup = false;
        }

        /// <summary>
        /// Test GetProvidersAndIntegrations
        /// </summary>
        [Fact]
        public void GetProvidersAndIntegrationsTest()
        {
            Setup();
            var response = _integrationResourceApi.GetProvidersAndIntegrations();
            AssertExtensions.AssertModelResponse<List<string>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetProvidersAndIntegrationsAsync
        /// </summary>
        [Fact]
        public async void GetProvidersAndIntegrationsAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetProvidersAndIntegrationsAsync();
            AssertExtensions.AssertModelResponse<List<string>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetPromptsWithIntegration
        /// </summary>
        [Fact]
        public void GetPromptsWithIntegrationTest()
        {
            Setup();
            var response = _integrationResourceApi.GetPromptsWithIntegration(TestConstants.IntegrationName, TestConstants.ModelName);
            Assert.IsType<List<MessageTemplate>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetPromptsWithIntegrationAsync
        /// </summary>
        [Fact]
        public async void GetPromptsWithIntegrationAsyncTest()
        {
            Setup();
            var response = await _integrationResourceApi.GetPromptsWithIntegrationAsync(TestConstants.IntegrationName, TestConstants.ModelName);
            Assert.IsType<List<MessageTemplate>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test SaveIntegrationProvider and SaveIntegrationApi
        /// </summary>
        [Fact]
        public void SaveIntegrationTest()
        {
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            IntegrationUpdate integrationUpdate = JsonConvert.DeserializeObject<IntegrationUpdate>(jsonObject.integrationUpdate.ToString());
            IntegrationApiUpdate integrationApiUpdate = JsonConvert.DeserializeObject<IntegrationApiUpdate>(jsonObject.integrationApiUpdate.ToString());
            Assert.Null(Record.Exception(() => _integrationResourceApi.SaveIntegrationProvider(integrationUpdate, TestConstants.IntegrationName)));
            Assert.Null(Record.Exception(() => _integrationResourceApi.SaveIntegrationApi(integrationApiUpdate, TestConstants.IntegrationName, TestConstants.ModelName)));
            _performCleanup = true;
        }

        /// <summary>
        /// Test SaveIntegrationProviderAsync and SaveIntegrationApiAsync
        /// </summary>
        [Fact]
        public async void SaveIntegrationAsyncTest()
        {
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            IntegrationUpdate integrationUpdate = JsonConvert.DeserializeObject<IntegrationUpdate>(jsonObject.integrationUpdate.ToString());
            IntegrationApiUpdate integrationApiUpdate = JsonConvert.DeserializeObject<IntegrationApiUpdate>(jsonObject.integrationApiUpdate.ToString());
            var saveIntegrationProviderException = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.SaveIntegrationProviderAsync(integrationUpdate, TestConstants.IntegrationName));
            Assert.Null(saveIntegrationProviderException);

            var saveIntegrationApiException = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.SaveIntegrationApiAsync(integrationApiUpdate, TestConstants.IntegrationName, TestConstants.ModelName));
            Assert.Null(saveIntegrationApiException);

            _performCleanup = true;
        }

        /// <summary>
        /// Test PutTagForIntegration
        /// </summary>
        [Fact]
        public void PutTagForIntegrationTest()
        {
            Setup();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            Assert.Null(Record.Exception(() => _integrationResourceApi.PutTagForIntegration(tagUpdate, TestConstants.IntegrationName, TestConstants.ModelName)));
            _performCleanup = true;
        }

        /// <summary>
        /// Test PutTagForIntegration
        /// </summary>
        [Fact]
        public async void PutTagForIntegrationAsyncTest()
        {
            Setup();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            var tagIntegrationException = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.PutTagForIntegrationAsync(tagUpdate, TestConstants.IntegrationName, TestConstants.ModelName));
            Assert.Null(tagIntegrationException);
            _performCleanup = true;
        }

        /// <summary>
        /// Test PutTagForIntegrationProvider
        /// </summary>
        [Fact]
        public void PutTagForIntegrationProviderTest()
        {
            Setup();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            Assert.Null(Record.Exception(() => _integrationResourceApi.PutTagForIntegrationProvider(tagUpdate, TestConstants.IntegrationName)));
            _performCleanup = true;
        }

        /// <summary>
        /// Test PutTagForIntegrationProvider
        /// </summary>
        [Fact]
        public async void PutTagForIntegrationProviderAsyncTest()
        {
            Setup();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            var tagIntegrationProviderException = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.PutTagForIntegrationProviderAsync(tagUpdate, TestConstants.IntegrationName));
            Assert.Null(tagIntegrationProviderException);
            _performCleanup = true;
        }

        /// <summary>
        /// Test PutTagForIntegrationProvider
        /// </summary>
        [Fact]
        public void DeleteTagForIntegrationTest()
        {
            Setup();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            Assert.Null(Record.Exception(() => _integrationResourceApi.PutTagForIntegration(tagUpdate, TestConstants.IntegrationName, TestConstants.ModelName)));
            Assert.Null(Record.Exception(() => _integrationResourceApi.DeleteTagForIntegration(tagUpdate, TestConstants.IntegrationName, TestConstants.ModelName)));
            _performCleanup = true;
        }

        /// <summary>
        /// Test PutTagForIntegrationProvider
        /// </summary>
        [Fact]
        public async void DeleteTagForIntegrationAsyncTest()
        {
            Setup();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            var tagIntegrationProviderException = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.PutTagForIntegrationProviderAsync(tagUpdate, TestConstants.IntegrationName));
            Assert.Null(tagIntegrationProviderException);

            var deletetagIntegrationProviderException = await Record.ExceptionAsync(async () =>
            await _integrationResourceApi.DeleteTagForIntegrationAsync(tagUpdate, TestConstants.IntegrationName, TestConstants.ModelName));
            Assert.Null(deletetagIntegrationProviderException);
            _performCleanup = true;
        }

        /// <summary>
        /// Dispose method to clean up
        /// </summary>
        public void Dispose()
        {
            if (_performCleanup)
                _integrationResourceApi.DeleteIntegration();
        }

        private void Setup()
        {
            _integrationResourceApi.CreateIntegration();
        }
    }
}
