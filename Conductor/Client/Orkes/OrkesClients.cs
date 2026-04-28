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

namespace Conductor.Client.Orkes
{
    /// <summary>
    /// Factory class that creates all high-level Orkes clients from a single Configuration.
    /// </summary>
    public class OrkesClients
    {
        private readonly Configuration _configuration;

        public OrkesClients(Configuration configuration)
        {
            _configuration = configuration;
        }

        public IWorkflowClient GetWorkflowClient() => new OrkesWorkflowClient(_configuration);

        public ITaskClient GetTaskClient() => new OrkesTaskClient(_configuration);

        public IMetadataClient GetMetadataClient() => new OrkesMetadataClient(_configuration);

        public ISchedulerClient GetSchedulerClient() => new OrkesSchedulerClient(_configuration);

        public ISecretClient GetSecretClient() => new OrkesSecretClient(_configuration);

        public IAuthorizationClient GetAuthorizationClient() => new OrkesAuthorizationClient(_configuration);

        public IPromptClient GetPromptClient() => new OrkesPromptClient(_configuration);

        public IIntegrationClient GetIntegrationClient() => new OrkesIntegrationClient(_configuration);

        public IEventClient GetEventClient() => new OrkesEventClient(_configuration);
    }
}
