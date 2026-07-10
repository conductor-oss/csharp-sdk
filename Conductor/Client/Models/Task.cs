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
    /// Task
    /// </summary>
    [DataContract]
    public partial class Task : IEquatable<Task>, IValidatableObject
    {
        /// <summary>
        /// Defines Status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusEnum
        {
            /// <summary>
            /// Enum INPROGRESS for value: IN_PROGRESS
            /// </summary>
            [EnumMember(Value = "IN_PROGRESS")]
            INPROGRESS = 1,
            /// <summary>
            /// Enum CANCELED for value: CANCELED
            /// </summary>
            [EnumMember(Value = "CANCELED")]
            CANCELED = 2,
            /// <summary>
            /// Enum FAILED for value: FAILED
            /// </summary>
            [EnumMember(Value = "FAILED")]
            FAILED = 3,
            /// <summary>
            /// Enum FAILEDWITHTERMINALERROR for value: FAILED_WITH_TERMINAL_ERROR
            /// </summary>
            [EnumMember(Value = "FAILED_WITH_TERMINAL_ERROR")]
            FAILEDWITHTERMINALERROR = 4,
            /// <summary>
            /// Enum COMPLETED for value: COMPLETED
            /// </summary>
            [EnumMember(Value = "COMPLETED")]
            COMPLETED = 5,
            /// <summary>
            /// Enum COMPLETEDWITHERRORS for value: COMPLETED_WITH_ERRORS
            /// </summary>
            [EnumMember(Value = "COMPLETED_WITH_ERRORS")]
            COMPLETEDWITHERRORS = 6,
            /// <summary>
            /// Enum SCHEDULED for value: SCHEDULED
            /// </summary>
            [EnumMember(Value = "SCHEDULED")]
            SCHEDULED = 7,
            /// <summary>
            /// Enum TIMEDOUT for value: TIMED_OUT
            /// </summary>
            [EnumMember(Value = "TIMED_OUT")]
            TIMEDOUT = 8,
            /// <summary>
            /// Enum SKIPPED for value: SKIPPED
            /// </summary>
            [EnumMember(Value = "SKIPPED")]
            SKIPPED = 9
        }
        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status", EmitDefaultValue = false)]
        public StatusEnum? Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Task" /> class.
        /// </summary>
        /// <param name="callbackAfterSeconds">callbackAfterSeconds.</param>
        /// <param name="callbackFromWorker">callbackFromWorker.</param>
        /// <param name="correlationId">correlationId.</param>
        /// <param name="domain">domain.</param>
        /// <param name="endTime">endTime.</param>
        /// <param name="executed">executed.</param>
        /// <param name="executionNameSpace">executionNameSpace.</param>
        /// <param name="externalInputPayloadStoragePath">externalInputPayloadStoragePath.</param>
        /// <param name="externalOutputPayloadStoragePath">externalOutputPayloadStoragePath.</param>
        /// <param name="inputData">inputData.</param>
        /// <param name="isolationGroupId">isolationGroupId.</param>
        /// <param name="iteration">iteration.</param>
        /// <param name="loopOverTask">loopOverTask.</param>
        /// <param name="outputData">outputData.</param>
        /// <param name="pollCount">pollCount.</param>
        /// <param name="queueWaitTime">queueWaitTime.</param>
        /// <param name="rateLimitFrequencyInSeconds">rateLimitFrequencyInSeconds.</param>
        /// <param name="rateLimitPerFrequency">rateLimitPerFrequency.</param>
        /// <param name="reasonForIncompletion">reasonForIncompletion.</param>
        /// <param name="referenceTaskName">referenceTaskName.</param>
        /// <param name="responseTimeoutSeconds">responseTimeoutSeconds.</param>
        /// <param name="retried">retried.</param>
        /// <param name="retriedTaskId">retriedTaskId.</param>
        /// <param name="retryCount">retryCount.</param>
        /// <param name="scheduledTime">scheduledTime.</param>
        /// <param name="seq">seq.</param>
        /// <param name="startDelayInSeconds">startDelayInSeconds.</param>
        /// <param name="startTime">startTime.</param>
        /// <param name="status">status.</param>
        /// <param name="subWorkflowId">subWorkflowId.</param>
        /// <param name="subworkflowChanged">subworkflowChanged.</param>
        /// <param name="taskDefName">taskDefName.</param>
        /// <param name="taskDefinition">taskDefinition.</param>
        /// <param name="taskId">taskId.</param>
        /// <param name="taskType">taskType.</param>
        /// <param name="updateTime">updateTime.</param>
        /// <param name="workerId">workerId.</param>
        /// <param name="workflowInstanceId">workflowInstanceId.</param>
        /// <param name="workflowPriority">workflowPriority.</param>
        /// <param name="workflowTask">workflowTask.</param>
        /// <param name="workflowType">workflowType.</param>
        public Task(long? callbackAfterSeconds = default(long?), bool? callbackFromWorker = default(bool?), string correlationId = default(string), string domain = default(string), long? endTime = default(long?), bool? executed = default(bool?), string executionNameSpace = default(string), string externalInputPayloadStoragePath = default(string), string externalOutputPayloadStoragePath = default(string), Dictionary<string, Object> inputData = default(Dictionary<string, Object>), string isolationGroupId = default(string), int? iteration = default(int?), bool? loopOverTask = default(bool?), Dictionary<string, Object> outputData = default(Dictionary<string, Object>), int? pollCount = default(int?), long? queueWaitTime = default(long?), int? rateLimitFrequencyInSeconds = default(int?), int? rateLimitPerFrequency = default(int?), string reasonForIncompletion = default(string), string referenceTaskName = default(string), long? responseTimeoutSeconds = default(long?), bool? retried = default(bool?), string retriedTaskId = default(string), int? retryCount = default(int?), long? scheduledTime = default(long?), int? seq = default(int?), int? startDelayInSeconds = default(int?), long? startTime = default(long?), StatusEnum? status = default(StatusEnum?), string subWorkflowId = default(string), bool? subworkflowChanged = default(bool?), string taskDefName = default(string), TaskDef taskDefinition = default(TaskDef), string taskId = default(string), string taskType = default(string), long? updateTime = default(long?), string workerId = default(string), string workflowInstanceId = default(string), int? workflowPriority = default(int?), WorkflowTask workflowTask = default(WorkflowTask), string workflowType = default(string), Dictionary<string, string> runtimeMetadata = default(Dictionary<string, string>))
        {
            this.CallbackAfterSeconds = callbackAfterSeconds;
            this.CallbackFromWorker = callbackFromWorker;
            this.CorrelationId = correlationId;
            this.Domain = domain;
            this.EndTime = endTime;
            this.Executed = executed;
            this.ExecutionNameSpace = executionNameSpace;
            this.ExternalInputPayloadStoragePath = externalInputPayloadStoragePath;
            this.ExternalOutputPayloadStoragePath = externalOutputPayloadStoragePath;
            this.InputData = inputData;
            this.IsolationGroupId = isolationGroupId;
            this.Iteration = iteration;
            this.LoopOverTask = loopOverTask;
            this.OutputData = outputData;
            this.PollCount = pollCount;
            this.QueueWaitTime = queueWaitTime;
            this.RateLimitFrequencyInSeconds = rateLimitFrequencyInSeconds;
            this.RateLimitPerFrequency = rateLimitPerFrequency;
            this.ReasonForIncompletion = reasonForIncompletion;
            this.ReferenceTaskName = referenceTaskName;
            this.ResponseTimeoutSeconds = responseTimeoutSeconds;
            this.Retried = retried;
            this.RetriedTaskId = retriedTaskId;
            this.RetryCount = retryCount;
            this.ScheduledTime = scheduledTime;
            this.Seq = seq;
            this.StartDelayInSeconds = startDelayInSeconds;
            this.StartTime = startTime;
            this.Status = status;
            this.SubWorkflowId = subWorkflowId;
            this.SubworkflowChanged = subworkflowChanged;
            this.TaskDefName = taskDefName;
            this.TaskDefinition = taskDefinition;
            this.TaskId = taskId;
            this.TaskType = taskType;
            this.UpdateTime = updateTime;
            this.WorkerId = workerId;
            this.WorkflowInstanceId = workflowInstanceId;
            this.WorkflowPriority = workflowPriority;
            this.WorkflowTask = workflowTask;
            this.WorkflowType = workflowType;
            this.RuntimeMetadata = runtimeMetadata;
        }

        /// <summary>
        /// Gets or Sets CallbackAfterSeconds
        /// </summary>
        [DataMember(Name = "callbackAfterSeconds", EmitDefaultValue = false)]
        public long? CallbackAfterSeconds { get; set; }

        /// <summary>
        /// Gets or Sets CallbackFromWorker
        /// </summary>
        [DataMember(Name = "callbackFromWorker", EmitDefaultValue = false)]
        public bool? CallbackFromWorker { get; set; }

        /// <summary>
        /// Gets or Sets CorrelationId
        /// </summary>
        [DataMember(Name = "correlationId", EmitDefaultValue = false)]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or Sets Domain
        /// </summary>
        [DataMember(Name = "domain", EmitDefaultValue = false)]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or Sets EndTime
        /// </summary>
        [DataMember(Name = "endTime", EmitDefaultValue = false)]
        public long? EndTime { get; set; }

        /// <summary>
        /// Gets or Sets Executed
        /// </summary>
        [DataMember(Name = "executed", EmitDefaultValue = false)]
        public bool? Executed { get; set; }

        /// <summary>
        /// Gets or Sets ExecutionNameSpace
        /// </summary>
        [DataMember(Name = "executionNameSpace", EmitDefaultValue = false)]
        public string ExecutionNameSpace { get; set; }

        /// <summary>
        /// Gets or Sets ExternalInputPayloadStoragePath
        /// </summary>
        [DataMember(Name = "externalInputPayloadStoragePath", EmitDefaultValue = false)]
        public string ExternalInputPayloadStoragePath { get; set; }

        /// <summary>
        /// Gets or Sets ExternalOutputPayloadStoragePath
        /// </summary>
        [DataMember(Name = "externalOutputPayloadStoragePath", EmitDefaultValue = false)]
        public string ExternalOutputPayloadStoragePath { get; set; }

        /// <summary>
        /// Gets or Sets InputData
        /// </summary>
        [DataMember(Name = "inputData", EmitDefaultValue = false)]
        public Dictionary<string, Object> InputData { get; set; }

        /// <summary>
        /// Gets or Sets IsolationGroupId
        /// </summary>
        [DataMember(Name = "isolationGroupId", EmitDefaultValue = false)]
        public string IsolationGroupId { get; set; }

        /// <summary>
        /// Gets or Sets Iteration
        /// </summary>
        [DataMember(Name = "iteration", EmitDefaultValue = false)]
        public int? Iteration { get; set; }

        /// <summary>
        /// Gets or Sets LoopOverTask
        /// </summary>
        [DataMember(Name = "loopOverTask", EmitDefaultValue = false)]
        public bool? LoopOverTask { get; set; }

        /// <summary>
        /// Gets or Sets OutputData
        /// </summary>
        [DataMember(Name = "outputData", EmitDefaultValue = false)]
        public Dictionary<string, Object> OutputData { get; set; }

        /// <summary>
        /// Secret values the server resolved for this task at poll time and delivered on the wire
        /// only (never persisted). Populated when the task's TaskDef.runtimeMetadata declares secret
        /// names the host resolves from its secret store (conductor-oss PR #1255). Empty/absent otherwise.
        /// </summary>
        [DataMember(Name = "runtimeMetadata", EmitDefaultValue = false)]
        public Dictionary<string, string> RuntimeMetadata { get; set; }

        /// <summary>
        /// Gets or Sets PollCount
        /// </summary>
        [DataMember(Name = "pollCount", EmitDefaultValue = false)]
        public int? PollCount { get; set; }

        /// <summary>
        /// Gets or Sets QueueWaitTime
        /// </summary>
        [DataMember(Name = "queueWaitTime", EmitDefaultValue = false)]
        public long? QueueWaitTime { get; set; }

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
        /// Gets or Sets ReasonForIncompletion
        /// </summary>
        [DataMember(Name = "reasonForIncompletion", EmitDefaultValue = false)]
        public string ReasonForIncompletion { get; set; }

        /// <summary>
        /// Gets or Sets ReferenceTaskName
        /// </summary>
        [DataMember(Name = "referenceTaskName", EmitDefaultValue = false)]
        public string ReferenceTaskName { get; set; }

        /// <summary>
        /// Gets or Sets ResponseTimeoutSeconds
        /// </summary>
        [DataMember(Name = "responseTimeoutSeconds", EmitDefaultValue = false)]
        public long? ResponseTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or Sets Retried
        /// </summary>
        [DataMember(Name = "retried", EmitDefaultValue = false)]
        public bool? Retried { get; set; }

        /// <summary>
        /// Gets or Sets RetriedTaskId
        /// </summary>
        [DataMember(Name = "retriedTaskId", EmitDefaultValue = false)]
        public string RetriedTaskId { get; set; }

        /// <summary>
        /// Gets or Sets RetryCount
        /// </summary>
        [DataMember(Name = "retryCount", EmitDefaultValue = false)]
        public int? RetryCount { get; set; }

        /// <summary>
        /// Gets or Sets ScheduledTime
        /// </summary>
        [DataMember(Name = "scheduledTime", EmitDefaultValue = false)]
        public long? ScheduledTime { get; set; }

        /// <summary>
        /// Gets or Sets Seq
        /// </summary>
        [DataMember(Name = "seq", EmitDefaultValue = false)]
        public int? Seq { get; set; }

        /// <summary>
        /// Gets or Sets StartDelayInSeconds
        /// </summary>
        [DataMember(Name = "startDelayInSeconds", EmitDefaultValue = false)]
        public int? StartDelayInSeconds { get; set; }

        /// <summary>
        /// Gets or Sets StartTime
        /// </summary>
        [DataMember(Name = "startTime", EmitDefaultValue = false)]
        public long? StartTime { get; set; }


        /// <summary>
        /// Gets or Sets SubWorkflowId
        /// </summary>
        [DataMember(Name = "subWorkflowId", EmitDefaultValue = false)]
        public string SubWorkflowId { get; set; }

        /// <summary>
        /// Gets or Sets SubworkflowChanged
        /// </summary>
        [DataMember(Name = "subworkflowChanged", EmitDefaultValue = false)]
        public bool? SubworkflowChanged { get; set; }

        /// <summary>
        /// Gets or Sets TaskDefName
        /// </summary>
        [DataMember(Name = "taskDefName", EmitDefaultValue = false)]
        public string TaskDefName { get; set; }

        /// <summary>
        /// Gets or Sets TaskDefinition
        /// </summary>
        [DataMember(Name = "taskDefinition", EmitDefaultValue = false)]
        public TaskDef TaskDefinition { get; set; }

        /// <summary>
        /// Gets or Sets TaskId
        /// </summary>
        [DataMember(Name = "taskId", EmitDefaultValue = false)]
        public string TaskId { get; set; }

        /// <summary>
        /// Gets or Sets TaskType
        /// </summary>
        [DataMember(Name = "taskType", EmitDefaultValue = false)]
        public string TaskType { get; set; }

        /// <summary>
        /// Gets or Sets UpdateTime
        /// </summary>
        [DataMember(Name = "updateTime", EmitDefaultValue = false)]
        public long? UpdateTime { get; set; }

        /// <summary>
        /// Gets or Sets WorkerId
        /// </summary>
        [DataMember(Name = "workerId", EmitDefaultValue = false)]
        public string WorkerId { get; set; }

        /// <summary>
        /// Gets or Sets WorkflowInstanceId
        /// </summary>
        [DataMember(Name = "workflowInstanceId", EmitDefaultValue = false)]
        public string WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or Sets WorkflowPriority
        /// </summary>
        [DataMember(Name = "workflowPriority", EmitDefaultValue = false)]
        public int? WorkflowPriority { get; set; }

        /// <summary>
        /// Gets or Sets WorkflowTask
        /// </summary>
        [DataMember(Name = "workflowTask", EmitDefaultValue = false)]
        public WorkflowTask WorkflowTask { get; set; }

        /// <summary>
        /// Gets or Sets WorkflowType
        /// </summary>
        [DataMember(Name = "workflowType", EmitDefaultValue = false)]
        public string WorkflowType { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Task {\n");
            sb.Append("  CallbackAfterSeconds: ").Append(CallbackAfterSeconds).Append("\n");
            sb.Append("  CallbackFromWorker: ").Append(CallbackFromWorker).Append("\n");
            sb.Append("  CorrelationId: ").Append(CorrelationId).Append("\n");
            sb.Append("  Domain: ").Append(Domain).Append("\n");
            sb.Append("  EndTime: ").Append(EndTime).Append("\n");
            sb.Append("  Executed: ").Append(Executed).Append("\n");
            sb.Append("  ExecutionNameSpace: ").Append(ExecutionNameSpace).Append("\n");
            sb.Append("  ExternalInputPayloadStoragePath: ").Append(ExternalInputPayloadStoragePath).Append("\n");
            sb.Append("  ExternalOutputPayloadStoragePath: ").Append(ExternalOutputPayloadStoragePath).Append("\n");
            sb.Append("  InputData: ").Append(InputData).Append("\n");
            sb.Append("  IsolationGroupId: ").Append(IsolationGroupId).Append("\n");
            sb.Append("  Iteration: ").Append(Iteration).Append("\n");
            sb.Append("  LoopOverTask: ").Append(LoopOverTask).Append("\n");
            sb.Append("  OutputData: ").Append(OutputData).Append("\n");
            sb.Append("  RuntimeMetadata: ").Append(RuntimeMetadata).Append("\n");
            sb.Append("  PollCount: ").Append(PollCount).Append("\n");
            sb.Append("  QueueWaitTime: ").Append(QueueWaitTime).Append("\n");
            sb.Append("  RateLimitFrequencyInSeconds: ").Append(RateLimitFrequencyInSeconds).Append("\n");
            sb.Append("  RateLimitPerFrequency: ").Append(RateLimitPerFrequency).Append("\n");
            sb.Append("  ReasonForIncompletion: ").Append(ReasonForIncompletion).Append("\n");
            sb.Append("  ReferenceTaskName: ").Append(ReferenceTaskName).Append("\n");
            sb.Append("  ResponseTimeoutSeconds: ").Append(ResponseTimeoutSeconds).Append("\n");
            sb.Append("  Retried: ").Append(Retried).Append("\n");
            sb.Append("  RetriedTaskId: ").Append(RetriedTaskId).Append("\n");
            sb.Append("  RetryCount: ").Append(RetryCount).Append("\n");
            sb.Append("  ScheduledTime: ").Append(ScheduledTime).Append("\n");
            sb.Append("  Seq: ").Append(Seq).Append("\n");
            sb.Append("  StartDelayInSeconds: ").Append(StartDelayInSeconds).Append("\n");
            sb.Append("  StartTime: ").Append(StartTime).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  SubWorkflowId: ").Append(SubWorkflowId).Append("\n");
            sb.Append("  SubworkflowChanged: ").Append(SubworkflowChanged).Append("\n");
            sb.Append("  TaskDefName: ").Append(TaskDefName).Append("\n");
            sb.Append("  TaskDefinition: ").Append(TaskDefinition).Append("\n");
            sb.Append("  TaskId: ").Append(TaskId).Append("\n");
            sb.Append("  TaskType: ").Append(TaskType).Append("\n");
            sb.Append("  UpdateTime: ").Append(UpdateTime).Append("\n");
            sb.Append("  WorkerId: ").Append(WorkerId).Append("\n");
            sb.Append("  WorkflowInstanceId: ").Append(WorkflowInstanceId).Append("\n");
            sb.Append("  WorkflowPriority: ").Append(WorkflowPriority).Append("\n");
            sb.Append("  WorkflowTask: ").Append(WorkflowTask).Append("\n");
            sb.Append("  WorkflowType: ").Append(WorkflowType).Append("\n");
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
            return this.Equals(input as Task);
        }

        /// <summary>
        /// Returns true if Task instances are equal
        /// </summary>
        /// <param name="input">Instance of Task to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Task input)
        {
            if (input == null)
                return false;

            return
                (
                    this.CallbackAfterSeconds == input.CallbackAfterSeconds ||
                    (this.CallbackAfterSeconds != null &&
                    this.CallbackAfterSeconds.Equals(input.CallbackAfterSeconds))
                ) &&
                (
                    this.CallbackFromWorker == input.CallbackFromWorker ||
                    (this.CallbackFromWorker != null &&
                    this.CallbackFromWorker.Equals(input.CallbackFromWorker))
                ) &&
                (
                    this.CorrelationId == input.CorrelationId ||
                    (this.CorrelationId != null &&
                    this.CorrelationId.Equals(input.CorrelationId))
                ) &&
                (
                    this.Domain == input.Domain ||
                    (this.Domain != null &&
                    this.Domain.Equals(input.Domain))
                ) &&
                (
                    this.EndTime == input.EndTime ||
                    (this.EndTime != null &&
                    this.EndTime.Equals(input.EndTime))
                ) &&
                (
                    this.Executed == input.Executed ||
                    (this.Executed != null &&
                    this.Executed.Equals(input.Executed))
                ) &&
                (
                    this.ExecutionNameSpace == input.ExecutionNameSpace ||
                    (this.ExecutionNameSpace != null &&
                    this.ExecutionNameSpace.Equals(input.ExecutionNameSpace))
                ) &&
                (
                    this.ExternalInputPayloadStoragePath == input.ExternalInputPayloadStoragePath ||
                    (this.ExternalInputPayloadStoragePath != null &&
                    this.ExternalInputPayloadStoragePath.Equals(input.ExternalInputPayloadStoragePath))
                ) &&
                (
                    this.ExternalOutputPayloadStoragePath == input.ExternalOutputPayloadStoragePath ||
                    (this.ExternalOutputPayloadStoragePath != null &&
                    this.ExternalOutputPayloadStoragePath.Equals(input.ExternalOutputPayloadStoragePath))
                ) &&
                (
                    this.InputData == input.InputData ||
                    this.InputData != null &&
                    input.InputData != null &&
                    this.InputData.SequenceEqual(input.InputData)
                ) &&
                (
                    this.IsolationGroupId == input.IsolationGroupId ||
                    (this.IsolationGroupId != null &&
                    this.IsolationGroupId.Equals(input.IsolationGroupId))
                ) &&
                (
                    this.Iteration == input.Iteration ||
                    (this.Iteration != null &&
                    this.Iteration.Equals(input.Iteration))
                ) &&
                (
                    this.LoopOverTask == input.LoopOverTask ||
                    (this.LoopOverTask != null &&
                    this.LoopOverTask.Equals(input.LoopOverTask))
                ) &&
                (
                    this.OutputData == input.OutputData ||
                    this.OutputData != null &&
                    input.OutputData != null &&
                    this.OutputData.SequenceEqual(input.OutputData)
                ) &&
                (
                    this.RuntimeMetadata == input.RuntimeMetadata ||
                    this.RuntimeMetadata != null &&
                    input.RuntimeMetadata != null &&
                    this.RuntimeMetadata.SequenceEqual(input.RuntimeMetadata)
                ) &&
                (
                    this.PollCount == input.PollCount ||
                    (this.PollCount != null &&
                    this.PollCount.Equals(input.PollCount))
                ) &&
                (
                    this.QueueWaitTime == input.QueueWaitTime ||
                    (this.QueueWaitTime != null &&
                    this.QueueWaitTime.Equals(input.QueueWaitTime))
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
                    this.ReasonForIncompletion == input.ReasonForIncompletion ||
                    (this.ReasonForIncompletion != null &&
                    this.ReasonForIncompletion.Equals(input.ReasonForIncompletion))
                ) &&
                (
                    this.ReferenceTaskName == input.ReferenceTaskName ||
                    (this.ReferenceTaskName != null &&
                    this.ReferenceTaskName.Equals(input.ReferenceTaskName))
                ) &&
                (
                    this.ResponseTimeoutSeconds == input.ResponseTimeoutSeconds ||
                    (this.ResponseTimeoutSeconds != null &&
                    this.ResponseTimeoutSeconds.Equals(input.ResponseTimeoutSeconds))
                ) &&
                (
                    this.Retried == input.Retried ||
                    (this.Retried != null &&
                    this.Retried.Equals(input.Retried))
                ) &&
                (
                    this.RetriedTaskId == input.RetriedTaskId ||
                    (this.RetriedTaskId != null &&
                    this.RetriedTaskId.Equals(input.RetriedTaskId))
                ) &&
                (
                    this.RetryCount == input.RetryCount ||
                    (this.RetryCount != null &&
                    this.RetryCount.Equals(input.RetryCount))
                ) &&
                (
                    this.ScheduledTime == input.ScheduledTime ||
                    (this.ScheduledTime != null &&
                    this.ScheduledTime.Equals(input.ScheduledTime))
                ) &&
                (
                    this.Seq == input.Seq ||
                    (this.Seq != null &&
                    this.Seq.Equals(input.Seq))
                ) &&
                (
                    this.StartDelayInSeconds == input.StartDelayInSeconds ||
                    (this.StartDelayInSeconds != null &&
                    this.StartDelayInSeconds.Equals(input.StartDelayInSeconds))
                ) &&
                (
                    this.StartTime == input.StartTime ||
                    (this.StartTime != null &&
                    this.StartTime.Equals(input.StartTime))
                ) &&
                (
                    this.Status == input.Status ||
                    (this.Status != null &&
                    this.Status.Equals(input.Status))
                ) &&
                (
                    this.SubWorkflowId == input.SubWorkflowId ||
                    (this.SubWorkflowId != null &&
                    this.SubWorkflowId.Equals(input.SubWorkflowId))
                ) &&
                (
                    this.SubworkflowChanged == input.SubworkflowChanged ||
                    (this.SubworkflowChanged != null &&
                    this.SubworkflowChanged.Equals(input.SubworkflowChanged))
                ) &&
                (
                    this.TaskDefName == input.TaskDefName ||
                    (this.TaskDefName != null &&
                    this.TaskDefName.Equals(input.TaskDefName))
                ) &&
                (
                    this.TaskDefinition == input.TaskDefinition ||
                    (this.TaskDefinition != null &&
                    this.TaskDefinition.Equals(input.TaskDefinition))
                ) &&
                (
                    this.TaskId == input.TaskId ||
                    (this.TaskId != null &&
                    this.TaskId.Equals(input.TaskId))
                ) &&
                (
                    this.TaskType == input.TaskType ||
                    (this.TaskType != null &&
                    this.TaskType.Equals(input.TaskType))
                ) &&
                (
                    this.UpdateTime == input.UpdateTime ||
                    (this.UpdateTime != null &&
                    this.UpdateTime.Equals(input.UpdateTime))
                ) &&
                (
                    this.WorkerId == input.WorkerId ||
                    (this.WorkerId != null &&
                    this.WorkerId.Equals(input.WorkerId))
                ) &&
                (
                    this.WorkflowInstanceId == input.WorkflowInstanceId ||
                    (this.WorkflowInstanceId != null &&
                    this.WorkflowInstanceId.Equals(input.WorkflowInstanceId))
                ) &&
                (
                    this.WorkflowPriority == input.WorkflowPriority ||
                    (this.WorkflowPriority != null &&
                    this.WorkflowPriority.Equals(input.WorkflowPriority))
                ) &&
                (
                    this.WorkflowTask == input.WorkflowTask ||
                    (this.WorkflowTask != null &&
                    this.WorkflowTask.Equals(input.WorkflowTask))
                ) &&
                (
                    this.WorkflowType == input.WorkflowType ||
                    (this.WorkflowType != null &&
                    this.WorkflowType.Equals(input.WorkflowType))
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
                if (this.CallbackAfterSeconds != null)
                    hashCode = hashCode * 59 + this.CallbackAfterSeconds.GetHashCode();
                if (this.CallbackFromWorker != null)
                    hashCode = hashCode * 59 + this.CallbackFromWorker.GetHashCode();
                if (this.CorrelationId != null)
                    hashCode = hashCode * 59 + this.CorrelationId.GetHashCode();
                if (this.Domain != null)
                    hashCode = hashCode * 59 + this.Domain.GetHashCode();
                if (this.EndTime != null)
                    hashCode = hashCode * 59 + this.EndTime.GetHashCode();
                if (this.Executed != null)
                    hashCode = hashCode * 59 + this.Executed.GetHashCode();
                if (this.ExecutionNameSpace != null)
                    hashCode = hashCode * 59 + this.ExecutionNameSpace.GetHashCode();
                if (this.ExternalInputPayloadStoragePath != null)
                    hashCode = hashCode * 59 + this.ExternalInputPayloadStoragePath.GetHashCode();
                if (this.ExternalOutputPayloadStoragePath != null)
                    hashCode = hashCode * 59 + this.ExternalOutputPayloadStoragePath.GetHashCode();
                if (this.InputData != null)
                    hashCode = hashCode * 59 + this.InputData.GetHashCode();
                if (this.IsolationGroupId != null)
                    hashCode = hashCode * 59 + this.IsolationGroupId.GetHashCode();
                if (this.Iteration != null)
                    hashCode = hashCode * 59 + this.Iteration.GetHashCode();
                if (this.LoopOverTask != null)
                    hashCode = hashCode * 59 + this.LoopOverTask.GetHashCode();
                if (this.OutputData != null)
                    hashCode = hashCode * 59 + this.OutputData.GetHashCode();
                if (this.RuntimeMetadata != null)
                    hashCode = hashCode * 59 + this.RuntimeMetadata.GetHashCode();
                if (this.PollCount != null)
                    hashCode = hashCode * 59 + this.PollCount.GetHashCode();
                if (this.QueueWaitTime != null)
                    hashCode = hashCode * 59 + this.QueueWaitTime.GetHashCode();
                if (this.RateLimitFrequencyInSeconds != null)
                    hashCode = hashCode * 59 + this.RateLimitFrequencyInSeconds.GetHashCode();
                if (this.RateLimitPerFrequency != null)
                    hashCode = hashCode * 59 + this.RateLimitPerFrequency.GetHashCode();
                if (this.ReasonForIncompletion != null)
                    hashCode = hashCode * 59 + this.ReasonForIncompletion.GetHashCode();
                if (this.ReferenceTaskName != null)
                    hashCode = hashCode * 59 + this.ReferenceTaskName.GetHashCode();
                if (this.ResponseTimeoutSeconds != null)
                    hashCode = hashCode * 59 + this.ResponseTimeoutSeconds.GetHashCode();
                if (this.Retried != null)
                    hashCode = hashCode * 59 + this.Retried.GetHashCode();
                if (this.RetriedTaskId != null)
                    hashCode = hashCode * 59 + this.RetriedTaskId.GetHashCode();
                if (this.RetryCount != null)
                    hashCode = hashCode * 59 + this.RetryCount.GetHashCode();
                if (this.ScheduledTime != null)
                    hashCode = hashCode * 59 + this.ScheduledTime.GetHashCode();
                if (this.Seq != null)
                    hashCode = hashCode * 59 + this.Seq.GetHashCode();
                if (this.StartDelayInSeconds != null)
                    hashCode = hashCode * 59 + this.StartDelayInSeconds.GetHashCode();
                if (this.StartTime != null)
                    hashCode = hashCode * 59 + this.StartTime.GetHashCode();
                if (this.Status != null)
                    hashCode = hashCode * 59 + this.Status.GetHashCode();
                if (this.SubWorkflowId != null)
                    hashCode = hashCode * 59 + this.SubWorkflowId.GetHashCode();
                if (this.SubworkflowChanged != null)
                    hashCode = hashCode * 59 + this.SubworkflowChanged.GetHashCode();
                if (this.TaskDefName != null)
                    hashCode = hashCode * 59 + this.TaskDefName.GetHashCode();
                if (this.TaskDefinition != null)
                    hashCode = hashCode * 59 + this.TaskDefinition.GetHashCode();
                if (this.TaskId != null)
                    hashCode = hashCode * 59 + this.TaskId.GetHashCode();
                if (this.TaskType != null)
                    hashCode = hashCode * 59 + this.TaskType.GetHashCode();
                if (this.UpdateTime != null)
                    hashCode = hashCode * 59 + this.UpdateTime.GetHashCode();
                if (this.WorkerId != null)
                    hashCode = hashCode * 59 + this.WorkerId.GetHashCode();
                if (this.WorkflowInstanceId != null)
                    hashCode = hashCode * 59 + this.WorkflowInstanceId.GetHashCode();
                if (this.WorkflowPriority != null)
                    hashCode = hashCode * 59 + this.WorkflowPriority.GetHashCode();
                if (this.WorkflowTask != null)
                    hashCode = hashCode * 59 + this.WorkflowTask.GetHashCode();
                if (this.WorkflowType != null)
                    hashCode = hashCode * 59 + this.WorkflowType.GetHashCode();
                return hashCode;
            }
        }

        // Method to convert Task to TaskResult
        public TaskResult ToTaskResult(TaskResult.StatusEnum status = TaskResult.StatusEnum.COMPLETED)
        {
            return new TaskResult
            {
                TaskId = this.TaskId,
                WorkflowInstanceId = this.WorkflowInstanceId,
                WorkerId = this.WorkerId,
                Status = status
            };
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
