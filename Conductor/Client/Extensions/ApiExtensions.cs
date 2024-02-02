using Conductor.Api;
using Conductor.Executor;
using Conductor.Client.Authentication;
using System;
using System.Diagnostics;

namespace Conductor.Client.Extensions
{
    public class ApiExtensions
    {
        private const string ENV_ROOT_URI = "CONDUCTOR_SERVER_URL";
        private const string ENV_KEY_ID = "KEY";
        private const string ENV_SECRET = "SECRET";
        private const int REST_CLIENT_REQUEST_TIME_OUT = 30 * 1000;

        public static Configuration Configuration { get; set; }

        static ApiExtensions()
        {
            Configuration = new Configuration(REST_CLIENT_REQUEST_TIME_OUT)
            {
                BasePath = GetEnvironmentVariable(ENV_ROOT_URI),
                AuthenticationSettings = new OrkesAuthenticationSettings(
                GetEnvironmentVariable(ENV_KEY_ID),
                GetEnvironmentVariable(ENV_SECRET)
            )
            };
        }

        public static WorkflowExecutor GetWorkflowExecutor()
        {
            return new WorkflowExecutor(
            metadataClient: GetClient<MetadataResourceApi>(),
            workflowClient: GetClient<WorkflowResourceApi>()
            );
        }

        public static T GetClient<T>() where T : IApiAccessor, new()
        {
            return Configuration.GetClient<T>();
        }

        public static Configuration GetConfiguration()
        {
            return Configuration;
        }

        public static string GetWorkflowExecutionURL(string workflowId)
        {
            var basePath = Configuration.BasePath;
            var prefix = basePath.Remove(basePath.Length - 4);
            return $"{prefix}/execution/{workflowId}";
        }

        private static string GetEnvironmentVariable(string variable)
        {
            string value = Environment.GetEnvironmentVariable(variable);
            Debug.Assert(value != null);
            return value;
        }
    }
}
