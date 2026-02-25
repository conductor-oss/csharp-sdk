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
using conductor_csharp.Api;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using System.Collections.Generic;
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Client.Orkes
{
    public class OrkesEventClient : IEventClient
    {
        private readonly IEventResourceApi _eventApi;

        public OrkesEventClient(Configuration configuration)
        {
            _eventApi = configuration.GetClient<EventResourceApi>();
        }

        public OrkesEventClient(IEventResourceApi eventApi)
        {
            _eventApi = eventApi;
        }

        public void AddEventHandler(EventHandler eventHandler) => _eventApi.AddEventHandler(eventHandler);
        public ThreadTask.Task AddEventHandlerAsync(EventHandler eventHandler) => ThreadTask.Task.Run(() => _eventApi.AddEventHandler(eventHandler));

        public void UpdateEventHandler(EventHandler eventHandler) => _eventApi.UpdateEventHandler(eventHandler);
        public ThreadTask.Task UpdateEventHandlerAsync(EventHandler eventHandler) => ThreadTask.Task.Run(() => _eventApi.UpdateEventHandler(eventHandler));

        public void RemoveEventHandler(string name) => _eventApi.RemoveEventHandlerStatus(name);
        public ThreadTask.Task RemoveEventHandlerAsync(string name) => ThreadTask.Task.Run(() => _eventApi.RemoveEventHandlerStatus(name));

        public List<EventHandler> GetEventHandlers() => _eventApi.GetEventHandlers();
        public ThreadTask.Task<List<EventHandler>> GetEventHandlersAsync() => _eventApi.GetEventHandlersAsync();

        public List<EventHandler> GetEventHandlersForEvent(string eventName, bool activeOnly = true) => _eventApi.GetEventHandlersForEvent(eventName, activeOnly);
        public ThreadTask.Task<List<EventHandler>> GetEventHandlersForEventAsync(string eventName, bool activeOnly = true) => _eventApi.GetEventHandlersForEventAsync(eventName, activeOnly);

        public void PutQueueConfig(string queueType, string queueName, string config) => _eventApi.PutQueueConfig(config, queueType, queueName);
        public ThreadTask.Task PutQueueConfigAsync(string queueType, string queueName, string config) => ThreadTask.Task.Run(() => _eventApi.PutQueueConfig(config, queueType, queueName));

        public Dictionary<string, object> GetQueueConfig(string queueType, string queueName) => _eventApi.GetQueueConfig(queueType, queueName);
        public ThreadTask.Task<Dictionary<string, object>> GetQueueConfigAsync(string queueType, string queueName) => _eventApi.GetQueueConfigAsync(queueType, queueName);

        public void DeleteQueueConfig(string queueType, string queueName) => _eventApi.DeleteQueueConfig(queueType, queueName);
        public ThreadTask.Task DeleteQueueConfigAsync(string queueType, string queueName) => ThreadTask.Task.Run(() => _eventApi.DeleteQueueConfig(queueType, queueName));

        public Dictionary<string, string> GetQueueNames() => _eventApi.GetQueueNames();
        public ThreadTask.Task<Dictionary<string, string>> GetQueueNamesAsync() => _eventApi.GetQueueNamesAsync();
    }
}
