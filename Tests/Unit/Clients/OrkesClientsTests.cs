using Conductor.Client;
using Conductor.Client.Interfaces;
using Conductor.Client.Orkes;
using Xunit;

namespace Tests.Unit.Clients
{
    public class OrkesClientsTests
    {
        private readonly OrkesClients _orkesClients;

        public OrkesClientsTests()
        {
            var configuration = new Configuration
            {
                BasePath = "https://test-conductor.example.com/api"
            };
            _orkesClients = new OrkesClients(configuration);
        }

        [Fact]
        public void GetWorkflowClient_ReturnsOrkesWorkflowClient()
        {
            var client = _orkesClients.GetWorkflowClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesWorkflowClient>(client);
        }

        [Fact]
        public void GetTaskClient_ReturnsOrkesTaskClient()
        {
            var client = _orkesClients.GetTaskClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesTaskClient>(client);
        }

        [Fact]
        public void GetMetadataClient_ReturnsOrkesMetadataClient()
        {
            var client = _orkesClients.GetMetadataClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesMetadataClient>(client);
        }

        [Fact]
        public void GetSchedulerClient_ReturnsOrkesSchedulerClient()
        {
            var client = _orkesClients.GetSchedulerClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesSchedulerClient>(client);
        }

        [Fact]
        public void GetSecretClient_ReturnsOrkesSecretClient()
        {
            var client = _orkesClients.GetSecretClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesSecretClient>(client);
        }

        [Fact]
        public void GetAuthorizationClient_ReturnsOrkesAuthorizationClient()
        {
            var client = _orkesClients.GetAuthorizationClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesAuthorizationClient>(client);
        }

        [Fact]
        public void GetPromptClient_ReturnsOrkesPromptClient()
        {
            var client = _orkesClients.GetPromptClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesPromptClient>(client);
        }

        [Fact]
        public void GetIntegrationClient_ReturnsOrkesIntegrationClient()
        {
            var client = _orkesClients.GetIntegrationClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesIntegrationClient>(client);
        }

        [Fact]
        public void GetEventClient_ReturnsOrkesEventClient()
        {
            var client = _orkesClients.GetEventClient();
            Assert.NotNull(client);
            Assert.IsType<OrkesEventClient>(client);
        }

        [Fact]
        public void AllClients_ImplementCorrectInterfaces()
        {
            Assert.IsAssignableFrom<IWorkflowClient>(_orkesClients.GetWorkflowClient());
            Assert.IsAssignableFrom<ITaskClient>(_orkesClients.GetTaskClient());
            Assert.IsAssignableFrom<IMetadataClient>(_orkesClients.GetMetadataClient());
            Assert.IsAssignableFrom<ISchedulerClient>(_orkesClients.GetSchedulerClient());
            Assert.IsAssignableFrom<ISecretClient>(_orkesClients.GetSecretClient());
            Assert.IsAssignableFrom<IAuthorizationClient>(_orkesClients.GetAuthorizationClient());
            Assert.IsAssignableFrom<IPromptClient>(_orkesClients.GetPromptClient());
            Assert.IsAssignableFrom<IIntegrationClient>(_orkesClients.GetIntegrationClient());
            Assert.IsAssignableFrom<IEventClient>(_orkesClients.GetEventClient());
        }

        [Fact]
        public void MultipleCalls_ReturnNewInstances()
        {
            var client1 = _orkesClients.GetWorkflowClient();
            var client2 = _orkesClients.GetWorkflowClient();
            Assert.NotSame(client1, client2);
        }
    }
}
