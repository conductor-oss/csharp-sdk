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
using Newtonsoft.Json.Converters;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Conductor.Client.Models
{
    /// <summary>
    /// TaskDef
    /// </summary>
    [DataContract]
    public partial class TaskDef : IEquatable<TaskDef>, IValidatableObject
    {
        /// <summary>
        /// Defines RetryLogic
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum RetryLogicEnum
        {
            /// <summary>
            /// Enum FIXED for value: FIXED
            /// </summary>
            [EnumMember(Value = "FIXED")]
            FIXED = 1,
            /// <summary>
            /// Enum EXPONENTIALBACKOFF for value: EXPONENTIAL_BACKOFF
            /// </summary>
            [EnumMember(Value = "EXPONENTIAL_BACKOFF")]
            EXPONENTIALBACKOFF = 2,
            /// <summary>
            /// Enum LINEARBACKOFF for value: LINEAR_BACKOFF
            /// </summary>
            [EnumMember(Value = "LINEAR_BACKOFF")]
            LINEARBACKOFF = 3
        }
        /// <summary>
        /// Gets or Sets RetryLogic
        /// </summary>
        [DataMember(Name = "retryLogic", EmitDefaultValue = false)]
        public RetryLogicEnum? RetryLogic { get; set; }
        /// <summary>
        /// Defines TimeoutPolicy
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TimeoutPolicyEnum
        {
            /// <summary>
            /// Enum RETRY for value: RETRY
            /// </summary>
            [EnumMember(Value = "RETRY")]
            RETRY = 1,
            /// <summary>
            /// Enum TIMEOUTWF for value: TIME_OUT_WF
            /// </summary>
            [EnumMember(Value = "TIME_OUT_WF")]
            TIMEOUTWF = 2,
            /// <summary>
            /// Enum ALERTONLY for value: ALERT_ONLY
            /// </summary>
            [EnumMember(Value = "ALERT_ONLY")]
            ALERTONLY = 3
        }
        /// <summary>
        /// Gets or Sets TimeoutPolicy
        /// </summary>
        [DataMember(Name = "timeoutPolicy", EmitDefaultValue = false)]
        public TimeoutPolicyEnum? TimeoutPolicy { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDef" /> class.
        /// </summary>
        /// <param name="backoffScaleFactor">backoffScaleFactor.</param>
        /// <param name="concurrentExecLimit">concurrentExecLimit.</param>
        /// <param name="createTime">createTime.</param>
        /// <param name="createdBy">createdBy.</param>
        /// <param name="description">description.</param>
        /// <param name="executionNameSpace">executionNameSpace.</param>
        /// <param name="inputKeys">inputKeys.</param>
        /// <param name="inputTemplate">inputTemplate.</param>
        /// <param name="isolationGroupId">isolationGroupId.</param>
        /// <param name="name">name (required).</param>
        /// <param name="outputKeys">outputKeys.</param>
        /// <param name="ownerApp">ownerApp.</param>
        /// <param name="ownerEmail">ownerEmail.</param>
        /// <param name="pollTimeoutSeconds">pollTimeoutSeconds.</param>
        /// <param name="rateLimitFrequencyInSeconds">rateLimitFrequencyInSeconds.</param>
        /// <param name="rateLimitPerFrequency">rateLimitPerFrequency.</param>
        /// <param name="responseTimeoutSeconds">responseTimeoutSeconds.</param>
        /// <param name="retryCount">retryCount.</param>
        /// <param name="retryDelaySeconds">retryDelaySeconds.</param>
        /// <param name="retryLogic">retryLogic.</param>
        /// <param name="timeoutPolicy">timeoutPolicy.</param>
        /// <param name="timeoutSeconds">timeoutSeconds (required).</param>
        /// <param name="updateTime">updateTime.</param>
        /// <param name="updatedBy">updatedBy.</param>
        public TaskDef(int? backoffScaleFactor = default(int?), int? concurrentExecLimit = default(int?), long? createTime = default(long?), string createdBy = default(string), string description = default(string), string executionNameSpace = default(string), List<string> inputKeys = default(List<string>), Dictionary<string, Object> inputTemplate = default(Dictionary<string, Object>), string isolationGroupId = default(string), string name = default(string), List<string> outputKeys = default(List<string>), string ownerApp = default(string), string ownerEmail = default(string), int? pollTimeoutSeconds = default(int?), int? rateLimitFrequencyInSeconds = default(int?), int? rateLimitPerFrequency = default(int?), long? responseTimeoutSeconds = default(long?), int? retryCount = default(int?), int? retryDelaySeconds = default(int?), RetryLogicEnum? retryLogic = default(RetryLogicEnum?), TimeoutPolicyEnum? timeoutPolicy = default(TimeoutPolicyEnum?), long? timeoutSeconds = default(long?), long? updateTime = default(long?), string updatedBy = default(string))
        {
            this.Name = name;
            this.TimeoutSeconds = timeoutSeconds;
            this.BackoffScaleFactor = backoffScaleFactor;
            this.ConcurrentExecLimit = concurrentExecLimit;
            this.CreateTime = createTime;
            this.CreatedBy = createdBy;
            this.Description = description;
            this.ExecutionNameSpace = executionNameSpace;
            this.InputKeys = inputKeys;
            this.InputTemplate = inputTemplate;
            this.IsolationGroupId = isolationGroupId;
            this.OutputKeys = outputKeys;
            this.OwnerApp = ownerApp;
            this.OwnerEmail = ownerEmail;
            this.PollTimeoutSeconds = pollTimeoutSeconds;
            this.RateLimitFrequencyInSeconds = rateLimitFrequencyInSeconds;
            this.RateLimitPerFrequency = rateLimitPerFrequency;
            this.ResponseTimeoutSeconds = responseTimeoutSeconds;
            this.RetryCount = retryCount;
            this.RetryDelaySeconds = retryDelaySeconds;
            this.RetryLogic = retryLogic;
            this.TimeoutPolicy = timeoutPolicy;
            this.UpdateTime = updateTime;
            this.UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Gets or Sets BackoffScaleFactor
        /// </summary>
        [DataMember(Name = "backoffScaleFactor", EmitDefaultValue = false)]
        public int? BackoffScaleFactor { get; set; }

        /// <summary>
        /// Gets or Sets ConcurrentExecLimit
        /// </summary>
        [DataMember(Name = "concurrentExecLimit", EmitDefaultValue = false)]
        public int? ConcurrentExecLimit { get; set; }

        /// <summary>
        /// Gets or Sets CreateTime
        /// </summary>
        [DataMember(Name = "createTime", EmitDefaultValue = false)]
        public long? CreateTime { get; set; }

        /// <summary>
        /// Gets or Sets CreatedBy
        /// </summary>
        [DataMember(Name = "createdBy", EmitDefaultValue = false)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets ExecutionNameSpace
        /// </summary>
        [DataMember(Name = "executionNameSpace", EmitDefaultValue = false)]
        public string ExecutionNameSpace { get; set; }

        /// <summary>
        /// Gets or Sets InputKeys
        /// </summary>
        [DataMember(Name = "inputKeys", EmitDefaultValue = false)]
        public List<string> InputKeys { get; set; }

        /// <summary>
        /// Gets or Sets InputTemplate
        /// </summary>
        [DataMember(Name = "inputTemplate", EmitDefaultValue = false)]
        public Dictionary<string, Object> InputTemplate { get; set; }

        /// <summary>
        /// Gets or Sets IsolationGroupId
        /// </summary>
        [DataMember(Name = "isolationGroupId", EmitDefaultValue = false)]
        public string IsolationGroupId { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets OutputKeys
        /// </summary>
        [DataMember(Name = "outputKeys", EmitDefaultValue = false)]
        public List<string> OutputKeys { get; set; }

        /// <summary>
        /// Names of secrets this task type requires — stamped by the SDK at
        /// registration time and delivered on the polled Task by the server
        /// (spec R6, wire-only credential contract).
        /// </summary>
        [DataMember(Name = "runtimeMetadata", EmitDefaultValue = false)]
        public List<string> RuntimeMetadata { get; set; }

        /// <summary>
        /// Gets or Sets OwnerApp
        /// </summary>
        [DataMember(Name = "ownerApp", EmitDefaultValue = false)]
        public string OwnerApp { get; set; }

        /// <summary>
        /// Gets or Sets OwnerEmail
        /// </summary>
        [DataMember(Name = "ownerEmail", EmitDefaultValue = false)]
        public string OwnerEmail { get; set; }

        /// <summary>
        /// Gets or Sets PollTimeoutSeconds
        /// </summary>
        [DataMember(Name = "pollTimeoutSeconds", EmitDefaultValue = false)]
        public int? PollTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or Sets RateLimitFrequencyInSeconds
        /// </summary>
        [DataMember(Name = "rateLimitFrequencyInSeconds", EmitDefaultValue = false)]
        public int? RateLimitFrequencyInSeconds { get; set; }

        /// <summary>
        /// Gets or Sets RateLimitPerFrequency
        /// </summary>
        [DataMember(Name = "rateLimitPerFrequency", EmitDefaultValue = false)]
        public int? RateLimitPerFrequency { get; set; }

        /// <summary>
        /// Gets or Sets ResponseTimeoutSeconds
        /// </summary>
        [DataMember(Name = "responseTimeoutSeconds", EmitDefaultValue = false)]
        public long? ResponseTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or Sets RetryCount
        /// </summary>
        [DataMember(Name = "retryCount", EmitDefaultValue = false)]
        public int? RetryCount { get; set; }

        /// <summary>
        /// Gets or Sets RetryDelaySeconds
        /// </summary>
        [DataMember(Name = "retryDelaySeconds", EmitDefaultValue = false)]
        public int? RetryDelaySeconds { get; set; }



        /// <summary>
        /// Gets or Sets TimeoutSeconds
        /// </summary>
        [DataMember(Name = "timeoutSeconds", EmitDefaultValue = false)]
        public long? TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or Sets UpdateTime
        /// </summary>
        [DataMember(Name = "updateTime", EmitDefaultValue = false)]
        public long? UpdateTime { get; set; }

        /// <summary>
        /// Gets or Sets UpdatedBy
        /// </summary>
        [DataMember(Name = "updatedBy", EmitDefaultValue = false)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TaskDef {\n");
            sb.Append("  BackoffScaleFactor: ").Append(BackoffScaleFactor).Append("\n");
            sb.Append("  ConcurrentExecLimit: ").Append(ConcurrentExecLimit).Append("\n");
            sb.Append("  CreateTime: ").Append(CreateTime).Append("\n");
            sb.Append("  CreatedBy: ").Append(CreatedBy).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  ExecutionNameSpace: ").Append(ExecutionNameSpace).Append("\n");
            sb.Append("  InputKeys: ").Append(InputKeys).Append("\n");
            sb.Append("  InputTemplate: ").Append(InputTemplate).Append("\n");
            sb.Append("  IsolationGroupId: ").Append(IsolationGroupId).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  OutputKeys: ").Append(OutputKeys).Append("\n");
            sb.Append("  RuntimeMetadata: ").Append(RuntimeMetadata).Append("\n");
            sb.Append("  OwnerApp: ").Append(OwnerApp).Append("\n");
            sb.Append("  OwnerEmail: ").Append(OwnerEmail).Append("\n");
            sb.Append("  PollTimeoutSeconds: ").Append(PollTimeoutSeconds).Append("\n");
            sb.Append("  RateLimitFrequencyInSeconds: ").Append(RateLimitFrequencyInSeconds).Append("\n");
            sb.Append("  RateLimitPerFrequency: ").Append(RateLimitPerFrequency).Append("\n");
            sb.Append("  ResponseTimeoutSeconds: ").Append(ResponseTimeoutSeconds).Append("\n");
            sb.Append("  RetryCount: ").Append(RetryCount).Append("\n");
            sb.Append("  RetryDelaySeconds: ").Append(RetryDelaySeconds).Append("\n");
            sb.Append("  RetryLogic: ").Append(RetryLogic).Append("\n");
            sb.Append("  TimeoutPolicy: ").Append(TimeoutPolicy).Append("\n");
            sb.Append("  TimeoutSeconds: ").Append(TimeoutSeconds).Append("\n");
            sb.Append("  UpdateTime: ").Append(UpdateTime).Append("\n");
            sb.Append("  UpdatedBy: ").Append(UpdatedBy).Append("\n");
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
            return this.Equals(input as TaskDef);
        }

        /// <summary>
        /// Returns true if TaskDef instances are equal
        /// </summary>
        /// <param name="input">Instance of TaskDef to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TaskDef input)
        {
            if (input == null)
                return false;

            return
                (
                    this.BackoffScaleFactor == input.BackoffScaleFactor ||
                    (this.BackoffScaleFactor != null &&
                    this.BackoffScaleFactor.Equals(input.BackoffScaleFactor))
                ) &&
                (
                    this.ConcurrentExecLimit == input.ConcurrentExecLimit ||
                    (this.ConcurrentExecLimit != null &&
                    this.ConcurrentExecLimit.Equals(input.ConcurrentExecLimit))
                ) &&
                (
                    this.CreateTime == input.CreateTime ||
                    (this.CreateTime != null &&
                    this.CreateTime.Equals(input.CreateTime))
                ) &&
                (
                    this.CreatedBy == input.CreatedBy ||
                    (this.CreatedBy != null &&
                    this.CreatedBy.Equals(input.CreatedBy))
                ) &&
                (
                    this.Description == input.Description ||
                    (this.Description != null &&
                    this.Description.Equals(input.Description))
                ) &&
                (
                    this.ExecutionNameSpace == input.ExecutionNameSpace ||
                    (this.ExecutionNameSpace != null &&
                    this.ExecutionNameSpace.Equals(input.ExecutionNameSpace))
                ) &&
                (
                    this.InputKeys == input.InputKeys ||
                    this.InputKeys != null &&
                    input.InputKeys != null &&
                    this.InputKeys.SequenceEqual(input.InputKeys)
                ) &&
                (
                    this.InputTemplate == input.InputTemplate ||
                    this.InputTemplate != null &&
                    input.InputTemplate != null &&
                    this.InputTemplate.SequenceEqual(input.InputTemplate)
                ) &&
                (
                    this.IsolationGroupId == input.IsolationGroupId ||
                    (this.IsolationGroupId != null &&
                    this.IsolationGroupId.Equals(input.IsolationGroupId))
                ) &&
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) &&
                (
                    this.OutputKeys == input.OutputKeys ||
                    this.OutputKeys != null &&
                    input.OutputKeys != null &&
                    this.OutputKeys.SequenceEqual(input.OutputKeys)
                ) &&
                (
                    this.RuntimeMetadata == input.RuntimeMetadata ||
                    this.RuntimeMetadata != null &&
                    input.RuntimeMetadata != null &&
                    this.RuntimeMetadata.SequenceEqual(input.RuntimeMetadata)
                ) &&
                (
                    this.OwnerApp == input.OwnerApp ||
                    (this.OwnerApp != null &&
                    this.OwnerApp.Equals(input.OwnerApp))
                ) &&
                (
                    this.OwnerEmail == input.OwnerEmail ||
                    (this.OwnerEmail != null &&
                    this.OwnerEmail.Equals(input.OwnerEmail))
                ) &&
                (
                    this.PollTimeoutSeconds == input.PollTimeoutSeconds ||
                    (this.PollTimeoutSeconds != null &&
                    this.PollTimeoutSeconds.Equals(input.PollTimeoutSeconds))
                ) &&
                (
                    this.RateLimitFrequencyInSeconds == input.RateLimitFrequencyInSeconds ||
                    (this.RateLimitFrequencyInSeconds != null &&
                    this.RateLimitFrequencyInSeconds.Equals(input.RateLimitFrequencyInSeconds))
                ) &&
                (
                    this.RateLimitPerFrequency == input.RateLimitPerFrequency ||
                    (this.RateLimitPerFrequency != null &&
                    this.RateLimitPerFrequency.Equals(input.RateLimitPerFrequency))
                ) &&
                (
                    this.ResponseTimeoutSeconds == input.ResponseTimeoutSeconds ||
                    (this.ResponseTimeoutSeconds != null &&
                    this.ResponseTimeoutSeconds.Equals(input.ResponseTimeoutSeconds))
                ) &&
                (
                    this.RetryCount == input.RetryCount ||
                    (this.RetryCount != null &&
                    this.RetryCount.Equals(input.RetryCount))
                ) &&
                (
                    this.RetryDelaySeconds == input.RetryDelaySeconds ||
                    (this.RetryDelaySeconds != null &&
                    this.RetryDelaySeconds.Equals(input.RetryDelaySeconds))
                ) &&
                (
                    this.RetryLogic == input.RetryLogic ||
                    (this.RetryLogic != null &&
                    this.RetryLogic.Equals(input.RetryLogic))
                ) &&
                (
                    this.TimeoutPolicy == input.TimeoutPolicy ||
                    (this.TimeoutPolicy != null &&
                    this.TimeoutPolicy.Equals(input.TimeoutPolicy))
                ) &&
                (
                    this.TimeoutSeconds == input.TimeoutSeconds ||
                    (this.TimeoutSeconds != null &&
                    this.TimeoutSeconds.Equals(input.TimeoutSeconds))
                ) &&
                (
                    this.UpdateTime == input.UpdateTime ||
                    (this.UpdateTime != null &&
                    this.UpdateTime.Equals(input.UpdateTime))
                ) &&
                (
                    this.UpdatedBy == input.UpdatedBy ||
                    (this.UpdatedBy != null &&
                    this.UpdatedBy.Equals(input.UpdatedBy))
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
                if (this.BackoffScaleFactor != null)
                    hashCode = hashCode * 59 + this.BackoffScaleFactor.GetHashCode();
                if (this.ConcurrentExecLimit != null)
                    hashCode = hashCode * 59 + this.ConcurrentExecLimit.GetHashCode();
                if (this.CreateTime != null)
                    hashCode = hashCode * 59 + this.CreateTime.GetHashCode();
                if (this.CreatedBy != null)
                    hashCode = hashCode * 59 + this.CreatedBy.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.ExecutionNameSpace != null)
                    hashCode = hashCode * 59 + this.ExecutionNameSpace.GetHashCode();
                if (this.InputKeys != null)
                    hashCode = hashCode * 59 + this.InputKeys.GetHashCode();
                if (this.InputTemplate != null)
                    hashCode = hashCode * 59 + this.InputTemplate.GetHashCode();
                if (this.IsolationGroupId != null)
                    hashCode = hashCode * 59 + this.IsolationGroupId.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.OutputKeys != null)
                    hashCode = hashCode * 59 + this.OutputKeys.GetHashCode();
                if (this.RuntimeMetadata != null)
                    hashCode = hashCode * 59 + this.RuntimeMetadata.GetHashCode();
                if (this.OwnerApp != null)
                    hashCode = hashCode * 59 + this.OwnerApp.GetHashCode();
                if (this.OwnerEmail != null)
                    hashCode = hashCode * 59 + this.OwnerEmail.GetHashCode();
                if (this.PollTimeoutSeconds != null)
                    hashCode = hashCode * 59 + this.PollTimeoutSeconds.GetHashCode();
                if (this.RateLimitFrequencyInSeconds != null)
                    hashCode = hashCode * 59 + this.RateLimitFrequencyInSeconds.GetHashCode();
                if (this.RateLimitPerFrequency != null)
                    hashCode = hashCode * 59 + this.RateLimitPerFrequency.GetHashCode();
                if (this.ResponseTimeoutSeconds != null)
                    hashCode = hashCode * 59 + this.ResponseTimeoutSeconds.GetHashCode();
                if (this.RetryCount != null)
                    hashCode = hashCode * 59 + this.RetryCount.GetHashCode();
                if (this.RetryDelaySeconds != null)
                    hashCode = hashCode * 59 + this.RetryDelaySeconds.GetHashCode();
                if (this.RetryLogic != null)
                    hashCode = hashCode * 59 + this.RetryLogic.GetHashCode();
                if (this.TimeoutPolicy != null)
                    hashCode = hashCode * 59 + this.TimeoutPolicy.GetHashCode();
                if (this.TimeoutSeconds != null)
                    hashCode = hashCode * 59 + this.TimeoutSeconds.GetHashCode();
                if (this.UpdateTime != null)
                    hashCode = hashCode * 59 + this.UpdateTime.GetHashCode();
                if (this.UpdatedBy != null)
                    hashCode = hashCode * 59 + this.UpdatedBy.GetHashCode();
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
