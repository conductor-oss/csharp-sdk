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
using Conductor.Client.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Tests.Extensions
{
    public class DependencyInjectionExtensionsTests
    {
        [Fact]
        public void AddConductorWorker_WithMetricsEnabled_RegistersMetricsCollectorAsSingleton()
        {
            var services = new ServiceCollection();
            var config = new Conductor.Client.Configuration { EnableMetrics = true };

            services.AddConductorWorker(config);

            var provider = services.BuildServiceProvider();
            var metrics = provider.GetService<MetricsCollector>();
            Assert.NotNull(metrics);

            var metrics2 = provider.GetService<MetricsCollector>();
            Assert.Same(metrics, metrics2);

            var resolvedConfig = provider.GetRequiredService<Conductor.Client.Configuration>();
            Assert.Same(metrics, resolvedConfig.ApiClient.Metrics);
        }

        [Fact]
        public void AddConductorWorker_MetricsDisabledByDefault_DoesNotRegisterMetricsCollector()
        {
            var services = new ServiceCollection();

            services.AddConductorWorker();

            var provider = services.BuildServiceProvider();
            Assert.Null(provider.GetService<MetricsCollector>());

            var config = provider.GetRequiredService<Conductor.Client.Configuration>();
            Assert.Null(config.ApiClient.Metrics);
        }

        [Fact]
        public void AddConductorWorker_RegistersExpectedServices()
        {
            var services = new ServiceCollection();

            var result = services.AddConductorWorker();

            Assert.Same(services, result);
            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<Conductor.Client.Configuration>());
            Assert.Null(provider.GetService<MetricsCollector>());
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

        [Fact]
        public void ConfigureConductorWorkerDiscovery_WithoutConfiguration_UsesEmptyAssemblyCollection()
        {
            var services = new ServiceCollection();

            services.AddConductorWorker();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<WorkerDiscoveryOptions>>().Value;

            Assert.Empty(options.Assemblies);
        }

        [Fact]
        public void ConfigureConductorWorkerDiscovery_WithAction_ConfiguresAssemblyCollection()
        {
            var services = new ServiceCollection();
            var assembly = typeof(DependencyInjectionExtensionsTests).Assembly;

            services.ConfigureConductorWorkerDiscovery(options =>
                options.Assemblies = new[] { assembly });

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<WorkerDiscoveryOptions>>().Value;

            Assert.Contains(assembly, options.Assemblies);
        }

        [Fact]
        public void ConfigureConductorWorkerDiscovery_WithAttributeDiscoveryDisabled_ConfiguresOptions()
        {
            var services = new ServiceCollection();

            services.ConfigureConductorWorkerDiscovery(options =>
                options.EnableAttributeDiscovery = false);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<WorkerDiscoveryOptions>>().Value;

            Assert.False(options.EnableAttributeDiscovery);
        }

        [Fact]
        public void ConfigureConductorWorkerDiscovery_WithOptions_CopiesAssemblyCollection()
        {
            var services = new ServiceCollection();
            var assembly = typeof(DependencyInjectionExtensionsTests).Assembly;
            var assemblies = new List<Assembly> { assembly };

            services.ConfigureConductorWorkerDiscovery(new WorkerDiscoveryOptions
            {
                Assemblies = assemblies
            });
            assemblies.Clear();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<WorkerDiscoveryOptions>>().Value;

            Assert.Contains(assembly, options.Assemblies);
        }
    }
}
