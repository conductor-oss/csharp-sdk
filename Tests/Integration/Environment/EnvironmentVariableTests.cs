/*
 * Copyright 2024 Conductor Authors.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software distributed under the LICENSE is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using Conductor.Api;
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Environment
{
    // Confirmed Orkes-only against a freshly-pulled `conductoross/conductor:latest`: OSS added
    // `GET /api/environment` (read-only) in 3.32.0-rc.9 but no write support, so
    // CreateOrUpdateEnvVariable's `PUT /environment/{key}` 405s ("Request method 'PUT' is not
    // supported"). See conductor-test-harness/server/3.32.0-rc.9/CHANGES.md.
    [Collection("Integration")]
    [Trait("Category", "Integration")]
    [Trait("ServerType", "Orkes")]
    public class EnvironmentVariableTests : IClassFixture<ConductorFixture>
    {
        private readonly EnvironmentResourceApi _envClient;
        private readonly string _key;

        public EnvironmentVariableTests(ConductorFixture fixture)
        {
            _envClient = fixture.Configuration.GetClient<EnvironmentResourceApi>();
            _key = TestPrefix.Name("env_var");
        }

        [Fact]
        public void CreateEnvVariable_CanBeRetrieved()
        {
            _envClient.CreateOrUpdateEnvVariable("test_value", _key);
            var value = _envClient.Get1(_key)?.ToString().Trim('"');
            Assert.Equal("test_value", value);
            Cleanup();
        }

        [Fact]
        public void UpdateEnvVariable_ValueChanges()
        {
            _envClient.CreateOrUpdateEnvVariable("original", _key);
            _envClient.DeleteEnvVariable(_key);
            _envClient.CreateOrUpdateEnvVariable("updated", _key);
            string value = null;
            for (var i = 0; i < 10; i++)
            {
                value = _envClient.Get1(_key)?.ToString().Trim('"');
                if (value == "updated") break;
                System.Threading.Thread.Sleep(500);
            }
            Assert.Equal("updated", value);
            Cleanup();
        }

        [Fact]
        public void GetAllEnvVariables_ContainsCreated()
        {
            _envClient.CreateOrUpdateEnvVariable("test_value", _key);
            var all = _envClient.GetAll();
            Assert.NotNull(all);
            Assert.True(all.ContainsKey(_key));
            Cleanup();
        }

        [Fact]
        public void DeleteEnvVariable_RemovedFromList()
        {
            _envClient.CreateOrUpdateEnvVariable("test_value", _key);
            _envClient.DeleteEnvVariable(_key);
            var all = _envClient.GetAll();
            Assert.False(all.ContainsKey(_key));
        }

        private void Cleanup()
        {
            try { _envClient.DeleteEnvVariable(_key); } catch { }
        }
    }
}
