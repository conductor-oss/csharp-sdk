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
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Conductor.Examples
{
    /// <summary>
    /// Demonstrates worker auto-discovery by scanning assemblies for IWorkflowTask implementations.
    /// This is the C# equivalent of Python's WorkerLoader.scan_packages().
    /// </summary>
    public class WorkerDiscoveryExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Worker Discovery Example ===\n");

            // 1. Discover workers in the current assembly
            Console.WriteLine("1. Scanning current assembly for workers...");
            var currentAssembly = Assembly.GetExecutingAssembly();
            var workers = DiscoverWorkers(currentAssembly);
            Console.WriteLine($"   Found {workers.Count} worker(s) in {currentAssembly.GetName().Name}:");
            foreach (var worker in workers)
            {
                Console.WriteLine($"   - {worker.FullName}");
            }

            // 2. Scan multiple assemblies
            Console.WriteLine("\n2. Scanning all loaded assemblies...");
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allWorkers = new List<Type>();
            foreach (var assembly in allAssemblies)
            {
                try
                {
                    var found = DiscoverWorkers(assembly);
                    allWorkers.AddRange(found);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }
            Console.WriteLine($"   Found {allWorkers.Count} total worker(s) across all assemblies.");

            // 3. Show how to create instances
            Console.WriteLine("\n3. Creating worker instances...");
            foreach (var workerType in workers)
            {
                try
                {
                    var constructor = workerType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        var instance = Activator.CreateInstance(workerType);
                        Console.WriteLine($"   Created: {workerType.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"   Skipped {workerType.Name} (no parameterless constructor)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   Error creating {workerType.Name}: {ex.Message}");
                }
            }

            Console.WriteLine("\nWorker Discovery Example completed!");
        }

        /// <summary>
        /// Discovers all types implementing IWorkflowTask in the given assembly.
        /// </summary>
        public static List<Type> DiscoverWorkers(Assembly assembly)
        {
            var workflowTaskType = typeof(IWorkflowTask);
            return assembly.GetTypes()
                .Where(t => workflowTaskType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
        }
    }
}
