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
    public class HumanTaskResourceApiTest
    {
        private readonly HumanTaskResourceApi _humanTaskResourceApi;
        private readonly OrkesApiClient _orkesApiClient;
        private const string UserFormName = "USER_FORM_NAME_TEST";
        private readonly ITestOutputHelper _testOutputHelper;

        public HumanTaskResourceApiTest(ITestOutputHelper testOutputHelper)
        {
            // dev local testing
            //_orkesApiClient = new OrkesApiClient(new Configuration(), new OrkesAuthenticationSettings(Constants.KEY_ID, Constants.KEY_SECRET));
            //_humanTaskResourceApi = _orkesApiClient.GetClient<HumanTaskResourceApi>();

            _testOutputHelper = testOutputHelper;
            _humanTaskResourceApi = ApiExtensions.GetClient<HumanTaskResourceApi>();
        }

        /// <summary>
        /// Test case for HumanTaskResourceApi using  BaseUrl:- 'https://sdkdev.orkesconductor.io/'
        /// Get templates of user form based on name
        /// </summary>
        [Fact]
        public void TestUserForm()
        {
            var templates = _humanTaskResourceApi.GetAllTemplates(UserFormName, null);
            if (templates != null && templates.Count > 0)
            {
                _testOutputHelper.WriteLine($"UserFormName --> {UserFormName} and result count of templates --> {templates.Count}");
                Assert.NotEmpty(templates);
            }
            else
            {
                _testOutputHelper.WriteLine($"UserFormName --> {UserFormName} and result count of templates is 0");
                Assert.Empty(templates);
            }
        }
    }
}
