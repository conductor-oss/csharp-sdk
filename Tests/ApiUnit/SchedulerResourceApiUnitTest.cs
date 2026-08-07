/*
 * Copyright 2026 Conductor Authors.
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Conductor.Api;
using Conductor.Client;
using conductor_csharp.test.Helper;
using Xunit;

namespace conductor_csharp.test.ApiUnit
{
    [Trait("Category", "Unit")]
    public class SchedulerResourceApiUnitTest
    {
        [Fact]
        public void PauseSchedule_SendsPutFirst()
        {
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(HttpStatusCode.OK, "{}");

            client.PauseSchedule("s1");

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/api/scheduler/schedules/s1/pause", request.RequestUri.AbsolutePath);
        }

        [Fact]
        public void PauseSchedule_FallsBackToGetOn405()
        {
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(req =>
                req.Method == HttpMethod.Put ? (HttpStatusCode.MethodNotAllowed, "") : (HttpStatusCode.OK, "{}"));

            client.PauseSchedule("s1");

            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(HttpMethod.Put, handler.Requests[0].Method);
            Assert.Equal(HttpMethod.Get, handler.Requests[1].Method);
            Assert.Equal("/api/scheduler/schedules/s1/pause", handler.Requests[1].RequestUri.AbsolutePath);
        }

        [Fact]
        public void ResumeSchedule_FallsBackToGetOn405Independently_EachCallRetriesPutFirst()
        {
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(req =>
                req.Method == HttpMethod.Put ? (HttpStatusCode.MethodNotAllowed, "") : (HttpStatusCode.OK, "{}"));

            client.PauseSchedule("s1");
            client.ResumeSchedule("s1");

            Assert.Equal(4, handler.Requests.Count);
            Assert.Equal(HttpMethod.Put, handler.Requests[2].Method);
            Assert.Equal(HttpMethod.Get, handler.Requests[3].Method);
            Assert.Equal("/api/scheduler/schedules/s1/resume", handler.Requests[3].RequestUri.AbsolutePath);
        }

        [Fact]
        public void PauseSchedule_RethrowsOn403WithoutFallingBackToGet()
        {
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(HttpStatusCode.Forbidden, "");

            var ex = Assert.Throws<ApiException>(() => client.PauseSchedule("s1"));

            Assert.Equal(403, ex.ErrorCode);
            Assert.Single(handler.Requests);
        }

        [Fact]
        public void PauseSchedule_RethrowsOn404WithoutFallingBackToGet()
        {
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(HttpStatusCode.NotFound, "");

            var ex = Assert.Throws<ApiException>(() => client.PauseSchedule("s1"));

            Assert.Equal(404, ex.ErrorCode);
            Assert.Single(handler.Requests);
        }

        [Fact]
        public async Task ResumeScheduleAsync_FallsBackToGetOn405()
        {
            var callCount = 0;
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(_ =>
            {
                callCount++;
                return callCount == 1 ? (HttpStatusCode.MethodNotAllowed, "") : (HttpStatusCode.OK, "{}");
            });

            await client.ResumeScheduleAsync("s1");

            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(HttpMethod.Put, handler.Requests[0].Method);
            Assert.Equal(HttpMethod.Get, handler.Requests[1].Method);
            Assert.Equal("/api/scheduler/schedules/s1/resume", handler.Requests[1].RequestUri.AbsolutePath);
        }

        [Fact]
        public async Task ResumeScheduleAsync_RethrowsOn403WithoutFallingBackToGet()
        {
            var (client, handler) = MockApiClient.Build<SchedulerResourceApi>(HttpStatusCode.Forbidden, "");

            var ex = await Assert.ThrowsAsync<ApiException>(() => client.ResumeScheduleAsync("s1"));

            Assert.Equal(403, ex.ErrorCode);
            Assert.Single(handler.Requests);
        }
    }
}
