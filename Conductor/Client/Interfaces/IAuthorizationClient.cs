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
    public interface IAuthorizationClient
    {
        // Permissions
        void GrantPermissions(AuthorizationRequest authorizationRequest);
        ThreadTask.Task GrantPermissionsAsync(AuthorizationRequest authorizationRequest);

        void RemovePermissions(AuthorizationRequest authorizationRequest);
        ThreadTask.Task RemovePermissionsAsync(AuthorizationRequest authorizationRequest);

        object GetPermissions(string type, string id);
        ThreadTask.Task<object> GetPermissionsAsync(string type, string id);

        // Applications
        object CreateApplication(CreateOrUpdateApplicationRequest request);
        ThreadTask.Task<object> CreateApplicationAsync(CreateOrUpdateApplicationRequest request);

        object GetApplication(string applicationId);
        ThreadTask.Task<object> GetApplicationAsync(string applicationId);

        List<ExtendedConductorApplication> ListApplications();
        ThreadTask.Task<List<ExtendedConductorApplication>> ListApplicationsAsync();

        object UpdateApplication(CreateOrUpdateApplicationRequest request, string applicationId);
        ThreadTask.Task<object> UpdateApplicationAsync(CreateOrUpdateApplicationRequest request, string applicationId);

        void DeleteApplication(string applicationId);
        ThreadTask.Task DeleteApplicationAsync(string applicationId);

        object CreateAccessKey(string applicationId);
        ThreadTask.Task<object> CreateAccessKeyAsync(string applicationId);

        object GetAccessKeys(string applicationId);
        ThreadTask.Task<object> GetAccessKeysAsync(string applicationId);

        object ToggleAccessKeyStatus(string applicationId, string keyId);
        ThreadTask.Task<object> ToggleAccessKeyStatusAsync(string applicationId, string keyId);

        void DeleteAccessKey(string applicationId, string keyId);
        ThreadTask.Task DeleteAccessKeyAsync(string applicationId, string keyId);

        void AddRoleToApplicationUser(string applicationId, string role);
        ThreadTask.Task AddRoleToApplicationUserAsync(string applicationId, string role);

        void RemoveRoleFromApplicationUser(string applicationId, string role);
        ThreadTask.Task RemoveRoleFromApplicationUserAsync(string applicationId, string role);

        List<TagObject> GetTagsForApplication(string applicationId);
        void PutTagForApplication(List<TagObject> tags, string applicationId);
        void DeleteTagForApplication(List<TagObject> tags, string applicationId);

        // Users
        object UpsertUser(UpsertUserRequest request, string userId);
        ThreadTask.Task<object> UpsertUserAsync(UpsertUserRequest request, string userId);

        object GetUser(string userId);
        ThreadTask.Task<object> GetUserAsync(string userId);

        List<ConductorUser> ListUsers(bool? apps = null);
        ThreadTask.Task<List<ConductorUser>> ListUsersAsync(bool? apps = null);

        void DeleteUser(string userId);
        ThreadTask.Task DeleteUserAsync(string userId);

        void SendInviteEmail(string userId, ConductorUser body = null);
        ThreadTask.Task SendInviteEmailAsync(string userId, ConductorUser body = null);

        // Groups
        object UpsertGroup(UpsertGroupRequest request, string groupId);
        ThreadTask.Task<object> UpsertGroupAsync(UpsertGroupRequest request, string groupId);

        object GetGroup(string groupId);
        ThreadTask.Task<object> GetGroupAsync(string groupId);

        List<Group> ListGroups();
        ThreadTask.Task<List<Group>> ListGroupsAsync();

        void DeleteGroup(string groupId);
        ThreadTask.Task DeleteGroupAsync(string groupId);

        void AddUserToGroup(string groupId, string userId);
        ThreadTask.Task AddUserToGroupAsync(string groupId, string userId);

        void AddUsersToGroup(List<string> userIds, string groupId);
        ThreadTask.Task AddUsersToGroupAsync(List<string> userIds, string groupId);

        void RemoveUserFromGroup(string groupId, string userId);
        ThreadTask.Task RemoveUserFromGroupAsync(string groupId, string userId);

        void RemoveUsersFromGroup(List<string> userIds, string groupId);
        ThreadTask.Task RemoveUsersFromGroupAsync(List<string> userIds, string groupId);

        object GetUsersInGroup(string groupId);
        ThreadTask.Task<object> GetUsersInGroupAsync(string groupId);

        object GetGrantedPermissionsForGroup(string groupId);
        ThreadTask.Task<object> GetGrantedPermissionsForGroupAsync(string groupId);

        object GetGrantedPermissionsForUser(string userId);
        ThreadTask.Task<object> GetGrantedPermissionsForUserAsync(string userId);

        // Tokens
        Token GenerateToken(GenerateTokenRequest request);
        ThreadTask.Task<Token> GenerateTokenAsync(GenerateTokenRequest request);

        object GetUserInfo(bool? claims = null);
        ThreadTask.Task<object> GetUserInfoAsync(bool? claims = null);
    }
}
