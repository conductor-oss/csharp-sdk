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
    [Trait("Category", "CloudIntegration")]
    public class PromptResourceApiTest : IDisposable
    {
        private readonly PromptResourceApi _promptResourceApi;
        private readonly IntegrationResourceApi _integrationResourceApi;
        private readonly OrkesApiClient _orkesApiClient;
        private readonly ITestOutputHelper _testOutputHelper;
        private bool _performCleanup = true;

        /// <summary>
        /// Init operations
        /// </summary>
        /// <param name="testOutputHelper"></param>
        public PromptResourceApiTest(ITestOutputHelper testOutputHelper)
        {
            //dev local testing
            //_orkesApiClient = new OrkesApiClient(new Configuration(), new OrkesAuthenticationSettings(Constants.KEY_ID, Constants.KEY_SECRET));
            //_promptResourceApi = _orkesApiClient.GetClient<PromptResourceApi>();
            //_integrationResourceApi = _orkesApiClient.GetClient<IntegrationResourceApi>();

            _testOutputHelper = testOutputHelper;
            _promptResourceApi = ApiExtensions.GetClient<PromptResourceApi>();
            _integrationResourceApi = ApiExtensions.GetClient<IntegrationResourceApi>();
        }

        /// <summary>
        /// Test SaveMessageTemplate
        /// </summary>
        [Fact]
        public void CreatePromptTemplateTest()
        {
            Setup();
            string ModelName = IntegrationExtensions.GetModelName(TestConstants.IntegrationPromptName, TestConstants.ModelPromptName);
            List<string> model = new List<string>() { ModelName };
            Assert.Null(Record.Exception(() => _promptResourceApi.SaveMessageTemplate(TestConstants.TemplateBody, TestConstants.TemplateDescription, TestConstants.PromptName, model)));
            _performCleanup = true;
        }

        /// <summary>
        /// Test SaveMessageTemplateAsync
        /// </summary>
        [Fact]
        public async void CreatePromptTemplateAsyncTest()
        {
            Setup();
            string ModelName = IntegrationExtensions.GetModelName(TestConstants.IntegrationPromptName, TestConstants.ModelPromptName);
            List<string> model = new List<string>() { ModelName };
            var saveMessageTemplateException = await Record.ExceptionAsync(async () =>
            await _promptResourceApi.SaveMessageTemplateAsync(TestConstants.TemplateBody, TestConstants.TemplateDescription, TestConstants.PromptName, model));
            Assert.Null(saveMessageTemplateException);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetMessageTemplates
        /// </summary>
        [Fact]
        public void GetMessageTemplatesTest()
        {
            Setup();
            var response = _promptResourceApi.GetMessageTemplates();
            AssertExtensions.AssertModelResponse<List<MessageTemplate>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetMessageTemplatesAsync
        /// </summary>
        [Fact]
        public async void GetMessageTemplatesAsyncTest()
        {
            Setup();
            var response = await _promptResourceApi.GetMessageTemplatesAsync();
            AssertExtensions.AssertModelResponse<List<MessageTemplate>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetMessageTemplate
        /// </summary>
        [Fact]
        public void GetMessageTemplateTest()
        {
            Setup();
            var response = _promptResourceApi.GetMessageTemplate(TestConstants.PromptName);
            AssertExtensions.AssertModelResponse<MessageTemplate>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetMessageTemplateAsync
        /// </summary>
        [Fact]
        public async void GetMessageTemplateAsyncTest()
        {
            Setup();
            var response = await _promptResourceApi.GetMessageTemplateAsync(TestConstants.PromptName);
            AssertExtensions.AssertModelResponse<MessageTemplate>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test DeleteMessageTemplate
        /// </summary>
        [Fact]
        public void DeleteMessageTemplateTest()
        {
            Setup();
            Assert.Null(Record.Exception(() => _promptResourceApi.DeleteMessageTemplate(TestConstants.PromptName)));
        }

        /// <summary>
        /// Test DeleteMessageTemplateAsyncTest
        /// </summary>
        [Fact]
        public async void DeleteMessageTemplateAsyncTest()
        {
            Setup();
            var deleteMessageTemplateException = await Record.ExceptionAsync(async () => await _promptResourceApi.DeleteMessageTemplateAsync(TestConstants.PromptName));
        }

        /// <summary>
        /// Test PutTagForPromptTemplateTest
        /// </summary>
        [Fact]
        public void PutTagForPromptTemplateTest()
        {
            CreatePromptTemplate();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            Assert.Null(Record.Exception(() => _promptResourceApi.PutTagForPromptTemplate(tagUpdate, TestConstants.PromptName)));
            _performCleanup = true;

        }

        /// <summary>
        /// Test PutTagForPromptTemplateAsync
        /// </summary>
        [Fact]
        public async void PutTagForPromptTemplateAsyncTest()
        {
            CreatePromptTemplate();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            var tagForPromptTemplateException = await Record.ExceptionAsync(() => _promptResourceApi.PutTagForPromptTemplateAsync(tagUpdate, TestConstants.PromptName));
            Assert.Null(tagForPromptTemplateException);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTagForPromptTemplateTest
        /// </summary>
        [Fact]
        public void GetTagsForPromptTemplateTest()
        {
            CreatePromptTemplate();
            var response = _promptResourceApi.GetTagsForPromptTemplate(TestConstants.PromptName);
            AssertExtensions.AssertModelResponse<List<Tag>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test GetTagForPromptTemplateAsyncTest
        /// </summary>
        [Fact]
        public async void GetTagsForPromptTemplateAsyncTest()
        {
            CreatePromptTemplate();
            var response = await _promptResourceApi.GetTagsForPromptTemplateAsync(TestConstants.PromptName);
            AssertExtensions.AssertModelResponse<List<Tag>>(response);
            _performCleanup = true;
        }

        /// <summary>
        /// Test DeleteTagForPromptTemplate
        /// </summary>
        [Fact]
        public void DeleteTagForPromptTemplateTest()
        {
            CreatePromptTemplate();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            Assert.Null(Record.Exception(() => _promptResourceApi.DeleteTagForPromptTemplate(tagUpdate, TestConstants.PromptName)));
        }

        /// <summary>
        /// Test DeleteTagForPromptTemplateAsync
        /// </summary>
        [Fact]
        public async void DeleteTagForPromptTemplateAsyncTest()
        {
            CreatePromptTemplate();
            var jsonObject = IntegrationExtensions.LoadIntegrationData();
            List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
            var deleteTagForPromptException = await Record.ExceptionAsync(async () => await _promptResourceApi.DeleteTagForPromptTemplateAsync(tagUpdate, TestConstants.PromptName));
            Assert.Null(deleteTagForPromptException);
        }

        /// <summary>
        /// Dispose method to clean up
        /// </summary>
        public void Dispose()
        {
            if (_performCleanup)
                IntegrationExtensions.DeleteIntegration(_integrationResourceApi, true);
        }

        /// <summary>
        /// Method to create prompts for an integration
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void CreatePromptTemplate()
        {
            try
            {
                Setup();
                var jsonObject = IntegrationExtensions.LoadIntegrationData();
                string ModelName = IntegrationExtensions.GetModelName(TestConstants.IntegrationPromptName, TestConstants.ModelPromptName);
                List<string> model = new List<string>() { ModelName };
                _promptResourceApi.SaveMessageTemplate(TestConstants.TemplateBody, TestConstants.TemplateDescription, TestConstants.PromptName, model);
                List<Tag> tagUpdate = JsonConvert.DeserializeObject<List<Tag>>(jsonObject.tagUpdate.ToString());
                _promptResourceApi.PutTagForPromptTemplate(tagUpdate, TestConstants.PromptName);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to create Prompt Template: {ex.Message}";
                throw new Exception(errorMessage, ex);
            }
        }

        private void Setup()
        {
            _integrationResourceApi.CreateIntegration(true);
        }
    }
}
