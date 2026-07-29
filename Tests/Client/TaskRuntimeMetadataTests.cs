/*
 * Copyright 2025 Conductor Authors.
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
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using ModelTask = Conductor.Client.Models.Task;

namespace Tests.Client
{
    /// <summary>
    /// The server delivers host-resolved secret values on Task.runtimeMetadata (wire-only, never
    /// persisted) when a worker's TaskDef.runtimeMetadata declares secret names (conductor-oss PR
    /// #1255). Verify the client model round-trips the field and omits it when empty.
    /// </summary>
    public class TaskRuntimeMetadataTests
    {
        [Fact]
        public void RuntimeMetadata_RoundTrips()
        {
            var task = new ModelTask(taskId: "t1")
            {
                RuntimeMetadata = new Dictionary<string, string>
                {
                    { "GITHUB_TOKEN", "ghp_secret" },
                    { "GH_APP_ID", "42" }
                });

            var json = JsonConvert.SerializeObject(task);
            Assert.Contains("\"runtimeMetadata\"", json);

            var back = JsonConvert.DeserializeObject<ModelTask>(json);
            Assert.Equal("ghp_secret", back.RuntimeMetadata["GITHUB_TOKEN"]);
            Assert.Equal("42", back.RuntimeMetadata["GH_APP_ID"]);
        }

        [Fact]
        public void RuntimeMetadata_OmittedWhenEmpty()
        {
            var json = JsonConvert.SerializeObject(new ModelTask(taskId: "t1"));
            Assert.DoesNotContain("runtimeMetadata", json);
        }
    }
}
