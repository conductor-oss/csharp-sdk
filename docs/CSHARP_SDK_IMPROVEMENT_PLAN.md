# Conductor C# SDK Improvement Plan

## Making C# SDK on par with the Python SDK

### Context

The Python SDK (conductor-python) is the gold-standard reference implementation with 33+ examples, 12 high-level clients, 44 task types, full telemetry, event system, comprehensive testing, and rich documentation. The C# SDK has a solid foundation (16 API clients, 20+ task types, AI orchestration, worker framework) but has significant gaps. This plan addresses every gap systematically.

---

## GAP ANALYSIS: What's Missing in C#

### A. CRITICAL BUGS TO FIX FIRST (5 bugs)

| # | Bug | File | Issue |
|---|-----|------|-------|
| 1 | Namespace typo | `Conductor/Definition/TaskType/LlmTasks/LlmIndexText.cs:18` | `Conductor.DefinitaskNametion.TaskType.LlmTasks` should be `Conductor.Definition.TaskType.LlmTasks` |
| 2 | Duplicate key in LlmChatComplete | `Conductor/Definition/TaskType/LlmTasks/LlmChatComplete.cs:173` | `WithInput(Constants.MAXTOKENS, StopWords)` should be `WithInput(Constants.STOPWORDS, StopWords)` |
| 3 | Inverted cancellation check | `Conductor/Client/Worker/WorkflowTaskExecutor.cs:254` | `if (token == CancellationToken.None)` should be `if (token != CancellationToken.None)` |
| 4 | Stack trace destroyed | `Conductor/Client/Worker/WorkflowTaskService.cs:50` | `throw ex;` should be `throw;` |
| 5 | Interface typo | `Conductor/Client/Interfaces/IWorkflowTaskCoodinator.cs` | `IWorkflowTaskCoodinator` should be `IWorkflowTaskCoordinator` (missing 'r') |

### B. HIGH-LEVEL CLIENT LAYER (Missing entirely)

**Python has**: 12 abstract client interfaces (ABCs) + Orkes implementations + `OrkesClients` factory class

**C# has**: Only raw API clients (RestSharp-based auto-generated code). No high-level abstraction.

**Missing Clients to add:**

| Client | Python Methods | C# Status |
|--------|---------------|-----------|
| `IWorkflowClient` | start, execute, pause, resume, terminate, restart, retry, rerun, skip_task, search, get_by_correlation_ids, update_variables, update_state | Missing (raw API exists) |
| `ITaskClient` | poll, batch_poll, get, update, update_sync, queue_size, add_log, get_logs | Missing (raw API exists) |
| `IMetadataClient` | register/update/unregister/get workflows & tasks, tagging | Missing (raw API exists) |
| `ISchedulerClient` | save/get/delete/pause/resume schedules, execution times, search, tags | Missing (raw API exists) |
| `ISecretClient` | put/get/delete/exists/list secrets, tags | Missing (raw API exists) |
| `IAuthorizationClient` | ~49 methods: apps, users, groups, roles, permissions, tokens, gateway auth | Missing (raw API exists) |
| `IPromptClient` | save/get/delete prompts, test, tags | Missing (raw API exists) |
| `IIntegrationClient` | integrations & APIs CRUD, prompt associations, token tracking, tags | Missing (raw API exists) |
| `ISchemaClient` | register/get/delete schemas | **Completely missing** (no API client at all) |
| `IServiceRegistryClient` | service CRUD, circuit breaker, methods, protobuf, discovery | **Completely missing** (no API client at all) |
| `IEventClient` | Queue configuration CRUD (Kafka, general) | Missing (EventResourceApi exists but limited) |
| `OrkesClients` | Factory creating all clients from Configuration | Missing |

### C. MISSING TASK TYPES IN DSL

**Python has 44 task types, C# has ~24. Missing:**

| Task Type | Python Class | C# Status |
|-----------|-------------|-----------|
| `HttpPollTask` | `HttpPollTask` | Missing |
| `KafkaPublish` | `KafkaPublish` | Missing |
| `StartWorkflowTask` | `StartWorkflowTask` | Missing |
| `InlineTask` | `InlineTask` | Missing (has `JavascriptTask` but not generic inline) |
| `LlmStoreEmbeddings` | `LlmStoreEmbeddings` | Missing |
| `LlmSearchEmbeddings` | `LlmSearchEmbeddings` | Missing |
| `GetDocument` | `GetDocument` | Missing |
| `GenerateImage` | `GenerateImage` | Missing |
| `GenerateAudio` | `GenerateAudio` | Missing |
| `ListMcpTools` | `ListMcpTools` | Missing |
| `CallMcpTool` | `CallMcpTool` | Missing |
| `ToolCall` | `ToolCall` | Missing |
| `ToolSpec` | `ToolSpec` | Missing |
| `ChatMessage` | `ChatMessage` | Missing |

