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
namespace Conductor.AI.Examples;

internal static class Settings
{
    public static string LlmModel =>
        Environment.GetEnvironmentVariable("CONDUCTOR_AGENT_LLM_MODEL")
        ?? "openai/gpt-4o-mini";

    public static string ServerUrl =>
        Environment.GetEnvironmentVariable("CONDUCTOR_SERVER_URL")
        ?? "http://localhost:8080/api";
}
