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
using Conductor.Client.Interfaces;
using Conductor.Client.Telemetry;
using Conductor.Client.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

namespace Conductor.Client.Extensions
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddConductorWorkflowTask<T>(this IServiceCollection services, T worker) where T : IWorkflowTask
        {
            services.AddSingleton<IWorkflowTask>(worker);
            return services;
        }

        public static IServiceCollection AddConductorWorker(this IServiceCollection services, Configuration configuration = null, Action<IServiceProvider, HttpClient> configureHttpClient = null)
        {
            services.AddHttpClient();
            services.AddOptions();
            if (configuration == null)
            {
                configuration = new Configuration();
            }
            services.AddSingleton<Configuration>(configuration);
            services.AddSingleton<IWorkflowTaskClient, WorkflowTaskHttpClient>();
            services.AddSingleton<MetricsCollector>(sp =>
            {
                var collector = new MetricsCollector();
                ApiClient.Metrics = collector;
                return collector;
            });
            services.AddTransient<IWorkflowTaskCoordinator, WorkflowTaskCoordinator>();
            return services;
        }

        public static IServiceCollection WithHostedService<T>(this IServiceCollection services) where T : BackgroundService
        {
            services.AddHostedService<T>();
            return services;
        }

        public static IServiceCollection WithHostedService(this IServiceCollection services)
        {
            services.AddHostedService<WorkflowTaskService>();
            return services;
        }
    }
}
