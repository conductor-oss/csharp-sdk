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
using System.Collections.Generic;

namespace Conductor.Client.Models
{
    public class StateChangeEvent
    {
        /// <summary>
        /// Gets or sets Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets Payload
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangeEvent" /> class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="payload"></param>
        public StateChangeEvent(string type, Dictionary<string, object> payload)
        {
            Type = type;
            Payload = payload;
        }
    }
}
