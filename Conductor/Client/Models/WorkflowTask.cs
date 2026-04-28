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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Conductor.Client.Models
{
    /// <summary>
    /// WorkflowTask
    /// </summary>
    [DataContract]
    public partial class WorkflowTask : IEquatable<WorkflowTask>, IValidatableObject
    {
        /// <summary>
        /// Defines WorkflowTaskType
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum WorkflowTaskTypeEnum
        {
            /// <summary>
            /// Enum SIMPLE for value: SIMPLE
            /// </summary>
            [EnumMember(Value = "SIMPLE")]
            SIMPLE = 1,
            /// <summary>
            /// Enum DYNAMIC for value: DYNAMIC
            /// </summary>
            [EnumMember(Value = "DYNAMIC")]
            DYNAMIC = 2,
            /// <summary>
            /// Enum FORKJOIN for value: FORK_JOIN
            /// </summary>
            [EnumMember(Value = "FORK_JOIN")]
            FORKJOIN = 3,
            /// <summary>
            /// Enum FORKJOINDYNAMIC for value: FORK_JOIN_DYNAMIC
            /// </summary>
            [EnumMember(Value = "FORK_JOIN_DYNAMIC")]
            FORKJOINDYNAMIC = 4,
            /// <summary>
            /// Enum DECISION for value: DECISION
            /// </summary>
            [EnumMember(Value = "DECISION")]
            DECISION = 5,
            /// <summary>
            /// Enum SWITCH for value: SWITCH
            /// </summary>
            [EnumMember(Value = "SWITCH")]
            SWITCH = 6,
            /// <summary>
            /// Enum JOIN for value: JOIN
            /// </summary>
            [EnumMember(Value = "JOIN")]
            JOIN = 7,
            /// <summary>
            /// Enum DOWHILE for value: DO_WHILE
            /// </summary>
            [EnumMember(Value = "DO_WHILE")]
            DOWHILE = 8,
            /// <summary>
            /// Enum SUBWORKFLOW for value: SUB_WORKFLOW
            /// </summary>
            [EnumMember(Value = "SUB_WORKFLOW")]
            SUBWORKFLOW = 9,
            /// <summary>
            /// Enum STARTWORKFLOW for value: START_WORKFLOW
            /// </summary>
            [EnumMember(Value = "START_WORKFLOW")]
            STARTWORKFLOW = 10,
            /// <summary>
            /// Enum EVENT for value: EVENT
            /// </summary>
            [EnumMember(Value = "EVENT")]
            EVENT = 11,
            /// <summary>
            /// Enum WAIT for value: WAIT
            /// </summary>
            [EnumMember(Value = "WAIT")]
            WAIT = 12,
            /// <summary>
            /// Enum HUMAN for value: HUMAN
            /// </summary>
            [EnumMember(Value = "HUMAN")]
            HUMAN = 13,
            /// <summary>
            /// Enum USERDEFINED for value: USER_DEFINED
            /// </summary>
            [EnumMember(Value = "USER_DEFINED")]
            USERDEFINED = 14,
            /// <summary>
            /// Enum HTTP for value: HTTP
            /// </summary>
            [EnumMember(Value = "HTTP")]
            HTTP = 15,
            /// <summary>
            /// Enum LAMBDA for value: LAMBDA
            /// </summary>
            [EnumMember(Value = "LAMBDA")]
            LAMBDA = 16,
            /// <summary>
            /// Enum INLINE for value: INLINE
            /// </summary>
            [EnumMember(Value = "INLINE")]
            INLINE = 17,
            /// <summary>
            /// Enum EXCLUSIVEJOIN for value: EXCLUSIVE_JOIN
            /// </summary>
            [EnumMember(Value = "EXCLUSIVE_JOIN")]
            EXCLUSIVEJOIN = 18,
            /// <summary>
            /// Enum TERMINATE for value: TERMINATE
            /// </summary>
            [EnumMember(Value = "TERMINATE")]
            TERMINATE = 19,
            /// <summary>
            /// Enum KAFKAPUBLISH for value: KAFKA_PUBLISH
            /// </summary>
            [EnumMember(Value = "KAFKA_PUBLISH")]
            KAFKAPUBLISH = 20,
            /// <summary>
            /// Enum JSONJQTRANSFORM for value: JSON_JQ_TRANSFORM
            /// </summary>
            [EnumMember(Value = "JSON_JQ_TRANSFORM")]
            JSONJQTRANSFORM = 21,
            /// <summary>
            /// Enum SETVARIABLE for value: SET_VARIABLE
            /// </summary>
            [EnumMember(Value = "SET_VARIABLE")]
            SETVARIABLE = 22,
            /// <summary>
            /// Enum LLMCHATCOMPLETE for value: LLM_CHAT_COMPLETE
            /// </summary>
            [EnumMember(Value = "LLM_CHAT_COMPLETE")]
            LLMCHATCOMPLETE = 23,
            /// <summary>
            /// Enum LLMGENERATEEMBEDDINGS for value: LLM_GENERATE_EMBEDDINGS
            /// </summary>
            [EnumMember(Value = "LLM_GENERATE_EMBEDDINGS")]
            LLMGENERATEEMBEDDINGS = 24,
            /// <summary>
            /// Enum LLMINDEXDOCUMENT for value: LLM_INDEX_DOCUMENT
            /// </summary>
            [EnumMember(Value = "LLM_INDEX_DOCUMENT")]
            LLMINDEXDOCUMENT = 25,
            /// <summary>
            /// Enum LLMINDEXTEXT for value: LLM_INDEX_TEXT
            /// </summary>
            [EnumMember(Value = "LLM_INDEX_TEXT")]
            LLMINDEXTEXT = 26,
            /// <summary>
            /// Enum LLMGETEMBEDDINGS for value: LLM_GET_EMBEDDINGS
            /// </summary>
            [EnumMember(Value = "LLM_GET_EMBEDDINGS")]
            LLMGETEMBEDDINGS = 27,
            /// <summary>
            /// Enum LLMSEARCHINDEX for value: LLM_SEARCH_INDEX
            /// </summary>
            [EnumMember(Value = "LLM_SEARCH_INDEX")]
            LLMSEARCHINDEX = 28,
            /// <summary>
            /// Enum LLMINDEXTEXT for value: LLM_INDEX_TEXT
            /// </summary>
            [EnumMember(Value = "LLM_TEXT_COMPLETE")]
            LLMTEXTCOMPLETE = 29,
            /// <summary>
            /// Enum WAITFORWEBHOOK for value: WAIT_FOR_WEBHOOK
            /// </summary>
            [EnumMember(Value = "WAIT_FOR_WEBHOOK")]
            WAITFORWEBHOOK = 30,
            /// <summary>
            /// Enum HTTPPOLL for value: HTTP_POLL
            /// </summary>
            [EnumMember(Value = "HTTP_POLL")]
            HTTPPOLL = 31,
            /// <summary>
            /// Enum LLMSTOREEMBEDDINGS for value: LLM_STORE_EMBEDDINGS
            /// </summary>
            [EnumMember(Value = "LLM_STORE_EMBEDDINGS")]
            LLMSTOREEMBEDDINGS = 32,
            /// <summary>
            /// Enum LLMSEARCHEMBEDDINGS for value: LLM_SEARCH_EMBEDDINGS
            /// </summary>
            [EnumMember(Value = "LLM_SEARCH_EMBEDDINGS")]
            LLMSEARCHEMBEDDINGS = 33,
            /// <summary>
            /// Enum GETDOCUMENT for value: GET_DOCUMENT
            /// </summary>
            [EnumMember(Value = "GET_DOCUMENT")]
            GETDOCUMENT = 34,
            /// <summary>
            /// Enum GENERATEIMAGE for value: GENERATE_IMAGE
            /// </summary>
            [EnumMember(Value = "GENERATE_IMAGE")]
            GENERATEIMAGE = 35,
            /// <summary>
            /// Enum GENERATEAUDIO for value: GENERATE_AUDIO
            /// </summary>
            [EnumMember(Value = "GENERATE_AUDIO")]
            GENERATEAUDIO = 36,
            /// <summary>
            /// Enum LISTMCPTOOLS for value: LIST_MCP_TOOLS
            /// </summary>
            [EnumMember(Value = "LIST_MCP_TOOLS")]
            LISTMCPTOOLS = 37,
            /// <summary>
            /// Enum CALLMCPTOOL for value: CALL_MCP_TOOL
            /// </summary>
            [EnumMember(Value = "CALL_MCP_TOOL")]
            CALLMCPTOOL = 38,
            /// <summary>
            /// Enum TOOLCALL for value: TOOL_CALL
            /// </summary>
            [EnumMember(Value = "TOOL_CALL")]
            TOOLCALL = 39,
            /// <summary>
            /// Enum TOOLSPEC for value: TOOL_SPEC
            /// </summary>
            [EnumMember(Value = "TOOL_SPEC")]
            TOOLSPEC = 40
        }
        /// <summary>
        /// Gets or Sets WorkflowTaskType
        /// </summary>
        [DataMember(Name = "workflowTaskType", EmitDefaultValue = false)]
        public WorkflowTaskTypeEnum? WorkflowTaskType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTask" /> class.
        /// </summary>
        /// <param name="asyncComplete">asyncComplete.</param>
        /// <param name="caseExpression">caseExpression.</param>
        /// <param name="caseValueParam">caseValueParam.</param>
        /// <param name="decisionCases">decisionCases.</param>
        /// <param name="defaultCase">defaultCase.</param>
        /// <param name="defaultExclusiveJoinTask">defaultExclusiveJoinTask.</param>
        /// <param name="description">description.</param>
        /// <param name="dynamicForkJoinTasksParam">dynamicForkJoinTasksParam.</param>
        /// <param name="dynamicForkTasksInputParamName">dynamicForkTasksInputParamName.</param>
        /// <param name="dynamicForkTasksParam">dynamicForkTasksParam.</param>
        /// <param name="dynamicTaskNameParam">dynamicTaskNameParam.</param>
        /// <param name="evaluatorType">evaluatorType.</param>
        /// <param name="expression">expression.</param>
        /// <param name="forkTasks">forkTasks.</param>
        /// <param name="inputParameters">inputParameters.</param>
        /// <param name="joinOn">joinOn.</param>
        /// <param name="loopCondition">loopCondition.</param>
        /// <param name="loopOver">loopOver.</param>
        /// <param name="name">name (required).</param>
        /// <param name="optional">optional.</param>
        /// <param name="rateLimited">rateLimited.</param>
        /// <param name="retryCount">retryCount.</param>
        /// <param name="scriptExpression">scriptExpression.</param>
        /// <param name="sink">sink.</param>
        /// <param name="startDelay">startDelay.</param>
        /// <param name="subWorkflowParam">subWorkflowParam.</param>
        /// <param name="taskDefinition">taskDefinition.</param>
        /// <param name="taskReferenceName">taskReferenceName (required).</param>
        /// <param name="type">type.</param>
        /// <param name="workflowTaskType">workflowTaskType.</param>
        public WorkflowTask(bool? asyncComplete = default(bool?), string caseExpression = default(string), string caseValueParam = default(string), Dictionary<string, List<WorkflowTask>> decisionCases = default(Dictionary<string, List<WorkflowTask>>), List<WorkflowTask> defaultCase = default(List<WorkflowTask>), List<string> defaultExclusiveJoinTask = default(List<string>), string description = default(string), string dynamicForkJoinTasksParam = default(string), string dynamicForkTasksInputParamName = default(string), string dynamicForkTasksParam = default(string), string dynamicTaskNameParam = default(string), string evaluatorType = default(string), string expression = default(string), List<List<WorkflowTask>> forkTasks = default(List<List<WorkflowTask>>), Dictionary<string, Object> inputParameters = default(Dictionary<string, Object>), List<string> joinOn = default(List<string>), string loopCondition = default(string), List<WorkflowTask> loopOver = default(List<WorkflowTask>), string name = default(string), bool? optional = default(bool?), bool? rateLimited = default(bool?), int? retryCount = default(int?), string scriptExpression = default(string), string sink = default(string), int? startDelay = default(int?), SubWorkflowParams subWorkflowParam = default(SubWorkflowParams), TaskDef taskDefinition = default(TaskDef), string taskReferenceName = default(string), string type = default(string), WorkflowTaskTypeEnum? workflowTaskType = default(WorkflowTaskTypeEnum?), Dictionary<string, StateChangeConfig> onStateChange = default(Dictionary<string, StateChangeConfig>))
        {
            this.TaskReferenceName = taskReferenceName;
            this.Name = name;
            this.AsyncComplete = asyncComplete;
            this.CaseExpression = caseExpression;
            this.CaseValueParam = caseValueParam;
            this.DecisionCases = decisionCases;
            this.DefaultCase = defaultCase;
            this.DefaultExclusiveJoinTask = defaultExclusiveJoinTask;
            this.Description = description;
            this.DynamicForkJoinTasksParam = dynamicForkJoinTasksParam;
            this.DynamicForkTasksInputParamName = dynamicForkTasksInputParamName;
            this.DynamicForkTasksParam = dynamicForkTasksParam;
            this.DynamicTaskNameParam = dynamicTaskNameParam;
            this.EvaluatorType = evaluatorType;
            this.Expression = expression;
            this.ForkTasks = forkTasks;
            this.InputParameters = inputParameters;
            this.JoinOn = joinOn;
            this.LoopCondition = loopCondition;
            this.LoopOver = loopOver;
            this.Optional = optional;
            this.RateLimited = rateLimited;
            this.RetryCount = retryCount;
            this.ScriptExpression = scriptExpression;
            this.Sink = sink;
            this.StartDelay = startDelay;
            this.SubWorkflowParam = subWorkflowParam;
            this.TaskDefinition = taskDefinition;
            this.Type = type;
            this.WorkflowTaskType = workflowTaskType;
            this.OnStateChange = onStateChange;
        }

        /// <summary>
        /// Gets or Sets AsyncComplete
        /// </summary>
        [DataMember(Name = "asyncComplete", EmitDefaultValue = false)]
        public bool? AsyncComplete { get; set; }

        /// <summary>
        /// Gets or Sets CaseExpression
        /// </summary>
        [DataMember(Name = "caseExpression", EmitDefaultValue = false)]
        public string CaseExpression { get; set; }

        /// <summary>
        /// Gets or Sets CaseValueParam
        /// </summary>
        [DataMember(Name = "caseValueParam", EmitDefaultValue = false)]
        public string CaseValueParam { get; set; }

        /// <summary>
        /// Gets or Sets DecisionCases
        /// </summary>
        [DataMember(Name = "decisionCases", EmitDefaultValue = false)]
        public Dictionary<string, List<WorkflowTask>> DecisionCases { get; set; }

        /// <summary>
        /// Gets or Sets DefaultCase
        /// </summary>
        [DataMember(Name = "defaultCase", EmitDefaultValue = false)]
        public List<WorkflowTask> DefaultCase { get; set; }

        /// <summary>
        /// Gets or Sets DefaultExclusiveJoinTask
        /// </summary>
        [DataMember(Name = "defaultExclusiveJoinTask", EmitDefaultValue = false)]
        public List<string> DefaultExclusiveJoinTask { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets DynamicForkJoinTasksParam
        /// </summary>
        [DataMember(Name = "dynamicForkJoinTasksParam", EmitDefaultValue = false)]
        public string DynamicForkJoinTasksParam { get; set; }

        /// <summary>
        /// Gets or Sets DynamicForkTasksInputParamName
        /// </summary>
        [DataMember(Name = "dynamicForkTasksInputParamName", EmitDefaultValue = false)]
        public string DynamicForkTasksInputParamName { get; set; }

        /// <summary>
        /// Gets or Sets DynamicForkTasksParam
        /// </summary>
        [DataMember(Name = "dynamicForkTasksParam", EmitDefaultValue = false)]
        public string DynamicForkTasksParam { get; set; }

        /// <summary>
        /// Gets or Sets DynamicTaskNameParam
        /// </summary>
        [DataMember(Name = "dynamicTaskNameParam", EmitDefaultValue = false)]
        public string DynamicTaskNameParam { get; set; }

        /// <summary>
        /// Gets or Sets EvaluatorType
        /// </summary>
        [DataMember(Name = "evaluatorType", EmitDefaultValue = false)]
        public string EvaluatorType { get; set; }

        /// <summary>
        /// Gets or Sets Expression
        /// </summary>
        [DataMember(Name = "expression", EmitDefaultValue = false)]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or Sets ForkTasks
        /// </summary>
        [DataMember(Name = "forkTasks", EmitDefaultValue = false)]
        public List<List<WorkflowTask>> ForkTasks { get; set; }

        /// <summary>
        /// Gets or Sets InputParameters
        /// </summary>
        [DataMember(Name = "inputParameters", EmitDefaultValue = false)]
        public Dictionary<string, Object> InputParameters { get; set; }

        /// <summary>
        /// Gets or Sets JoinOn
        /// </summary>
        [DataMember(Name = "joinOn", EmitDefaultValue = false)]
        public List<string> JoinOn { get; set; }

        /// <summary>
        /// Gets or Sets LoopCondition
        /// </summary>
        [DataMember(Name = "loopCondition", EmitDefaultValue = false)]
        public string LoopCondition { get; set; }

        /// <summary>
        /// Gets or Sets LoopOver
        /// </summary>
        [DataMember(Name = "loopOver", EmitDefaultValue = false)]
        public List<WorkflowTask> LoopOver { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Optional
        /// </summary>
        [DataMember(Name = "optional", EmitDefaultValue = false)]
        public bool? Optional { get; set; }

        /// <summary>
        /// Gets or Sets RateLimited
        /// </summary>
        [DataMember(Name = "rateLimited", EmitDefaultValue = false)]
        public bool? RateLimited { get; set; }

        /// <summary>
        /// Gets or Sets RetryCount
        /// </summary>
        [DataMember(Name = "retryCount", EmitDefaultValue = false)]
        public int? RetryCount { get; set; }

        /// <summary>
        /// Gets or Sets ScriptExpression
        /// </summary>
        [DataMember(Name = "scriptExpression", EmitDefaultValue = false)]
        public string ScriptExpression { get; set; }

        /// <summary>
        /// Gets or Sets Sink
        /// </summary>
        [DataMember(Name = "sink", EmitDefaultValue = false)]
        public string Sink { get; set; }

        /// <summary>
        /// Gets or Sets StartDelay
        /// </summary>
        [DataMember(Name = "startDelay", EmitDefaultValue = false)]
        public int? StartDelay { get; set; }

        /// <summary>
        /// Gets or Sets SubWorkflowParam
        /// </summary>
        [DataMember(Name = "subWorkflowParam", EmitDefaultValue = false)]
        public SubWorkflowParams SubWorkflowParam { get; set; }

        /// <summary>
        /// Gets or Sets TaskDefinition
        /// </summary>
        [DataMember(Name = "taskDefinition", EmitDefaultValue = false)]
        public TaskDef TaskDefinition { get; set; }

        /// <summary>
        /// Gets or Sets TaskReferenceName
        /// </summary>
        [DataMember(Name = "taskReferenceName", EmitDefaultValue = false)]
        public string TaskReferenceName { get; set; }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets OnStateChange
        /// </summary>
        [DataMember(Name = "onStateChange", EmitDefaultValue = false)]
        public Dictionary<string, StateChangeConfig> OnStateChange { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class WorkflowTask {\n");
            sb.Append(" AsyncComplete: ").Append(AsyncComplete).Append("\n");
            sb.Append(" CaseExpression: ").Append(CaseExpression).Append("\n");
            sb.Append(" CaseValueParam: ").Append(CaseValueParam).Append("\n");
            sb.Append(" DecisionCases: ").Append(DecisionCases).Append("\n");
            sb.Append(" DefaultCase: ").Append(DefaultCase).Append("\n");
            sb.Append(" DefaultExclusiveJoinTask: ").Append(DefaultExclusiveJoinTask).Append("\n");
            sb.Append(" Description: ").Append(Description).Append("\n");
            sb.Append(" DynamicForkJoinTasksParam: ").Append(DynamicForkJoinTasksParam).Append("\n");
            sb.Append(" DynamicForkTasksInputParamName: ").Append(DynamicForkTasksInputParamName).Append("\n");
            sb.Append(" DynamicForkTasksParam: ").Append(DynamicForkTasksParam).Append("\n");
            sb.Append(" DynamicTaskNameParam: ").Append(DynamicTaskNameParam).Append("\n");
            sb.Append(" EvaluatorType: ").Append(EvaluatorType).Append("\n");
            sb.Append(" Expression: ").Append(Expression).Append("\n");
            sb.Append(" ForkTasks: ").Append(ForkTasks).Append("\n");
            sb.Append(" InputParameters: ").Append(InputParameters).Append("\n");
            sb.Append(" JoinOn: ").Append(JoinOn).Append("\n");
            sb.Append(" LoopCondition: ").Append(LoopCondition).Append("\n");
            sb.Append(" LoopOver: ").Append(LoopOver).Append("\n");
            sb.Append(" Name: ").Append(Name).Append("\n");
            sb.Append(" Optional: ").Append(Optional).Append("\n");
            sb.Append(" RateLimited: ").Append(RateLimited).Append("\n");
            sb.Append(" RetryCount: ").Append(RetryCount).Append("\n");
            sb.Append(" ScriptExpression: ").Append(ScriptExpression).Append("\n");
            sb.Append(" Sink: ").Append(Sink).Append("\n");
            sb.Append(" StartDelay: ").Append(StartDelay).Append("\n");
            sb.Append(" SubWorkflowParam: ").Append(SubWorkflowParam).Append("\n");
            sb.Append(" TaskDefinition: ").Append(TaskDefinition).Append("\n");
            sb.Append(" TaskReferenceName: ").Append(TaskReferenceName).Append("\n");
            sb.Append(" Type: ").Append(Type).Append("\n");
            sb.Append(" WorkflowTaskType: ").Append(WorkflowTaskType).Append("\n");
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
            return this.Equals(input as WorkflowTask);
        }

        /// <summary>
        /// Returns true if WorkflowTask instances are equal
        /// </summary>
        /// <param name="input">Instance of WorkflowTask to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(WorkflowTask input)
        {
            if (input == null)
                return false;

            return
            (
            this.AsyncComplete == input.AsyncComplete ||
            (this.AsyncComplete != null &&
            this.AsyncComplete.Equals(input.AsyncComplete))
            ) &&
            (
            this.CaseExpression == input.CaseExpression ||
            (this.CaseExpression != null &&
            this.CaseExpression.Equals(input.CaseExpression))
            ) &&
            (
            this.CaseValueParam == input.CaseValueParam ||
            (this.CaseValueParam != null &&
            this.CaseValueParam.Equals(input.CaseValueParam))
            ) &&
            (
            this.DecisionCases == input.DecisionCases ||
            this.DecisionCases != null &&
            input.DecisionCases != null &&
            this.DecisionCases.SequenceEqual(input.DecisionCases)
            ) &&
            (
            this.DefaultCase == input.DefaultCase ||
            this.DefaultCase != null &&
            input.DefaultCase != null &&
            this.DefaultCase.SequenceEqual(input.DefaultCase)
            ) &&
            (
            this.DefaultExclusiveJoinTask == input.DefaultExclusiveJoinTask ||
            this.DefaultExclusiveJoinTask != null &&
            input.DefaultExclusiveJoinTask != null &&
            this.DefaultExclusiveJoinTask.SequenceEqual(input.DefaultExclusiveJoinTask)
            ) &&
            (
            this.Description == input.Description ||
            (this.Description != null &&
            this.Description.Equals(input.Description))
            ) &&
            (
            this.DynamicForkJoinTasksParam == input.DynamicForkJoinTasksParam ||
            (this.DynamicForkJoinTasksParam != null &&
            this.DynamicForkJoinTasksParam.Equals(input.DynamicForkJoinTasksParam))
            ) &&
            (
            this.DynamicForkTasksInputParamName == input.DynamicForkTasksInputParamName ||
            (this.DynamicForkTasksInputParamName != null &&
            this.DynamicForkTasksInputParamName.Equals(input.DynamicForkTasksInputParamName))
            ) &&
            (
            this.DynamicForkTasksParam == input.DynamicForkTasksParam ||
            (this.DynamicForkTasksParam != null &&
            this.DynamicForkTasksParam.Equals(input.DynamicForkTasksParam))
            ) &&
            (
            this.DynamicTaskNameParam == input.DynamicTaskNameParam ||
            (this.DynamicTaskNameParam != null &&
            this.DynamicTaskNameParam.Equals(input.DynamicTaskNameParam))
            ) &&
            (
            this.EvaluatorType == input.EvaluatorType ||
            (this.EvaluatorType != null &&
            this.EvaluatorType.Equals(input.EvaluatorType))
            ) &&
            (
            this.Expression == input.Expression ||
            (this.Expression != null &&
            this.Expression.Equals(input.Expression))
            ) &&
            (
            this.ForkTasks == input.ForkTasks ||
            this.ForkTasks != null &&
            input.ForkTasks != null &&
            this.ForkTasks.SequenceEqual(input.ForkTasks)
            ) &&
            (
            this.InputParameters == input.InputParameters ||
            this.InputParameters != null &&
            input.InputParameters != null &&
            this.InputParameters.SequenceEqual(input.InputParameters)
            ) &&
            (
            this.JoinOn == input.JoinOn ||
            this.JoinOn != null &&
            input.JoinOn != null &&
            this.JoinOn.SequenceEqual(input.JoinOn)
            ) &&
            (
            this.LoopCondition == input.LoopCondition ||
            (this.LoopCondition != null &&
            this.LoopCondition.Equals(input.LoopCondition))
            ) &&
            (
            this.LoopOver == input.LoopOver ||
            this.LoopOver != null &&
            input.LoopOver != null &&
            this.LoopOver.SequenceEqual(input.LoopOver)
            ) &&
            (
            this.Name == input.Name ||
            (this.Name != null &&
            this.Name.Equals(input.Name))
            ) &&
            (
            this.Optional == input.Optional ||
            (this.Optional != null &&
            this.Optional.Equals(input.Optional))
            ) &&
            (
            this.RateLimited == input.RateLimited ||
            (this.RateLimited != null &&
            this.RateLimited.Equals(input.RateLimited))
            ) &&
            (
            this.RetryCount == input.RetryCount ||
            (this.RetryCount != null &&
            this.RetryCount.Equals(input.RetryCount))
            ) &&
            (
            this.ScriptExpression == input.ScriptExpression ||
            (this.ScriptExpression != null &&
            this.ScriptExpression.Equals(input.ScriptExpression))
            ) &&
            (
            this.Sink == input.Sink ||
            (this.Sink != null &&
            this.Sink.Equals(input.Sink))
            ) &&
            (
            this.StartDelay == input.StartDelay ||
            (this.StartDelay != null &&
            this.StartDelay.Equals(input.StartDelay))
            ) &&
            (
            this.SubWorkflowParam == input.SubWorkflowParam ||
            (this.SubWorkflowParam != null &&
            this.SubWorkflowParam.Equals(input.SubWorkflowParam))
            ) &&
            (
            this.TaskDefinition == input.TaskDefinition ||
            (this.TaskDefinition != null &&
            this.TaskDefinition.Equals(input.TaskDefinition))
            ) &&
            (
            this.TaskReferenceName == input.TaskReferenceName ||
            (this.TaskReferenceName != null &&
            this.TaskReferenceName.Equals(input.TaskReferenceName))
            ) &&
            (
            this.Type == input.Type ||
            (this.Type != null &&
            this.Type.Equals(input.Type))
            ) &&
            (
            this.WorkflowTaskType == input.WorkflowTaskType ||
            (this.WorkflowTaskType != null &&
            this.WorkflowTaskType.Equals(input.WorkflowTaskType))
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
                if (this.AsyncComplete != null)
                    hashCode = hashCode * 59 + this.AsyncComplete.GetHashCode();
                if (this.CaseExpression != null)
                    hashCode = hashCode * 59 + this.CaseExpression.GetHashCode();
                if (this.CaseValueParam != null)
                    hashCode = hashCode * 59 + this.CaseValueParam.GetHashCode();
                if (this.DecisionCases != null)
                    hashCode = hashCode * 59 + this.DecisionCases.GetHashCode();
                if (this.DefaultCase != null)
                    hashCode = hashCode * 59 + this.DefaultCase.GetHashCode();
                if (this.DefaultExclusiveJoinTask != null)
                    hashCode = hashCode * 59 + this.DefaultExclusiveJoinTask.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.DynamicForkJoinTasksParam != null)
                    hashCode = hashCode * 59 + this.DynamicForkJoinTasksParam.GetHashCode();
                if (this.DynamicForkTasksInputParamName != null)
                    hashCode = hashCode * 59 + this.DynamicForkTasksInputParamName.GetHashCode();
                if (this.DynamicForkTasksParam != null)
                    hashCode = hashCode * 59 + this.DynamicForkTasksParam.GetHashCode();
                if (this.DynamicTaskNameParam != null)
                    hashCode = hashCode * 59 + this.DynamicTaskNameParam.GetHashCode();
                if (this.EvaluatorType != null)
                    hashCode = hashCode * 59 + this.EvaluatorType.GetHashCode();
                if (this.Expression != null)
                    hashCode = hashCode * 59 + this.Expression.GetHashCode();
                if (this.ForkTasks != null)
                    hashCode = hashCode * 59 + this.ForkTasks.GetHashCode();
                if (this.InputParameters != null)
                    hashCode = hashCode * 59 + this.InputParameters.GetHashCode();
                if (this.JoinOn != null)
                    hashCode = hashCode * 59 + this.JoinOn.GetHashCode();
                if (this.LoopCondition != null)
                    hashCode = hashCode * 59 + this.LoopCondition.GetHashCode();
                if (this.LoopOver != null)
                    hashCode = hashCode * 59 + this.LoopOver.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Optional != null)
                    hashCode = hashCode * 59 + this.Optional.GetHashCode();
                if (this.RateLimited != null)
                    hashCode = hashCode * 59 + this.RateLimited.GetHashCode();
                if (this.RetryCount != null)
                    hashCode = hashCode * 59 + this.RetryCount.GetHashCode();
                if (this.ScriptExpression != null)
                    hashCode = hashCode * 59 + this.ScriptExpression.GetHashCode();
                if (this.Sink != null)
                    hashCode = hashCode * 59 + this.Sink.GetHashCode();
                if (this.StartDelay != null)
                    hashCode = hashCode * 59 + this.StartDelay.GetHashCode();
                if (this.SubWorkflowParam != null)
                    hashCode = hashCode * 59 + this.SubWorkflowParam.GetHashCode();
                if (this.TaskDefinition != null)
                    hashCode = hashCode * 59 + this.TaskDefinition.GetHashCode();
                if (this.TaskReferenceName != null)
                    hashCode = hashCode * 59 + this.TaskReferenceName.GetHashCode();
                if (this.Type != null)
                    hashCode = hashCode * 59 + this.Type.GetHashCode();
                if (this.WorkflowTaskType != null)
                    hashCode = hashCode * 59 + this.WorkflowTaskType.GetHashCode();
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
        /// <summary>
        /// Sets theDynamicForkJoinTasksParam
        /// </summary>
        /// <param name="dynamicForkTasksParam"></param>
        public void SetDynamicForkJoinTasksParam(string dynamicForkTasksParam)
        {
            this.DynamicForkJoinTasksParam = dynamicForkTasksParam;
        }

        /// <summary>
        /// Sets the dynamicForkTasksInputParamName
        /// </summary>
        /// <param name="dynamicForkTasksInputParamName"></param>
        public void SetDynamicForkTasksInputParamName(string dynamicForkTasksInputParamName)
        {
            this.DynamicForkTasksInputParamName = dynamicForkTasksInputParamName;
        }
    }
}
