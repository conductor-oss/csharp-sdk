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
using System.Linq;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Conductor.Client.Models
{
    /// <summary>
    /// EventHandler
    /// </summary>
    [DataContract]
    public partial class EventHandler : IEquatable<EventHandler>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler" /> class.
        /// </summary>
        /// <param name="actions">actions (required).</param>
        /// <param name="active">active.</param>
        /// <param name="condition">condition.</param>
        /// <param name="evaluatorType">evaluatorType.</param>
        /// <param name="_event">_event (required).</param>
        /// <param name="name">name (required).</param>
        /// <param name="description">description.</param>
        public EventHandler(List<Action> actions = default(List<Action>), bool? active = default(bool?), string condition = default(string), string evaluatorType = default(string), string _event = default(string), string name = default(string), string description = default(string))
        {
            // to ensure "actions" is required (not null)
            if (actions == null)
            {
                throw new InvalidDataException("actions is a required property for EventHandler and cannot be null");
            }
            else
            {
                this.Actions = actions;
            }
            // to ensure "_event" is required (not null)
            if (_event == null)
            {
                throw new InvalidDataException("_event is a required property for EventHandler and cannot be null");
            }
            else
            {
                this._Event = _event;
            }
            // to ensure "name" is required (not null)
            if (name == null)
            {
                throw new InvalidDataException("name is a required property for EventHandler and cannot be null");
            }
            else
            {
                this.Name = name;
            }
            this.Active = active;
            this.Condition = condition;
            this.EvaluatorType = evaluatorType;
            this.Description = description;
        }

        /// <summary>
        /// Gets or Sets Actions
        /// </summary>
        [DataMember(Name = "actions", EmitDefaultValue = false)]
        public List<Action> Actions { get; set; }

        /// <summary>
        /// Gets or Sets Active
        /// </summary>
        [DataMember(Name = "active", EmitDefaultValue = false)]
        public bool? Active { get; set; }

        /// <summary>
        /// Gets or Sets Condition
        /// </summary>
        [DataMember(Name = "condition", EmitDefaultValue = false)]
        public string Condition { get; set; }

        /// <summary>
        /// Gets or Sets EvaluatorType
        /// </summary>
        [DataMember(Name = "evaluatorType", EmitDefaultValue = false)]
        public string EvaluatorType { get; set; }

        /// <summary>
        /// Gets or Sets _Event
        /// </summary>
        [DataMember(Name = "event", EmitDefaultValue = false)]
        public string _Event { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class EventHandler {\n");
            sb.Append("  Actions: ").Append(Actions).Append("\n");
            sb.Append("  Active: ").Append(Active).Append("\n");
            sb.Append("  Condition: ").Append(Condition).Append("\n");
            sb.Append("  EvaluatorType: ").Append(EvaluatorType).Append("\n");
            sb.Append("  _Event: ").Append(_Event).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as EventHandler);
        }

        /// <summary>
        /// Returns true if EventHandler instances are equal
        /// </summary>
        /// <param name="input">Instance of EventHandler to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EventHandler input)
        {
            if (input == null)
                return false;

            return
                (
                    this.Actions == input.Actions ||
                    this.Actions != null &&
                    input.Actions != null &&
                    this.Actions.SequenceEqual(input.Actions)
                ) &&
                (
                    this.Active == input.Active ||
                    (this.Active != null &&
                    this.Active.Equals(input.Active))
                ) &&
                (
                    this.Condition == input.Condition ||
                    (this.Condition != null &&
                    this.Condition.Equals(input.Condition))
                ) &&
                (
                    this.EvaluatorType == input.EvaluatorType ||
                    (this.EvaluatorType != null &&
                    this.EvaluatorType.Equals(input.EvaluatorType))
                ) &&
                (
                    this._Event == input._Event ||
                    (this._Event != null &&
                    this._Event.Equals(input._Event))
                ) &&
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Actions != null)
                    hashCode = hashCode * 59 + this.Actions.GetHashCode();
                if (this.Active != null)
                    hashCode = hashCode * 59 + this.Active.GetHashCode();
                if (this.Condition != null)
                    hashCode = hashCode * 59 + this.Condition.GetHashCode();
                if (this.EvaluatorType != null)
                    hashCode = hashCode * 59 + this.EvaluatorType.GetHashCode();
                if (this._Event != null)
                    hashCode = hashCode * 59 + this._Event.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
