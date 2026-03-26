using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Worker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harness
{
    public class SimulatedTaskWorker : IWorkflowTask
    {
        private readonly string _taskName;
        private readonly string _codename;
        private readonly int _defaultDelayMs;
        private readonly int _batchSize;
        private readonly TimeSpan _pollInterval;
        private readonly Random _random = new();

        private const string AlphanumericChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public SimulatedTaskWorker(string taskName, string codename, int sleepSeconds,
            int batchSize = 5, int pollIntervalMs = 1000)
        {
            _taskName = taskName;
            _codename = codename;
            _defaultDelayMs = sleepSeconds * 1000;
            _batchSize = batchSize;
            _pollInterval = TimeSpan.FromMilliseconds(pollIntervalMs);
        }

        public string TaskType => _taskName;

        public WorkflowTaskExecutorConfiguration WorkerSettings => new()
        {
            WorkerId = _taskName,
            BatchSize = _batchSize,
            PollInterval = _pollInterval
        };

        public async Task<Conductor.Client.Models.TaskResult> Execute(
            Conductor.Client.Models.Task task, CancellationToken token = default)
        {
            var input = task.InputData ?? new Dictionary<string, object>();
            var taskId = task.TaskId;
            int taskIndex = GetOrDefault(input, "taskIndex", -1);

            Console.WriteLine(
                $"[{_taskName}] Starting simulated task [id={taskId}, index={taskIndex}, codename={_codename}]");

            var stopwatch = Stopwatch.StartNew();

            var delayType = GetOrDefault(input, "delayType", "fixed");
            int minDelay = GetOrDefault(input, "minDelay", _defaultDelayMs);
            int maxDelay = GetOrDefault(input, "maxDelay", minDelay + 100);
            int meanDelay = GetOrDefault(input, "meanDelay", (minDelay + maxDelay) / 2);
            int stdDeviation = GetOrDefault(input, "stdDeviation", 30);
            double successRate = GetOrDefault(input, "successRate", 1.0);
            var failureMode = GetOrDefault(input, "failureMode", "random");
            int outputSize = GetOrDefault(input, "outputSize", 1024);

            long delayMs = 0;
            if (!string.Equals(delayType, "wait", StringComparison.OrdinalIgnoreCase))
            {
                delayMs = CalculateDelay(delayType, minDelay, maxDelay, meanDelay, stdDeviation);

                Console.WriteLine(
                    $"[{_taskName}] Simulated task [id={taskId}, index={taskIndex}] sleeping for {delayMs} ms");
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs), token);
            }

            if (!ShouldTaskSucceed(successRate, failureMode, input))
            {
                Console.WriteLine(
                    $"[{_taskName}] Simulated task [id={taskId}, index={taskIndex}] failed as configured");
                throw new SimulatedTaskException("Simulated task failure based on configuration");
            }

            stopwatch.Stop();
            var output = GenerateOutput(input, taskId, taskIndex, delayMs, stopwatch.ElapsedMilliseconds, outputSize);
            return task.Completed(outputData: output);
        }

        public Conductor.Client.Models.TaskResult Execute(Conductor.Client.Models.Task task)
        {
            return Execute(task, CancellationToken.None).GetAwaiter().GetResult();
        }

        private long CalculateDelay(string delayType, int minDelay, int maxDelay, int meanDelay, int stdDeviation)
        {
            switch (delayType.ToLowerInvariant())
            {
                case "fixed":
                    return minDelay;

                case "random":
                    return minDelay + _random.Next(Math.Max(1, maxDelay - minDelay + 1));

                case "normal":
                    double gaussian = NextGaussian();
                    long delay = (long)Math.Round(meanDelay + gaussian * stdDeviation);
                    return Math.Max(1, delay);

                case "exponential":
                    double exp = -meanDelay * Math.Log(1 - _random.NextDouble());
                    return Math.Max(minDelay, Math.Min(maxDelay, (long)exp));

                default:
                    return minDelay;
            }
        }

        private double NextGaussian()
        {
            // Box-Muller transform
            double u1 = 1.0 - _random.NextDouble();
            double u2 = _random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        private bool ShouldTaskSucceed(double successRate, string failureMode, Dictionary<string, object> input)
        {
            bool? forceSuccess = GetNullableOrDefault<bool>(input, "forceSuccess");
            if (forceSuccess.HasValue)
                return forceSuccess.Value;

            bool? forceFail = GetNullableOrDefault<bool>(input, "forceFail");
            if (forceFail.HasValue)
                return !forceFail.Value;

            switch (failureMode.ToLowerInvariant())
            {
                case "random":
                    return _random.NextDouble() < successRate;

                case "conditional":
                    int taskIndex = GetOrDefault(input, "taskIndex", -1);
                    if (taskIndex >= 0)
                    {
                        if (input.TryGetValue("failIndexes", out var failIndexesObj) && failIndexesObj is IEnumerable indexes)
                        {
                            foreach (var index in indexes)
                            {
                                if (index?.ToString() == taskIndex.ToString())
                                    return false;
                            }
                        }

                        int failEvery = GetOrDefault(input, "failEvery", 0);
                        if (failEvery > 0 && taskIndex % failEvery == 0)
                            return false;
                    }
                    return _random.NextDouble() < successRate;

                case "sequential":
                    int attempt = GetOrDefault(input, "attempt", 1);
                    int failUntilAttempt = GetOrDefault(input, "failUntilAttempt", 2);
                    return attempt >= failUntilAttempt;

                default:
                    return _random.NextDouble() < successRate;
            }
        }

        private Dictionary<string, object> GenerateOutput(
            Dictionary<string, object> input, string taskId, int taskIndex,
            long delayMs, long elapsedTimeMs, int outputSize)
        {
            var output = new Dictionary<string, object>
            {
                ["taskId"] = taskId,
                ["taskIndex"] = taskIndex,
                ["codename"] = _codename,
                ["status"] = "completed",
                ["configuredDelayMs"] = delayMs,
                ["actualExecutionTimeMs"] = elapsedTimeMs,
                ["a_or_b"] = _random.Next(100) > 20 ? "a" : "b",
                ["c_or_d"] = _random.Next(100) > 33 ? "c" : "d",
            };

            if (GetOrDefault(input, "includeInput", false))
                output["input"] = input;

            if (input.TryGetValue("previousTaskOutput", out var previousTaskOutput) && previousTaskOutput != null)
                output["previousTaskData"] = previousTaskOutput;

            if (outputSize > 0)
                output["data"] = GenerateRandomData(outputSize);

            if (GetMapOrDefault(input, "outputTemplate") is { } template)
            {
                foreach (var kvp in template)
                    output[kvp.Key] = kvp.Value;
            }

            return output;
        }

        private string GenerateRandomData(int size)
        {
            if (size <= 0)
                return string.Empty;

            var sb = new StringBuilder(size);
            for (int i = 0; i < size; i++)
                sb.Append(AlphanumericChars[_random.Next(AlphanumericChars.Length)]);

            return sb.ToString();
        }

        private static T GetOrDefault<T>(Dictionary<string, object> map, string key, T defaultValue)
        {
            if (!map.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            try
            {
                if (value is T typed)
                    return typed;

                if (typeof(T) == typeof(int) && value is IConvertible)
                    return (T)(object)Convert.ToInt32(value);

                if (typeof(T) == typeof(double) && value is IConvertible)
                    return (T)(object)Convert.ToDouble(value);

                if (typeof(T) == typeof(bool) && value is IConvertible)
                    return (T)(object)Convert.ToBoolean(value);

                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString();

                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static T? GetNullableOrDefault<T>(Dictionary<string, object> map, string key) where T : struct
        {
            if (!map.TryGetValue(key, out var value) || value == null)
                return null;

            try
            {
                if (value is T typed)
                    return typed;

                if (typeof(T) == typeof(bool) && value is IConvertible)
                    return (T)(object)Convert.ToBoolean(value);

                return (T)value;
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<string, object> GetMapOrDefault(
            Dictionary<string, object> map, string key)
        {
            if (map.TryGetValue(key, out var value) && value is Dictionary<string, object> dict)
                return dict;
            return null;
        }

        public class SimulatedTaskException : Exception
        {
            public SimulatedTaskException(string message) : base(message) { }
        }
    }
}
