using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks.Schedulers;
using Habitat.Core;
using Habitat.Core.TestingLibrary;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Habitat.Client.Tests
{
    [TestClass]
    public class ConfigServiceProviderTests
    {
        private const string ResourceUrlTemplate = "Config/{0}";
        private const string ResourceName = "foo";

        private readonly CompareObjects _objectComparer = new CompareObjects();

        [ClassInitialize]
        public static void FixtureSetUp(TestContext context)
        {
            // This hack uses a TPL extension that forces tasks to run on a single thread without needing to change any application code
            new CurrentThreadTaskScheduler().SetDefaultScheduler();
        }

        [TestMethod]
        public void Test_provider_behavior_when_config_service_returns_valid_data()
        {
            ConfigRoot testConfig = MockConfigService.GetConfigRoot(ResourceName);
            DateTime expectedDate = testConfig.LastModified;

            HttpClient mockClient = HttpClientTestHelper.CreateStandardFakeClient(new MockConfigService());
            var provider = new ConfigServiceProvider(ResourceName, mockClient);

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual(ResourceName, response.Config.ComponentName);
            Assert.AreEqual(expectedDate, response.Config.LastModified);
            Assert.IsNotNull(response.Config.Data);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(response.Exception);
            Assert.IsNotNull(response.Config);
            Assert.IsTrue(_objectComparer.Compare(testConfig, response.Config));
        }

        [TestMethod]
        public void Test_provider_behavior_when_config_service_returns_404()
        {
            HttpClient mockClient = HttpClientTestHelper.CreateStandardFakeClient(new MockConfigService());
            var provider = new ConfigServiceProvider("foo2", mockClient);

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual("foo2", response.Config.ComponentName);
            Assert.AreEqual(default(DateTime), response.Config.LastModified);
            Assert.IsNull(response.Config.Data);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsNotNull(response.Exception);
            Assert.AreEqual(typeof(UnableToAccessConfigurationException), response.Exception.GetType());
            Assert.IsNull(response.Exception.InnerException);
        }

        [TestMethod]
        public void Test_provider_behavior_when_config_service_address_does_not_resolve()
        {
            var provider = new ConfigServiceProvider(ResourceUrlTemplate, HttpClientTestHelper.CreateClientSimulatingABadAddress());

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual(ResourceUrlTemplate, response.Config.ComponentName);
            Assert.AreEqual(default(DateTime), response.Config.LastModified);
            Assert.IsNull(response.Config.Data);
            Assert.AreEqual(default(HttpStatusCode), response.StatusCode);
            Assert.IsNotNull(response.Exception);
            Assert.AreEqual(typeof(UnableToAccessConfigurationException), response.Exception.GetType());
            Assert.AreEqual(typeof(AggregateException), response.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void Test_provider_behavior_when_config_service_returns_invalid_data()
        {
            var provider = new ConfigServiceProvider(ResourceUrlTemplate, HttpClientTestHelper.CreateClientThatAlwaysReturnsGibberish());

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual(ResourceUrlTemplate, response.Config.ComponentName);
            Assert.AreEqual(default(DateTime), response.Config.LastModified);
            Assert.IsNull(response.Config.Data);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(response.Exception);
            Assert.AreEqual(typeof(UnableToAccessConfigurationException), response.Exception.GetType());
            Assert.AreEqual(typeof(AggregateException), response.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void Test_provider_behavior_when_config_service_address_does_not_accept_http()
        {
            var provider = new ConfigServiceProvider(ResourceUrlTemplate, HttpClientTestHelper.CreateClientSimulatingServerWithNoHttpEndpoint());

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual(ResourceUrlTemplate, response.Config.ComponentName);
            Assert.AreEqual(default(DateTime), response.Config.LastModified);
            Assert.IsNull(response.Config.Data);
            Assert.AreEqual(default(HttpStatusCode), response.StatusCode);
            Assert.IsNotNull(response.Exception);
            Assert.AreEqual(typeof(UnableToAccessConfigurationException), response.Exception.GetType());
            Assert.AreEqual(typeof(AggregateException), response.Exception.InnerException.GetType());
        }

        [TestMethod]
        public void Test_provider_behavior_when_config_service_returns_500()
        {
            var provider = new ConfigServiceProvider(ResourceUrlTemplate, HttpClientTestHelper.CreateClientThatAlwaysThrowsServerError());

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual(ResourceUrlTemplate, response.Config.ComponentName);
            Assert.AreEqual(default(DateTime), response.Config.LastModified);
            Assert.IsNull(response.Config.Data);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.IsNotNull(response.Exception);
            Assert.AreEqual(typeof(UnableToAccessConfigurationException), response.Exception.GetType());
            Assert.IsNull(response.Exception.InnerException);
        }

        [TestMethod]
        public void Test_provider_behavior_when_there_is_a_timeout_connecting_to_config_service()
        {
            var provider = new ConfigServiceProvider(ResourceUrlTemplate, HttpClientTestHelper.CreateClientSimulatingRequestTimeout());

            ConfigServiceResponse response = provider.GetConfig();

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Config);
            Assert.AreEqual(ResourceUrlTemplate, response.Config.ComponentName);
            Assert.AreEqual(default(DateTime), response.Config.LastModified);
            Assert.IsNull(response.Config.Data);
            Assert.AreEqual(default(HttpStatusCode), response.StatusCode);
            Assert.IsNotNull(response.Exception);
            Assert.AreEqual(typeof(UnableToAccessConfigurationException), response.Exception.GetType());
            Assert.AreEqual(typeof(AggregateException), response.Exception.InnerException.GetType());
        }
    }
}
