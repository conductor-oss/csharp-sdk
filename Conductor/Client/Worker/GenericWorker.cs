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
using Client.Worker.Utils;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker.Utils;
using Conductor.Definition.TaskType;
using Definition.TaskType;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conductor.Client.Worker
{
    public class GenericWorker : IWorkflowTask
    {
        public string TaskType { get; set; }
        public WorkflowTaskExecutorConfiguration WorkerSettings { get; set; }

        private readonly object _workerInstance;
        private readonly MethodInfo _executeTaskMethod;
        private readonly List<TaskResult.StatusEnum> failedStatuses = new List<TaskResult.StatusEnum>() { TaskResult.StatusEnum.FAILED, TaskResult.StatusEnum.FAILEDWITHTERMINALERROR };
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericWorker" /> class.
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="workerSettings"></param>
        /// <param name="executeTaskMethod"></param>
        /// <param name="workerInstance"></param>
        public GenericWorker(string taskType, WorkflowTaskExecutorConfiguration workerSettings, MethodInfo executeTaskMethod, object workerInstance = null)
        {
            TaskType = taskType;
            WorkerSettings = workerSettings;
            _executeTaskMethod = executeTaskMethod;
            _workerInstance = workerInstance;
            _jsonSerializerSettings = ObjectMapperProvider.GetJsonSerializerSettings();
        }

        /// <summary>
        /// Executes a task asynchronously and returns a TaskResult.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<TaskResult> Execute(Models.Task task, CancellationToken token)
        {
            if (token != CancellationToken.None && token.IsCancellationRequested)
                return new TaskResult() { Status = TaskResult.StatusEnum.FAILED, ReasonForIncompletion = Constants.TOKENCANCELLATION };

            TaskResult result = null;
            try
            {
                WorkflowTaskContext workflowTaskContext = WorkflowTaskContext.Set(task);
                Object[] parameters = GetInvocationParameters(task);
                Object invocationResult = await System.Threading.Tasks.Task.Run(() => _executeTaskMethod.Invoke(_workerInstance, parameters));
                result = SetValue(invocationResult, workflowTaskContext.GetTaskResult());
                if (!failedStatuses.Contains((TaskResult.StatusEnum)result.Status) && result.CallbackAfterSeconds > 0)
                {
                    result.Status = TaskResult.StatusEnum.INPROGRESS;
                }
            }
            catch (TargetInvocationException targetInvocationException)
            {
                if (result == null)
                {
                    result = WorkerUtil.GetTaskResult(task);
                }

                Exception innerException = targetInvocationException.InnerException;
                if (WorkerUtil.IsTerminalError(innerException))
                {
                    result.Status = TaskResult.StatusEnum.FAILEDWITHTERMINALERROR;
                }
                else
                {
                    result.Status = TaskResult.StatusEnum.FAILED;
                }

                StringBuilder stackTrace = new StringBuilder();
                StackTrace exceptionStackTrace = new StackTrace(innerException);
                foreach (StackFrame stackFrame in exceptionStackTrace.GetFrames())
                {
                    string methodName = stackFrame.GetMethod().Name;
                    string fileName = stackFrame.GetFileName();
                    int lineNumber = stackFrame.GetFileLineNumber();
                    stackTrace.AppendLine($"Method: {methodName}, File: {fileName}, Line: {lineNumber}");
                }
                TaskExecLog taskExecLog = new TaskExecLog()
                {
                    Log = stackTrace.ToString(),
                    TaskId = task.TaskId
                };
                result.Logs = new List<TaskExecLog> { taskExecLog };
            }
            catch (Exception)
            {
                throw new Exception(Constants.RUNTIMEERROR);
            }

            return result;
        }

        public TaskResult Execute(Models.Task task)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns the parameters required for the task invocation.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private object[] GetInvocationParameters(Models.Task task)
        {
            Type[] parameterTypes = _executeTaskMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            ParameterInfo[] parameters = _executeTaskMethod.GetParameters();

            if (parameterTypes.Length == 1 && parameterTypes[0] == typeof(Models.Task))
            {
                return new object[] { task };
            }
            else if (parameterTypes.Length == 1 && parameterTypes[0] == typeof(Dictionary<string, object>))
            {
                return new object[] { task.InputData };
            }

            return GetParameters(task, parameterTypes, parameters);
        }

        /// <summary>
        /// Retrieves parameters for task invocation based on task and method signatures.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private object[] GetParameters(Models.Task task, Type[] parameterTypes, ParameterInfo[] parameters)
        {
            Attribute[][] parameterAnnotations = GetParameterAnnotations();
            Object[] values = new Object[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                Attribute[] annotations = parameterAnnotations[i];
                if (annotations != null && annotations.Length > 0)
                {
                    Type type = parameters[i].ParameterType;
                    Type parameterType = parameterTypes[i];
                    values[i] = GetInputValue(task, parameterType, type, annotations);
                }
                else
                {
                    string parameterName = parameters[i].Name;
                    values[i] = ConvertInputValue(task.InputData[parameterName], parameterTypes[i]);
                }
            }
            return values;
        }

        /// <summary>
        /// Retrieves annotations for parameters of the method.
        /// </summary>
        /// <returns></returns>
        private Attribute[][] GetParameterAnnotations()
        {
            ParameterInfo[] parameters = _executeTaskMethod.GetParameters();
            Attribute[][] parameterAnnotations = new Attribute[parameters.Length][];

            for (int i = 0; i < parameters.Length; i++)
            {
                List<Attribute> annotations = new List<Attribute>();

                foreach (object attribute in parameters[i].GetCustomAttributes(false))
                {
                    if (attribute is Attribute attr)
                    {
                        annotations.Add(attr);
                    }
                }

                parameterAnnotations[i] = annotations.ToArray();
            }

            return parameterAnnotations;
        }

        /// <summary>
        /// Retrieves the value for a parameter based on annotations and input data.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="parameterType"></param>
        /// <param name="type"></param>
        /// <param name="paramAnnotation"></param>
        /// <returns></returns>
        private Object GetInputValue(Models.Task task, Type parameterType, Type type, Attribute[] paramAnnotation)
        {
            InputParam inputParam = FindInputParamAnnotation(paramAnnotation);
            if (inputParam == null)
            {
                ConvertInputValue(task.InputData, parameterType);
            }

            string inputValue = inputParam.Value;
            object value;
            if (task.InputData.ContainsKey(inputValue))
            {
                value = task.InputData[inputValue];
            }
            else
            {
                return null;
            }

            if (typeof(List<object>).IsAssignableFrom(parameterType))
            {
                List<object> list = (List<object>)ConvertInputValue(value, typeof(List<object>));
                if (type.IsGenericType)
                {
                    Type typeOfParameter = type.GetGenericArguments()[0];
                    List<object> parameterizedList = new List<object>();
                    foreach (var item in list)
                    {
                        parameterizedList.Add(ConvertInputValue(item, typeOfParameter));
                    }

                    return parameterizedList;
                }
                else
                {
                    return list;
                }
            }
            else
            {
                return ConvertInputValue(value, parameterType);
            }
        }

        /// <summary>
        /// Converts input value to the specified target type.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private object ConvertInputValue(object input, Type targetType)
        {
            // Serialize the input object to JSON string using ObjectMapperProvider settings
            string jsonString = JsonConvert.SerializeObject(input, _jsonSerializerSettings);

            // Deserialize the JSON string to the target type using ObjectMapperProvider settings
            object result = JsonConvert.DeserializeObject(jsonString, targetType, _jsonSerializerSettings);

            return result;
        }

        /// <summary>
        /// Finds InputParam annotation in a list of parameter annotations.
        /// </summary>
        /// <param name="paramAnnotations"></param>
        /// <returns></returns>
        private static InputParam FindInputParamAnnotation(Attribute[] paramAnnotations)
        {
            return paramAnnotations
            .OfType<InputParam>()
            .FirstOrDefault();
        }

        /// <summary>
        /// Sets the value in TaskResult based on invocation result and annotations.
        /// </summary>
        /// <param name="InvocationResult"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private TaskResult SetValue(object InvocationResult, TaskResult result)
        {
            if (InvocationResult == null)
            {
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;
            }

            OutputParam outputAnnotation = _executeTaskMethod.GetCustomAttribute<OutputParam>();
            if (outputAnnotation != null)
            {
                string name = outputAnnotation.Value;
                result.OutputData = new Dictionary<string, object>()
                {
                    {
                        name,InvocationResult
                    }
                };
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;

            }
            else if (InvocationResult is TaskResult)
            {
                return (TaskResult)InvocationResult;
            }
            else if (InvocationResult is Dictionary<string, object>)
            {
                Dictionary<string, object> ResultAsDictionary = (Dictionary<string, object>)InvocationResult;
                foreach (var kvp in ResultAsDictionary)
                {
                    result.OutputData[kvp.Key] = kvp.Value;
                }
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;
            }
            else if (InvocationResult is string || InvocationResult is int
            || InvocationResult is float || InvocationResult is decimal
            || InvocationResult is double || InvocationResult is bool)
            {
                result.OutputData.Add("result", InvocationResult);
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;
            }
            else if (InvocationResult is List<object>)
            {
                List<object> resultAsList = (List<object>)ConvertInputValue(InvocationResult, typeof(List<object>));
                result.OutputData.Add("result", resultAsList);
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;
            }
            else if (InvocationResult is DynamicForkInput)
            {
                DynamicForkInput forkInput = (DynamicForkInput)InvocationResult;
                List<Definition.TaskType.Task> tasks = forkInput.Tasks;
                List<WorkflowTask> workflowTasks = new List<WorkflowTask>();
                foreach (var sdkTasks in tasks)
                {
                    List<WorkflowTask> task = sdkTasks.GetWorkflowDefTasks();
                    workflowTasks.AddRange(task);
                }
                result.OutputData.Add(DynamicFork.FORK_TASK_PARAM, workflowTasks);
                result.OutputData.Add(DynamicFork.FORK_TASK_INPUT_PARAM, forkInput.Inputs);
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;
            }
            else
            {
                Dictionary<string, object> resultAsDictionary = InvocationResult as Dictionary<string, object>;
                foreach (var kvp in resultAsDictionary)
                {
                    result.OutputData[kvp.Key] = kvp.Value;
                }
                result.Status = TaskResult.StatusEnum.COMPLETED;
                return result;
            }
        }
    }
}

