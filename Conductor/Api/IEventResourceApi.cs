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
using System;
using System.Collections.Generic;
using System.Text;
using ThreadTask = System.Threading.Tasks;
using EventHandler = Conductor.Client.Models.EventHandler;

namespace conductor_csharp.Api
{
    // <summary>
    // Represents a collection of functions to interact with the API endpoints
    // </summary>
    public interface IEventResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Add a new event handler.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        void AddEventHandler(EventHandler body);

        /// <summary>
        /// Delete queue config by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        void DeleteQueueConfig(string queueType, string queueName);

        /// <summary>
        /// Get all the event handlers
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;EventHandler&gt;</returns>
        List<EventHandler> GetEventHandlers();

        /// <summary>
        /// Get event handlers for a given event
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="_event"></param>
        /// <param name="activeOnly"> (optional, default to true)</param>
        /// <returns>List&lt;EventHandler&gt;</returns>
        List<EventHandler> GetEventHandlersForEvent(string _event, bool? activeOnly = null);

        /// <summary>
        /// Get queue config by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        Dictionary<string, Object> GetQueueConfig(string queueType, string queueName);

        /// <summary>
        /// Get all queue configs
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, string&gt;</returns>
        Dictionary<string, string> GetQueueNames();

        /// <summary>
        /// Create or update queue config by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        void PutQueueConfig(string body, string queueType, string queueName);

        /// <summary>
        /// Remove an event handler
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns></returns>
        void RemoveEventHandlerStatus(string name);

        /// <summary>
        /// Update an existing event handler.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        void UpdateEventHandler(EventHandler body);

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Add a new event handler.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        ThreadTask.Task AddEventHandlerAsync(EventHandler body);

        /// <summary>
        /// Asynchronous Delete queue config by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        ThreadTask.Task DeleteQueueConfigAsync(string queueType, string queueName);

        /// <summary>
        /// Asynchronous Get all the event handlers
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;EventHandler&gt;</returns>
        ThreadTask.Task<List<EventHandler>> GetEventHandlersAsync();

        /// <summary>
        /// Asynchronous Get event handlers for a given event
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="_event"></param>
        /// <param name="activeOnly"> (optional, default to true)</param>
        /// <returns>List&lt;EventHandler&gt;</returns>
        ThreadTask.Task<List<EventHandler>> GetEventHandlersForEventAsync(string _event, bool? activeOnly = null);

        /// <summary>
        /// Asynchronous Get queue config by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns>Dictionary&lt;string, Object&gt;</returns>
        ThreadTask.Task<Dictionary<string, Object>> GetQueueConfigAsync(string queueType, string queueName);

        /// <summary>
        /// Asynchronous Get all queue configs
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Dictionary&lt;string, string&gt;</returns>
        ThreadTask.Task<Dictionary<string, string>> GetQueueNamesAsync();

        /// <summary>
        /// Asynchronous Create or update queue config by name
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="queueType"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        ThreadTask.Task PutQueueConfigAsync(string body, string queueType, string queueName);

        /// <summary>
        /// Asynchronous Remove an event handler
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns></returns>
        ThreadTask.Task RemoveEventHandlerStatusAsync(string name);

        /// <summary>
        /// Asynchronous Update an existing event handler.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <returns></returns>
        ThreadTask.Task UpdateEventHandlerAsync(EventHandler body);

        #endregion Asynchronous Operations
    }
}
