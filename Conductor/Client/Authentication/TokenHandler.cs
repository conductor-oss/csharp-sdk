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
using conductor.csharp.Client.Extensions;
using Conductor.Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;

namespace Conductor.Client.Authentication
{
    public class TokenHandler
    {
        private static int REFRESH_TOKEN_RETRY_COUNTER_LIMIT = 5;
        private static readonly object _lockObject = new object();
        private readonly MemoryCache _memoryCache;
        private static ILogger _logger;

        // Set to true when the server does not support authentication (OSS mode).
        // Once disabled, auth is skipped for all future requests.
        private bool _authDisabled = false;

        public TokenHandler()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _logger = ApplicationLogging.CreateLogger<TokenHandler>();
        }

        public string GetToken(OrkesAuthenticationSettings authenticationSettings, TokenResourceApi tokenClient)
        {
            if (_authDisabled)
                return null;

            string token = (string)_memoryCache.Get(authenticationSettings);
            if (token != null)
            {
                return token;
            }
            return RefreshToken(authenticationSettings, tokenClient);
        }

        public string RefreshToken(OrkesAuthenticationSettings authenticationSettings, TokenResourceApi tokenClient)
        {
            if (_authDisabled)
                return null;

            lock (_lockObject)
            {
                string token = GetTokenFromServer(authenticationSettings, tokenClient);
                if (token == null)
                    return null;
                var expirationTime = System.DateTimeOffset.Now.AddMinutes(30);
                _memoryCache.Set(authenticationSettings, token, expirationTime);
                return token;
            }
        }

        private string GetTokenFromServer(OrkesAuthenticationSettings authenticationSettings, TokenResourceApi tokenClient)
        {
            var tokenRequest = new Client.Models.GenerateTokenRequest(
                keyId: authenticationSettings.KeyId,
                keySecret: authenticationSettings.KeySecret
            );
            for (int attempt = 0; attempt < REFRESH_TOKEN_RETRY_COUNTER_LIMIT; attempt += 1)
            {
                try
                {
                    var token = tokenClient.GenerateToken(tokenRequest);
                    return token._token;
                }
                catch (ApiException e)
                {
                    if (e.ErrorCode == 404 || e.ErrorCode == 405)
                    {
                        // Server does not support authentication (OSS mode) — disable auth silently.
                        _authDisabled = true;
                        _logger.LogInformation(
                            $"Authentication endpoint not found (HTTP {e.ErrorCode}) at {tokenClient.Configuration.BasePath}. " +
                            "Running in unauthenticated mode (OSS). Auth will be disabled for all requests.");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to refresh authentication token, attempt = {attempt}, error = {e.Message}");
                }
            }
            throw new Exception("Failed to refresh authentication token");
        }
    }
}