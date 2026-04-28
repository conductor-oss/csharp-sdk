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
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Orkes;
using System;
using System.Collections.Generic;

namespace Conductor.Examples.Orkes
{
    /// <summary>
    /// Demonstrates authorization operations: applications, users, groups, permissions.
    /// </summary>
    public class AuthorizationJourney
    {
        private readonly IAuthorizationClient _authClient;
        private const string TEST_APP_NAME = "csharp_test_app";
        private const string TEST_GROUP_ID = "csharp_test_group";

        public AuthorizationJourney()
        {
            var config = ApiExtensions.GetConfiguration();
            var clients = new OrkesClients(config);
            _authClient = clients.GetAuthorizationClient();
        }

        public void Run()
        {
            Console.WriteLine("=== Authorization Journey ===\n");

            // 1. List existing applications
            Console.WriteLine("1. Listing applications...");
            var apps = _authClient.ListApplications();
            Console.WriteLine($"   Found {apps?.Count ?? 0} applications.");

            // 2. Create an application
            Console.WriteLine("2. Creating application...");
            var createReq = new CreateOrUpdateApplicationRequest(name: TEST_APP_NAME);
            var createdApp = _authClient.CreateApplication(createReq);
            Console.WriteLine($"   Application created: {createdApp}");

            // 3. List users
            Console.WriteLine("3. Listing users...");
            var users = _authClient.ListUsers();
            Console.WriteLine($"   Found {users?.Count ?? 0} users.");

            // 4. List groups
            Console.WriteLine("4. Listing groups...");
            var groups = _authClient.ListGroups();
            Console.WriteLine($"   Found {groups?.Count ?? 0} groups.");

            // 5. Create a group
            Console.WriteLine("5. Creating group...");
            var groupReq = new UpsertGroupRequest(description: "Test group from C# SDK");
            var createdGroup = _authClient.UpsertGroup(groupReq, TEST_GROUP_ID);
            Console.WriteLine($"   Group created: {createdGroup}");

            // 6. Get group
            Console.WriteLine("6. Getting group...");
            var fetchedGroup = _authClient.GetGroup(TEST_GROUP_ID);
            Console.WriteLine($"   Group: {fetchedGroup}");

            // 7. Get current user info
            Console.WriteLine("7. Getting current user info...");
            var userInfo = _authClient.GetUserInfo();
            Console.WriteLine($"   User info: {userInfo}");

            // 8. Clean up - delete group
            Console.WriteLine("8. Cleaning up...");
            _authClient.DeleteGroup(TEST_GROUP_ID);
            Console.WriteLine("   Group deleted.");

            // 9. Clean up - delete application (need app ID from creation)
            // Note: In production, you would extract the app ID from createdApp
            Console.WriteLine("   Application cleanup would require the app ID.");

            Console.WriteLine("\nAuthorization Journey completed!");
        }
    }
}
