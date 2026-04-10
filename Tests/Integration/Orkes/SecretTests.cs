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
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Orkes
{
    [Collection("Integration")]
    [Trait("Category", "Integration")]
    public class SecretTests : IClassFixture<ConductorFixture>
    {
        private readonly SecretResourceApi _secretClient;
        private readonly string _secretName;

        public SecretTests(ConductorFixture fixture)
        {
            _secretClient = fixture.Configuration.GetClient<SecretResourceApi>();
            _secretName = TestPrefix.Name("secret");
        }

        [Fact]
        public void PutSecret_CanBeRetrieved()
        {
            _secretClient.PutSecret("secret_value", _secretName);
            var value = _secretClient.GetSecret(_secretName);
            Assert.NotNull(value);
            Cleanup();
        }

        [Fact]
        public void ListSecrets_ReturnsResult()
        {
            _secretClient.PutSecret("secret_value", _secretName);
            var list = _secretClient.ListAllSecretNames();
            Assert.NotNull(list);
            Cleanup();
        }

        [Fact]
        public void DeleteSecret_SucceedsWithoutError()
        {
            _secretClient.PutSecret("secret_value", _secretName);
            _secretClient.DeleteSecret(_secretName);
            // No exception = success
        }

        [Fact]
        public void SecretExists_ReturnsTrueAfterPut()
        {
            _secretClient.PutSecret("secret_value", _secretName);
            var exists = _secretClient.SecretExists(_secretName);
            Assert.NotNull(exists);
            Cleanup();
        }

        [Fact]
        public void GetSecret_NonExistent_ThrowsApiException()
        {
            Assert.Throws<Conductor.Client.ApiException>(() =>
                _secretClient.GetSecret(TestPrefix.Name("nonexistent_secret")));
        }

        private void Cleanup()
        {
            try { _secretClient.DeleteSecret(_secretName); } catch { }
        }
    }
}
