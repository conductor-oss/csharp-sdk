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
using Conductor.Client.Models;
using Conductor.Client.Telemetry;
using Conductor.Definition;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Conductor.Executor
{
    public class WorkflowExecutor
    {
        private readonly WorkflowResourceApi _workflowClient;
        private readonly MetadataResourceApi _metadataClient;
        private readonly MetricsCollector _metrics;

        public WorkflowExecutor(Configuration configuration, MetricsCollector metrics = null)
        {
            _workflowClient = configuration.GetClient<WorkflowResourceApi>();
            _metadataClient = configuration.GetClient<MetadataResourceApi>();
            _metrics = metrics;
        }

        public WorkflowExecutor(WorkflowResourceApi workflowClient, MetadataResourceApi metadataClient, MetricsCollector metrics = null)
        {
            _workflowClient = workflowClient;
            _metadataClient = metadataClient;
            _metrics = metrics;
        }

        public void RegisterWorkflow(WorkflowDef workflow, bool overwrite)
        {
            if (overwrite)
            {
                _metadataClient.UpdateWorkflowDefinitions(new List<WorkflowDef>(1) { workflow });
            }
            else
            {
                _metadataClient.Create(workflow);
            }
        }

        public string StartWorkflow(ConductorWorkflow conductorWorkflow)
        {
            return StartWorkflow(conductorWorkflow.GetStartWorkflowRequest());
        }

        public string StartWorkflow(StartWorkflowRequest startWorkflowRequest)
        {
            RecordInputSize(startWorkflowRequest);
            try
            {
                return _workflowClient.StartWorkflow(startWorkflowRequest);
            }
            catch (Exception ex)
            {
                _metrics?.RecordWorkflowStartError(
                    startWorkflowRequest.Name ?? "",
                    ex.GetType().Name);
                throw;
            }
        }

        private void RecordInputSize(StartWorkflowRequest request)
        {
            if (_metrics == null)
                return;
            try
            {
                double size = 0;
                if (request.Input != null)
                {
                    var json = JsonConvert.SerializeObject(request.Input);
                    size = json.Length;
                }
                _metrics.RecordWorkflowInputSize(
                    request.Name ?? "",
                    request.Version?.ToString() ?? "",
                    size);
            }
            catch
            {
                // Don't let metrics serialization failures disrupt workflow start.
            }
        }
    }
}
