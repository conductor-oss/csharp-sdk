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
using ThreadTask = System.Threading.Tasks;

namespace Conductor.Client.Interfaces
{
    public interface IEventClient
    {
        void AddEventHandler(EventHandler eventHandler);
        ThreadTask.Task AddEventHandlerAsync(EventHandler eventHandler);

        void UpdateEventHandler(EventHandler eventHandler);
        ThreadTask.Task UpdateEventHandlerAsync(EventHandler eventHandler);

        void RemoveEventHandler(string name);
        ThreadTask.Task RemoveEventHandlerAsync(string name);

        List<EventHandler> GetEventHandlers();
        ThreadTask.Task<List<EventHandler>> GetEventHandlersAsync();

        List<EventHandler> GetEventHandlersForEvent(string eventName, bool activeOnly = true);
        ThreadTask.Task<List<EventHandler>> GetEventHandlersForEventAsync(string eventName, bool activeOnly = true);

        void PutQueueConfig(string queueType, string queueName, string config);
        ThreadTask.Task PutQueueConfigAsync(string queueType, string queueName, string config);

        Dictionary<string, object> GetQueueConfig(string queueType, string queueName);
        ThreadTask.Task<Dictionary<string, object>> GetQueueConfigAsync(string queueType, string queueName);

        void DeleteQueueConfig(string queueType, string queueName);
        ThreadTask.Task DeleteQueueConfigAsync(string queueType, string queueName);

        Dictionary<string, string> GetQueueNames();
        ThreadTask.Task<Dictionary<string, string>> GetQueueNamesAsync();
    }
}
