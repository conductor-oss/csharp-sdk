# Writing Workers with the C# SDK

A worker is responsible for executing a task. 
Operator and System tasks are handled by the Conductor server, while user defined tasks needs to have a worker created that awaits the work to be scheduled by the server for it to be executed.

Worker framework provides features such as polling threads, metrics and server communication.

### Design Principles for Workers
Each worker embodies design pattern and follows certain basic principles:

1. Workers are stateless and do not implement a workflow specific logic. 
2. Each worker executes a very specific task and produces well-defined output given specific inputs. 
3. Workers are meant to be idempotent (or should handle cases where the task that partially executed gets rescheduled due to timeouts etc.)
4. Workers do not implement the logic to handle retries etc, that is taken care by the Conductor server.

### Creating Task Workers
Example worker

```csharp
public class SimpleWorker : IWorkflowTask
{
    public string TaskType { get; }
    public WorkflowTaskExecutorConfiguration WorkerSettings { get; }

    public SimpleWorker(string taskType = "test-sdk-csharp-task")
    {
        TaskType = taskType;
        WorkerSettings = new WorkflowTaskExecutorConfiguration();
    }

    public TaskResult Execute(Task task)
    {
        return task.Completed();
    }
}
```

## Starting Workers
You can use `WorkflowTaskHost` to create a worker host, it requires a configuration object and then you can add your workers.

```csharp
using Conductor.Client.Worker;
using System;
using System.Threading.Thread;

var host = WorkflowTaskHost.CreateWorkerHost(configuration, new SimpleWorker());
await host.startAsync();
Thread.Sleep(TimeSpan.FromSeconds(100));
```

Check out our [integration tests](https://github.com/conductor-oss/csharp-sdk/blob/main/Tests/Worker/WorkerTests.cs) for more examples

The worker SDK collects metrics for poll latency, execution time, update time, error rates, queue
utilization, and more. See [metrics.md](../metrics.md) for the full list of metrics, labels,
configuration options, and example Prometheus output.

### Next: [Create and Execute Workflows](https://github.com/conductor-oss/csharp-sdk/blob/main/docs/readme/workflow.md)
