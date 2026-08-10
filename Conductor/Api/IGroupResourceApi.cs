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
using Conductor.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ThreadTask = System.Threading.Tasks;

namespace conductor_csharp.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IGroupResourceApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Add user to group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        Object AddUserToGroup(string groupId, string userId);

        /// <summary>
        /// Add users to group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        void AddUsersToGroup(List<string> body, string groupId);

        /// <summary>
        /// Delete a group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Response</returns>
        Response DeleteGroup(string id);

        /// <summary>
        /// Get the permissions this group has over workflows and tasks
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <returns>Object</returns>
        Object GetGrantedPermissions(string groupId);

        /// <summary>
        /// Get a group by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object GetGroup(string id);

        /// <summary>
        /// Get all users in group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object GetUsersInGroup(string id);

        /// <summary>
        /// Get all groups
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Group&gt;</returns>
        List<Group> ListGroups();

        /// <summary>
        /// Remove user from group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        Object RemoveUserFromGroup(string groupId, string userId);

        /// <summary>
        /// Remove users from group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        void RemoveUsersFromGroup(List<string> body, string groupId);

        /// <summary>
        /// Create or update a group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        Object UpsertGroup(UpsertGroupRequest body, string id);

        #endregion Synchronous Operations

        #region Asynchronous Operations
        /// <summary>
        /// Asynchronous Add user to group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> AddUserToGroupAsync(string groupId, string userId);

        /// <summary>
        /// Asynchronous Add users to group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        ThreadTask.Task AddUsersToGroupAsync(List<string> body, string groupId);

        /// <summary>
        /// Asynchronous Delete a group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Response</returns>
        ThreadTask.Task<Response> DeleteGroupAsync(string id);

        /// <summary>
        /// Asynchronous Get the permissions this group has over workflows and tasks
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> GetGrantedPermissionsAsync(string groupId);

        /// <summary>
        /// Asynchronous Get a group by id
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> GetGroupAsync(string id);

        /// <summary>
        /// Asynchronous Get all users in group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> GetUsersInGroupAsync(string id);

        /// <summary>
        /// Asynchronous Get all groups
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Group&gt;</returns>
        ThreadTask.Task<List<Group>> ListGroupsAsync();

        /// <summary>
        /// Asynchronous Remove user from group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> RemoveUserFromGroupAsync(string groupId, string userId);

        /// <summary>
        /// Asynchronous Remove users from group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        ThreadTask.Task RemoveUsersFromGroupAsync(List<string> body, string groupId);

        /// <summary>
        /// Asynchronous Create or update a group
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Conductor.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"></param>
        /// <param name="id"></param>
        /// <returns>Object</returns>
        ThreadTask.Task<Object> UpsertGroupAsync(UpsertGroupRequest body, string id);
        #endregion Asynchronous Operations
    }
}
