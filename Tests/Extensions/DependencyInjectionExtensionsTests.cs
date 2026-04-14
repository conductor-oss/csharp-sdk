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
using Conductor.Client.Extensions;
using Conductor.Client.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.Extensions
{
    public class DependencyInjectionExtensionsTests
    {
        [Fact]
        public void AddConductorWorker_RegistersMetricsCollectorAsSingleton()
        {
            var services = new ServiceCollection();

            services.AddConductorWorker();

            var provider = services.BuildServiceProvider();
            var metrics = provider.GetService<MetricsCollector>();
            Assert.NotNull(metrics);

            var metrics2 = provider.GetService<MetricsCollector>();
            Assert.Same(metrics, metrics2);
        }

        [Fact]
        public void AddConductorWorker_RegistersExpectedServices()
        {
            var services = new ServiceCollection();

            var result = services.AddConductorWorker();

            Assert.Same(services, result);
            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<Conductor.Client.Configuration>());
            Assert.NotNull(provider.GetService<MetricsCollector>());
        }

        [Fact]
        public void AddConductorWorker_WithExplicitConfiguration_UsesProvidedInstance()
        {
            var services = new ServiceCollection();
            var config = new Conductor.Client.Configuration();

            services.AddConductorWorker(config);

            var provider = services.BuildServiceProvider();
            var resolved = provider.GetService<Conductor.Client.Configuration>();
            Assert.Same(config, resolved);
        }

        [Fact]
        public void AddConductorWorker_WithNullConfiguration_CreatesDefault()
        {
            var services = new ServiceCollection();

            services.AddConductorWorker(configuration: null);

            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<Conductor.Client.Configuration>());
        }
    }
}
