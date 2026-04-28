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
using Conductor.Client.Models;
using System.Collections.Generic;

namespace Conductor.Definition.TaskType
{
    public class KafkaPublishTask : Task
    {
        public KafkaPublishTask(string taskReferenceName, string bootStrapServers, string topic, object value, string key = null, Dictionary<string, string> headers = null) : base(taskReferenceName, WorkflowTask.WorkflowTaskTypeEnum.KAFKAPUBLISH)
        {
            var kafkaRequest = new Dictionary<string, object>
            {
                { "bootStrapServers", bootStrapServers },
                { "topic", topic },
                { "value", value }
            };
            if (key != null) kafkaRequest["key"] = key;
            if (headers != null) kafkaRequest["headers"] = headers;
            WithInput("kafka_request", kafkaRequest);
        }
    }
}
