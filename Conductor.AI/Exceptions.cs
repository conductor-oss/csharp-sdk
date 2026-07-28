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
namespace Conductor.AI;

/// <summary>Base exception for all Conductor errors.</summary>
public class ConductorAgentException : Exception
{
    public ConductorAgentException(string message) : base(message) { }
    public ConductorAgentException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Server returned an HTTP error.</summary>
public class AgentApiException : ConductorAgentException
{
    public int StatusCode { get; }
    public string? ResponseBody { get; }

    public AgentApiException(int statusCode, string message, string? body = null)
        : base($"API error {statusCode}: {message}")
    {
        StatusCode = statusCode;
        ResponseBody = body;
    }
}

/// <summary>Invalid agent configuration.</summary>
public class ConfigurationException : ConductorAgentException
{
    public ConfigurationException(string message) : base(message) { }
}

/// <summary>Agent not found on server.</summary>
public class AgentNotFoundException : ConductorAgentException
{
    public string AgentName { get; }
    public AgentNotFoundException(string agentName)
        : base($"Agent not found: {agentName}") => AgentName = agentName;
}

/// <summary>Credential not found in the credential store.</summary>
public class CredentialNotFoundException : ConductorAgentException
{
    public string CredentialName { get; }
    public CredentialNotFoundException(string name)
        : base($"Credential not found: {name}") => CredentialName = name;
}

/// <summary>
/// Tool threw a terminal (non-retryable) error.
/// Maps to Conductor's FAILED_WITH_TERMINAL_ERROR status.
/// </summary>
public class TerminalToolException : ConductorAgentException
{
    public TerminalToolException(string message) : base(message) { }
    public TerminalToolException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// The SSE stream for an agent execution could not be established (initial
/// connect failed or returned a non-2xx status). Callers should fall back to
/// status polling.
/// </summary>
public class SSEUnavailableException : ConductorAgentException
{
    public SSEUnavailableException(string message) : base(message) { }
    public SSEUnavailableException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// A stateful run's task sat unpolled past the configured stall threshold —
/// the local worker that owns this run's domain likely died. Thrown by
/// blocking waiters instead of hanging forever (spec R11).
/// </summary>
public class WorkerStallException : ConductorAgentException
{
    public string TaskReferenceName { get; }
    public string ExecutionId { get; }

    public WorkerStallException(string taskReferenceName, string executionId)
        : base($"Worker stall detected: task '{taskReferenceName}' has not been polled for execution "
             + $"'{executionId}'. The worker handling this run's domain may have died.")
    {
        TaskReferenceName = taskReferenceName;
        ExecutionId = executionId;
    }
}
