using System;
using System.Collections.Generic;
using System.Reflection;

namespace Conductor.Client.Worker
{

    public sealed class WorkerDiscoveryOptions
    {
        public bool EnableAttributeDiscovery { get; set; } = true;

        /// <summary>
        /// Empty means: preserve current behaviour and scan all loaded assemblies.
        /// </summary>
        public IReadOnlyCollection<Assembly> Assemblies { get; set; } = Array.Empty<Assembly>();
    }
}