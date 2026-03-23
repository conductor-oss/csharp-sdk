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
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration
{
    /// <summary>
    /// Starts a single Conductor OSS container for the entire integration test suite.
    /// All tests in the [Collection("Conductor")] share this instance.
    /// </summary>
    [CollectionDefinition("Conductor")]
    public class ConductorCollection : ICollectionFixture<ConductorFixture> { }

    public class ConductorFixture : IAsyncLifetime
    {
        private const string Image = "conductoross/conductor-standalone:3.15.0";
        private const int ContainerPort = 8080;

        private readonly IContainer _container;

        public Configuration Configuration { get; private set; }

        public ConductorFixture()
        {
            _container = new ContainerBuilder()
                .WithImage(Image)
                .WithPortBinding(ContainerPort, true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilHttpRequestIsSucceeded(r => r.ForPort(ContainerPort).ForPath("/health"))
                )
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            var host = _container.Hostname;
            var port = _container.GetMappedPublicPort(ContainerPort);
            Configuration = new Configuration { BasePath = $"http://{host}:{port}/api" };
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }
    }
}
