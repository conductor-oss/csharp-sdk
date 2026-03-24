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
using Conductor.Client.Models;
using System.Collections.Generic;
using Tests.Integration.Helpers;
using Xunit;

namespace Tests.Integration.Event
{
    [Collection("Integration")]
    [Trait("Category", "Integration")]
    public class EventHandlerTests : IClassFixture<ConductorFixture>
    {
        private readonly EventResourceApi _eventClient;
        private readonly string _handlerName;
        private readonly string _eventName;

        public EventHandlerTests(ConductorFixture fixture)
        {
            _eventClient = fixture.Configuration.GetClient<EventResourceApi>();
            _handlerName = TestPrefix.Name("event_handler");
            _eventName = $"conductor:{TestPrefix.Name("queue")}";
        }

        [Fact(Skip = "SDK Actions type (System.Action) cannot be serialized by RestSharp")]
        public void AddEventHandler_CanBeRetrieved()
        {
            Add();
            var handlers = _eventClient.GetEventHandlersForEvent(_eventName);
            Assert.Contains(handlers, h => h.Name == _handlerName);
            Cleanup();
        }

        [Fact(Skip = "SDK Actions type (System.Action) cannot be serialized by RestSharp")]
        public void UpdateEventHandler_ChangesAreReflected()
        {
            Add();
            var updated = Build();
            updated.Active = false;
            _eventClient.UpdateEventHandler(updated);

            var handlers = _eventClient.GetEventHandlersForEvent(_eventName);
            var handler = handlers.Find(h => h.Name == _handlerName);
            Assert.NotNull(handler);
            Assert.False(handler.Active);
            Cleanup();
        }

        [Fact(Skip = "SDK Actions type (System.Action) cannot be serialized by RestSharp")]
        public void GetAllEventHandlers_ContainsAdded()
        {
            Add();
            var all = _eventClient.GetEventHandlers();
            Assert.Contains(all, h => h.Name == _handlerName);
            Cleanup();
        }

        [Fact(Skip = "SDK Actions type (System.Action) cannot be serialized by RestSharp")]
        public void DeleteEventHandler_RemovedFromList()
        {
            Add();
            _eventClient.RemoveEventHandlerStatus(_handlerName);
            var all = _eventClient.GetEventHandlers();
            Assert.DoesNotContain(all, h => h.Name == _handlerName);
        }

        private EventHandler Build() => new EventHandler(
            actions: new List<System.Action>(),
            _event: _eventName,
            name: _handlerName,
            active: true
        );

        private void Add() => _eventClient.AddEventHandler(Build());

        private void Cleanup()
        {
            try { _eventClient.RemoveEventHandlerStatus(_handlerName); } catch { }
        }
    }
}
