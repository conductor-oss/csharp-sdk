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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Conductor.Client.Models
{
    /// <summary>
    /// Represents an action to be executed by an EventHandler.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// The type of action to perform.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ActionEnum
        {
            [EnumMember(Value = "start_workflow")]
            StartWorkflow = 1,

            [EnumMember(Value = "complete_task")]
            CompleteTask = 2,

            [EnumMember(Value = "fail_task")]
            FailTask = 3,

            [EnumMember(Value = "update_workflow_variables")]
            UpdateWorkflowVariables = 4,

            [EnumMember(Value = "terminate_workflow")]
            TerminateWorkflow = 5
        }

        /// <summary>
        /// The action type.
        /// </summary>
        [DataMember(Name = "action", EmitDefaultValue = false)]
        public ActionEnum? ActionType { get; set; }

        /// <summary>
        /// Whether to expand inline JSON in the payload.
        /// </summary>
        [DataMember(Name = "expandInlineJSON", EmitDefaultValue = false)]
        public bool? ExpandInlineJSON { get; set; }
    }
}