### D. WORKER FRAMEWORK GAPS

| Feature | Python | C# |
|---------|--------|----|
| Adaptive exponential backoff on empty queues | Yes | No (fixed 10ms sleep) |
| Auto-restart on worker crash | Yes (configurable max retries) | No |
| Worker auto-discovery via assembly scanning | Yes (`WorkerLoader.scan_packages()`) | Partial (assembly scanning exists but limited) |
| 3-tier hierarchical configuration | Yes (code < global env < worker-specific env) | No |
| Runtime pausing via env vars | Yes | No |
| Lease extension for long-running tasks | Yes (>30s) | No |
| Health checks (`is_healthy()`) | Yes (per-worker process status) | No |
| True async worker support | Yes (auto-detected, async event loop) | Partial (async interface exists but limited) |
| Metrics implementation | Yes (Prometheus with quantile gauges) | No (documented but not implemented) |

### E. EVENT SYSTEM (Missing entirely)

**Python has**: Full event system with sync/async dispatchers, listener registration, protocol-based interfaces (TaskRunnerEventsListener, WorkflowEventsListener, TaskEventsListener), queue implementations

**C# has**: Nothing equivalent

### F. TELEMETRY/METRICS (Missing entirely)

**Python has**: Prometheus metrics with sliding window (1000 observations), quantile gauges (p50/p75/p90/p95/p99), categories: API latency, task polling, task execution, task updates, queue saturation, worker restarts, payload sizes

**C# has**: Metrics documented in `docs/readme/workers.md` but zero actual implementation in code

### G. TESTING GAPS

| Category | Python | C# |
|----------|--------|----|
| Unit tests | 11 subdirectories | Zero |
| Integration tests | Full E2E suite | Yes (exists) |
| Serialization tests | 57 serde test files | Zero |
| Chaos tests | Yes | Zero |
| Backward compat tests | Yes | Zero |
| Workflow test framework | `task_ref_to_mock_output` mocking | None |

### H. EXAMPLES GAPS

**Python has 33+ examples. C# is missing these categories:**

| Category | Python Examples | C# Equivalent |
|----------|----------------|---------------|
| Kitchen Sink (all task types) | `kitchensink.py` | Missing |
| Metadata Journey (20 APIs) | `metadata_journey.py` | Missing |
| Authorization Journey (49 APIs) | `authorization_journey.py` | Missing |
| Schedule Journey (15 APIs) | `schedule_journey.py` | Missing |
| Prompt Journey (8 APIs) | `prompt_journey.py` | Missing |
| Worker Configuration | `worker_configuration_example.py` | Missing |
| Event Listeners | `event_listener_examples.py` | Missing |
| Metrics | `metrics_example.py` | Missing |
| Workflow Ops (lifecycle) | `workflow_ops.py` | Partial (`WorkFlowExamples.cs`) |
| Workflow Testing | `test_workflows.py` | Missing |
| LLM Chat | `agentic_workflows/llm_chat.py` | Partial (`OpenAIChatGpt.cs`) |
| Human-in-Loop Chat | `llm_chat_human_in_loop.py` | Missing |
| Multi-Agent Chat | `multiagent_chat.py` | Missing |
| Function Calling | `function_calling_example.py` | Partial (`OpenAIFunctionExample.cs`) |
| MCP Agent | `mcp_weather_agent.py` | Missing |
| RAG Pipeline | `rag_workflow.py` | Missing |
| ASP.NET Core integration | `fastapi_worker_service.py` | Missing |
| Worker Auto-Discovery | `worker_discovery/` | Missing |
| Dynamic Workflow | `dynamic_workflow.py` | Partial (`DynamicWorkflow.cs`) |

### I. AI/LLM PROVIDER GAPS

| Provider | Python | C# |
|----------|--------|----|
| OpenAI | Yes | Yes |
| Azure OpenAI | Yes | Yes |
| GCP Vertex AI | Yes | Yes |
| HuggingFace | Yes | Yes |
| **Anthropic** | Yes | **Missing** |
| **AWS Bedrock** | Yes | **Missing** |
| **Cohere** | Yes | **Missing** |
| **Grok** | Yes | **Missing** |
| **Mistral** | Yes | **Missing** |
| **Ollama** | Yes | **Missing** |
| **Perplexity** | Yes | **Missing** |
| Pinecone (VectorDB) | Yes | Yes |
| Weaviate (VectorDB) | Yes | Yes |
| **PostgreSQL/pgvector** | Yes | **Missing** |
| **MongoDB (VectorDB)** | Yes | **Missing** |

### J. DOCUMENTATION GAPS

