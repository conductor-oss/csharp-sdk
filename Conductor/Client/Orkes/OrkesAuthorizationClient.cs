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
    public class OrkesAuthorizationClient : IAuthorizationClient
    {
        private readonly IAuthorizationResourceApi _authApi;
        private readonly IApplicationResourceApi _appApi;
        private readonly IUserResourceApi _userApi;
        private readonly IGroupResourceApi _groupApi;
        private readonly ITokenResourceApi _tokenApi;

        public OrkesAuthorizationClient(Configuration configuration)
        {
            _authApi = configuration.GetClient<AuthorizationResourceApi>();
            _appApi = new ApplicationResourceApi(configuration);
            _userApi = configuration.GetClient<UserResourceApi>();
            _groupApi = configuration.GetClient<GroupResourceApi>();
            _tokenApi = configuration.GetClient<TokenResourceApi>();
        }

        public OrkesAuthorizationClient(IAuthorizationResourceApi authApi, IApplicationResourceApi appApi, IUserResourceApi userApi, IGroupResourceApi groupApi, ITokenResourceApi tokenApi)
        {
            _authApi = authApi;
            _appApi = appApi;
            _userApi = userApi;
            _groupApi = groupApi;
            _tokenApi = tokenApi;
        }

        // Permissions
        public void GrantPermissions(AuthorizationRequest authorizationRequest) => _authApi.GrantPermissions(authorizationRequest);
        public ThreadTask.Task GrantPermissionsAsync(AuthorizationRequest authorizationRequest) => _authApi.GrantPermissionsAsync(authorizationRequest);

        public void RemovePermissions(AuthorizationRequest authorizationRequest) => _authApi.RemovePermissions(authorizationRequest);
        public ThreadTask.Task RemovePermissionsAsync(AuthorizationRequest authorizationRequest) => _authApi.RemovePermissionsAsync(authorizationRequest);

        public object GetPermissions(string type, string id) => _authApi.GetPermissions(type, id);
        public ThreadTask.Task<object> GetPermissionsAsync(string type, string id) => _authApi.GetPermissionsAsync(type, id);

        // Applications
        public object CreateApplication(CreateOrUpdateApplicationRequest request) => _appApi.CreateApplication(request);
        public ThreadTask.Task<object> CreateApplicationAsync(CreateOrUpdateApplicationRequest request) => _appApi.CreateApplicationAsync(request);

        public object GetApplication(string applicationId) => _appApi.GetApplication(applicationId);
        public ThreadTask.Task<object> GetApplicationAsync(string applicationId) => _appApi.GetApplicationAsync(applicationId);

        public List<ExtendedConductorApplication> ListApplications() => _appApi.ListApplications();
        public ThreadTask.Task<List<ExtendedConductorApplication>> ListApplicationsAsync() => _appApi.ListApplicationsAsync();

        public object UpdateApplication(CreateOrUpdateApplicationRequest request, string applicationId) => _appApi.UpdateApplication(request, applicationId);
        public ThreadTask.Task<object> UpdateApplicationAsync(CreateOrUpdateApplicationRequest request, string applicationId) => _appApi.UpdateApplicationAsync(request, applicationId);

        public void DeleteApplication(string applicationId) => _appApi.DeleteApplication(applicationId);
        public ThreadTask.Task DeleteApplicationAsync(string applicationId) => _appApi.DeleteApplicationAsync(applicationId);

        public object CreateAccessKey(string applicationId) => _appApi.CreateAccessKey(applicationId);
        public ThreadTask.Task<object> CreateAccessKeyAsync(string applicationId) => _appApi.CreateAccessKeyAsync(applicationId);

        public object GetAccessKeys(string applicationId) => _appApi.GetAccessKeys(applicationId);
        public ThreadTask.Task<object> GetAccessKeysAsync(string applicationId) => _appApi.GetAccessKeysAsync(applicationId);

        public object ToggleAccessKeyStatus(string applicationId, string keyId) => _appApi.ToggleAccessKeyStatus(applicationId, keyId);
        public ThreadTask.Task<object> ToggleAccessKeyStatusAsync(string applicationId, string keyId) => _appApi.ToggleAccessKeyStatusAsync(applicationId, keyId);

        public void DeleteAccessKey(string applicationId, string keyId) => _appApi.DeleteAccessKey(applicationId, keyId);
        public ThreadTask.Task DeleteAccessKeyAsync(string applicationId, string keyId) => _appApi.DeleteAccessKeyAsync(applicationId, keyId);

        public void AddRoleToApplicationUser(string applicationId, string role) => _appApi.AddRoleToApplicationUser(applicationId, role);
        public ThreadTask.Task AddRoleToApplicationUserAsync(string applicationId, string role) => _appApi.AddRoleToApplicationUserAsync(applicationId, role);

        public void RemoveRoleFromApplicationUser(string applicationId, string role) => _appApi.RemoveRoleFromApplicationUser(applicationId, role);
        public ThreadTask.Task RemoveRoleFromApplicationUserAsync(string applicationId, string role) => _appApi.RemoveRoleFromApplicationUserAsync(applicationId, role);

        public List<TagObject> GetTagsForApplication(string applicationId) => _appApi.GetTagsForApplication(applicationId);
        public void PutTagForApplication(List<TagObject> tags, string applicationId) => _appApi.PutTagForApplication(tags, applicationId);
        public void DeleteTagForApplication(List<TagObject> tags, string applicationId) => _appApi.DeleteTagForApplication(tags, applicationId);

        // Users
        public object UpsertUser(UpsertUserRequest request, string userId) => _userApi.UpsertUser(request, userId);
        public ThreadTask.Task<object> UpsertUserAsync(UpsertUserRequest request, string userId) => _userApi.UpsertUserAsync(request, userId);

        public object GetUser(string userId) => _userApi.GetUser(userId);
        public ThreadTask.Task<object> GetUserAsync(string userId) => _userApi.GetUserAsync(userId);

        public List<ConductorUser> ListUsers(bool? apps = null) => _userApi.ListUsers(apps);
        public ThreadTask.Task<List<ConductorUser>> ListUsersAsync(bool? apps = null) => _userApi.ListUsersAsync(apps);

        public void DeleteUser(string userId) => _userApi.DeleteUser(userId);
        public ThreadTask.Task DeleteUserAsync(string userId) => _userApi.DeleteUserAsync(userId);

        public void SendInviteEmail(string userId, ConductorUser body = null) => _userApi.SendInviteEmail(userId, body);
        public ThreadTask.Task SendInviteEmailAsync(string userId, ConductorUser body = null) => _userApi.SendInviteEmailAsync(userId, body);

        // Groups
        public object UpsertGroup(UpsertGroupRequest request, string groupId) => _groupApi.UpsertGroup(request, groupId);
        public ThreadTask.Task<object> UpsertGroupAsync(UpsertGroupRequest request, string groupId) => _groupApi.UpsertGroupAsync(request, groupId);

        public object GetGroup(string groupId) => _groupApi.GetGroup(groupId);
        public ThreadTask.Task<object> GetGroupAsync(string groupId) => _groupApi.GetGroupAsync(groupId);

        public List<Group> ListGroups() => _groupApi.ListGroups();
        public ThreadTask.Task<List<Group>> ListGroupsAsync() => _groupApi.ListGroupsAsync();

        public void DeleteGroup(string groupId) => _groupApi.DeleteGroup(groupId);
        public ThreadTask.Task DeleteGroupAsync(string groupId) => _groupApi.DeleteGroupAsync(groupId);

        public void AddUserToGroup(string groupId, string userId) => _groupApi.AddUserToGroup(groupId, userId);
        public ThreadTask.Task AddUserToGroupAsync(string groupId, string userId) => _groupApi.AddUserToGroupAsync(groupId, userId);

        public void AddUsersToGroup(List<string> userIds, string groupId) => _groupApi.AddUsersToGroup(userIds, groupId);
        public ThreadTask.Task AddUsersToGroupAsync(List<string> userIds, string groupId) => ThreadTask.Task.Run(() => _groupApi.AddUsersToGroup(userIds, groupId));

        public void RemoveUserFromGroup(string groupId, string userId) => _groupApi.RemoveUserFromGroup(groupId, userId);
        public ThreadTask.Task RemoveUserFromGroupAsync(string groupId, string userId) => _groupApi.RemoveUserFromGroupAsync(groupId, userId);

        public void RemoveUsersFromGroup(List<string> userIds, string groupId) => _groupApi.RemoveUsersFromGroup(userIds, groupId);
        public ThreadTask.Task RemoveUsersFromGroupAsync(List<string> userIds, string groupId) => ThreadTask.Task.Run(() => _groupApi.RemoveUsersFromGroup(userIds, groupId));

        public object GetUsersInGroup(string groupId) => _groupApi.GetUsersInGroup(groupId);
        public ThreadTask.Task<object> GetUsersInGroupAsync(string groupId) => _groupApi.GetUsersInGroupAsync(groupId);

        public object GetGrantedPermissionsForGroup(string groupId) => _groupApi.GetGrantedPermissions(groupId);
        public ThreadTask.Task<object> GetGrantedPermissionsForGroupAsync(string groupId) => _groupApi.GetGrantedPermissionsAsync(groupId);

        public object GetGrantedPermissionsForUser(string userId) => _userApi.GetGrantedPermissions(userId);
        public ThreadTask.Task<object> GetGrantedPermissionsForUserAsync(string userId) => _userApi.GetGrantedPermissionsAsync(userId);

        // Tokens
        public Token GenerateToken(GenerateTokenRequest request) => _tokenApi.GenerateToken(request);
        public ThreadTask.Task<Token> GenerateTokenAsync(GenerateTokenRequest request) => _tokenApi.GenerateTokenAsync(request);

        public object GetUserInfo(bool? claims = null) => _tokenApi.GetUserInfo(claims);
        public ThreadTask.Task<object> GetUserInfoAsync(bool? claims = null) => _tokenApi.GetUserInfoAsync(claims);
    }
}
