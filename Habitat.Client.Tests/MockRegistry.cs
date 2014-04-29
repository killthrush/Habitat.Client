using System.Net.Http;
using Moq;
using ProTeck.Config.Dto.V1;
using ProTeck.Core.Log;
using ProTeck.Core.Repository;
using ProTeck.Core.TestingLibrary;
using StructureMap.Configuration.DSL;

namespace Habitat.Client.Tests
{
    /// <summary>
    /// Class used to establish the bindings used for this test fixture's Dependency Injection
    /// </summary>
    internal class MockRegistry : Registry
    {
        public const string NoHttpConnection = "NoHttpConnection";
        public const string BadHttpAddress = "BadHttpAddress";
        public const string ConnectionTimeout = "ConnectionTimeout";
        public const string ServerAlwaysReturns500Error = "ServerAlwaysReturns500Error";
        public const string ServerReturnsGibberish = "ServerReturnsGibberish";
        public const string ServerAlwaysReturnsJsonArray = "ServerAlwaysReturnsJsonArray";
        public const string ServerAlwaysReturnsJsonObject = "ServerAlwaysReturnsJsonObject";

        /// <summary>
        /// Constructs a MockRegistry instance that contains bindings for mock/fake objects
        /// </summary>
        public MockRegistry()
        {
            For<IConfigProvider>().Use<IConfigProvider>();
            For<IConfigServiceProvider>().Use<ConfigServiceProvider>();
            For<IRepository<IJsonEntity<ConfigRoot>>>().Use<DurableMemoryRepository<ConfigRoot>>();
            For<ILog>().Use(x => CreateMockLogger());

            For<HttpClient>().Use(() => HttpClientTestHelper.CreateStandardFakeClient(new MockConfigService()));

            Profile(NoHttpConnection, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientSimulatingServerWithNoHttpEndpoint));
            Profile(BadHttpAddress, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientSimulatingABadAddress));
            Profile(ConnectionTimeout, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientSimulatingRequestTimeout));
            Profile(ServerAlwaysReturns500Error, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientThatAlwaysThrowsServerError));
            Profile(ServerReturnsGibberish, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientThatAlwaysReturnsGibberish));
            Profile(ServerAlwaysReturnsJsonArray, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientThatAlwaysReturnsJsonArray));
            Profile(ServerAlwaysReturnsJsonObject, x => x.For<HttpClient>().Use(HttpClientTestHelper.CreateClientThatAlwaysReturnsJsonObject));
        }

        private ILog CreateMockLogger()
        {
            Mock<ILog> mockLogger = new Mock<ILog>(MockBehavior.Strict);
            return mockLogger.Object;
        }
    }
}