| Document | Python | C# |
|----------|--------|----|
| README | Comprehensive (quick-start, examples, FAQ) | Minimal (64 lines) |
| Workers guide | Detailed `workers.md` | Brief `docs/readme/workers.md` |
| Workflows guide | `workflows.md` (exec modes, lifecycle, search, failure) | Minimal `docs/readme/workflow.md` (30 lines) |
| Worker configuration guide | `WORKER_CONFIGURATION.md` (env vars, Docker/K8s) | Missing |
| Metrics guide | `METRICS.md` (Prometheus reference) | Missing |
| App integration guide | `conductor_apps.md` (testing, CI/CD, versioning) | Missing |
| API reference | Links to detailed guides | Missing |

### K. ASYNC/CONFIGURATION GAPS

| Feature | Python | C# |
|---------|--------|----|
| Many "Async" methods return void | N/A | Yes - should return `Task` |
| SSL/TLS certificate config | Yes | Missing |
| Proxy support | Yes | Missing |
| Custom logging levels | Yes (TRACE level) | Missing |
| Debug mode toggle | Yes | Missing |
| IDisposable on Configuration/ApiClient | N/A | Missing (resource leak risk) |

---

## IMPLEMENTATION PLAN (Phased)

### Phase 1: Bug Fixes & Code Quality (1-2 days)

1. Fix the 5 critical bugs listed in Section A
2. Fix async methods that return `void` to return `Task`
3. Add `IDisposable` to `Configuration`/`ApiClient`
4. Update incorrect README links

**Files to modify:**
- `Conductor/Definition/TaskType/LlmTasks/LlmIndexText.cs`
- `Conductor/Definition/TaskType/LlmTasks/LlmChatComplete.cs`
- `Conductor/Client/Worker/WorkflowTaskExecutor.cs`
- `Conductor/Client/Worker/WorkflowTaskService.cs`
- `Conductor/Client/Interfaces/IWorkflowTaskCoodinator.cs` (rename)
- `README.md`

### Phase 2: High-Level Client Layer (3-5 days)

1. Create abstract interfaces: `IWorkflowClient`, `ITaskClient`, `IMetadataClient`, `ISchedulerClient`, `ISecretClient`, `IAuthorizationClient`, `IPromptClient`, `IIntegrationClient`, `ISchemaClient`, `IEventClient`
2. Create Orkes implementations wrapping existing API clients
3. Create `OrkesClients` factory class
4. Add missing `SchemaResourceApi` and `ServiceRegistryResourceApi`

**New files:**
- `Conductor/Client/Interfaces/` - 10 new interface files
- `Conductor/Client/Orkes/` - 10 new implementation files
- `Conductor/Client/OrkesClients.cs` - Factory

### Phase 3: Missing Task Types (2-3 days)

Add all 14 missing task types to the DSL:
- `HttpPollTask`, `KafkaPublish`, `StartWorkflowTask`, `InlineTask`
- LLM: `LlmStoreEmbeddings`, `LlmSearchEmbeddings`, `GetDocument`, `GenerateImage`, `GenerateAudio`
- MCP: `ListMcpTools`, `CallMcpTool`
- Tool: `ToolCall`, `ToolSpec`, `ChatMessage`

**New files in** `Conductor/Definition/TaskType/`:
- 4 new files for control/integration tasks
- 10 new files in `LlmTasks/`

### Phase 4: Worker Framework Improvements (3-4 days)

1. Adaptive exponential backoff on empty poll queues
2. Worker health checks (`IsHealthy()`)
3. Auto-restart on worker failure with configurable max retries
4. Enhanced worker configuration (3-tier: code < global env < worker-specific env)
5. Lease extension support for long-running tasks
6. Runtime pause/resume via environment variables

**Files to modify:**
- `Conductor/Client/Worker/WorkflowTaskExecutor.cs`
- `Conductor/Client/Worker/WorkflowTaskCoordinator.cs`
- `Conductor/Client/Worker/WorkflowTaskHost.cs`
- New: `Conductor/Client/Worker/WorkerConfiguration.cs`
- New: `Conductor/Client/Worker/WorkerHealthCheck.cs`

### Phase 5: Metrics & Telemetry (2-3 days)

1. Implement metrics collection using `System.Diagnostics.Metrics` (or prometheus-net)
2. Categories: task polling, task execution, task updates, API latency, queue saturation
3. Configurable export (Prometheus endpoint, file export)

**New files:**
- `Conductor/Client/Telemetry/MetricsCollector.cs`
- `Conductor/Client/Telemetry/MetricsConfig.cs`
- `Conductor/Client/Telemetry/WorkerMetrics.cs`

### Phase 6: Event System (2-3 days)

1. Create event listener interfaces (`ITaskEventListener`, `IWorkflowEventListener`, `ITaskRunnerEventListener`)
2. Event dispatcher (sync + async)
3. Hook into worker framework for poll/execution/update events

**New files:**
- `Conductor/Client/Events/` - New directory with ~6 files

### Phase 7: AI/LLM Provider Expansion (1-2 days)

1. Add missing LLM provider enums and config classes: Anthropic, AWS Bedrock, Cohere, Grok, Mistral, Ollama, Perplexity
2. Add missing VectorDB enums and configs: PostgreSQL/pgvector, MongoDB

**Files to modify:**
- `Conductor/Client/Ai/Configuration.cs`
- `Conductor/Client/Ai/Integrations.cs`

### Phase 8: Examples (3-5 days)

Create comprehensive C# examples mirroring Python:

| Priority | Example | Pattern |
|----------|---------|---------|
| P0 | Kitchen Sink (all task types) | `kitchensink.py` |
| P0 | Hello World (simplest workflow) | `helloworld/` |
| P0 | Worker Configuration | `worker_configuration_example.py` |
| P1 | Metadata Journey | `metadata_journey.py` |
| P1 | Authorization Journey | `authorization_journey.py` |
| P1 | Schedule Journey | `schedule_journey.py` |
| P1 | Prompt Journey | `prompt_journey.py` |
| P1 | Workflow Ops (full lifecycle) | `workflow_ops.py` |
| P2 | Workflow Unit Testing | `test_workflows.py` |
| P2 | Human-in-Loop Chat | `llm_chat_human_in_loop.py` |
| P2 | Multi-Agent Chat | `multiagent_chat.py` |
| P2 | MCP Agent | `mcp_weather_agent.py` |
| P2 | RAG Pipeline | `rag_workflow.py` |
| P2 | ASP.NET Core Integration | `fastapi_worker_service.py` equivalent |
| P3 | Event Listeners | `event_listener_examples.py` |
| P3 | Metrics | `metrics_example.py` |
| P3 | Worker Discovery | `worker_discovery/` |

### Phase 9: Testing (3-5 days)

1. Add unit tests with mocking (xUnit + Moq/NSubstitute)
2. Add serialization/deserialization tests for all 83 models
3. Add workflow test framework (`TaskRefToMockOutput` pattern)
4. Target: 90%+ unit test coverage

**New files in** `Tests/`:
- `Tests/Unit/` - New directory with organized subdirectories
- `Tests/SerDeSer/` - Model serialization tests

### Phase 10: Documentation (2-3 days)

1. Rewrite README.md (comprehensive, with quick-start, examples, FAQ)
2. Expand `docs/readme/workers.md`
3. Expand `docs/readme/workflow.md`
4. Create `docs/readme/worker_configuration.md`
5. Create `docs/readme/metrics.md`
6. Create `docs/readme/conductor_apps.md`

---

## VERIFICATION PLAN

1. **Build**: `dotnet build conductor-csharp.sln` - must pass with zero warnings
2. **Existing tests**: `dotnet test` - all existing integration tests still pass
3. **New unit tests**: All new unit tests pass with >90% coverage
4. **Examples**: Each new example compiles and runs against a live Conductor server
5. **Bug fixes**: Write specific regression tests for each of the 5 bugs
6. **NuGet package**: `dotnet pack` produces valid package
7. **API compatibility**: Existing public APIs remain backward-compatible (no breaking changes)

---

## ESTIMATED TOTAL EFFORT

| Phase | Estimate |
|-------|----------|
| Phase 1: Bug Fixes & Code Quality | 1-2 days |
| Phase 2: High-Level Client Layer | 3-5 days |
| Phase 3: Missing Task Types | 2-3 days |
| Phase 4: Worker Framework | 3-4 days |
| Phase 5: Metrics & Telemetry | 2-3 days |
| Phase 6: Event System | 2-3 days |
| Phase 7: AI/LLM Providers | 1-2 days |
| Phase 8: Examples | 3-5 days |
| Phase 9: Testing | 3-5 days |
| Phase 10: Documentation | 2-3 days |
| **Total** | **~22-37 days** |

---

## OPEN QUESTIONS FOR TEAM REVIEW

1. **Metrics library**: Should we use `System.Diagnostics.Metrics` (built-in .NET) or `prometheus-net` (community standard)? Python uses Prometheus directly.
2. **Testing framework**: xUnit is already in the project. Should we use Moq or NSubstitute for mocking?
3. **Phase prioritization**: Are there phases the team wants to reorder or defer? E.g., skip Event System (Phase 6) if not customer-requested?
4. **Breaking changes**: The interface typo fix (Bug #5: `IWorkflowTaskCoodinator` -> `IWorkflowTaskCoordinator`) is technically a breaking change. Ship it in a major version bump, or accept the break?
5. **Min .NET version**: Currently targets .NET 6+. Should we require .NET 8+ to leverage newer APIs (e.g., `System.Diagnostics.Metrics` improvements)?
6. **NuGet versioning**: What version should the improved SDK ship as? Current is likely 1.x - does this warrant a 2.0?
