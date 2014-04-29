using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProTeck.Config.Dto.V1;
using ProTeck.Core.Facades;
using ProTeck.Core.TestingLibrary;

namespace Habitat.Client.Tests
{
    /// <summary>
    /// Test fixture that ensures that the config provider implementation works correctly.
    /// </summary>
    [TestClass]
    public class ConfigProviderTests
    {
        private const string TestComponentName = "foo";
        private const string TestPath = @"d:\foobar\jones\Fake.Name";
        private const string TestTempPath = @"d:\foobar\jones";
        private const string TestAssemblyName = "Fake.Name";

        private readonly CompareObjects _objectComparer = new CompareObjects();
        private Mock<IFileSystemFacade> _mockFileSystem;
        private HttpClient _mockConfigServiceHttpClient;
        private readonly MockFileSystemProvider _mockFileSystemProvider = new MockFileSystemProvider();

        [TestInitialize]
        public void SetUp()
        {
            _mockFileSystem = _mockFileSystemProvider.MockFileSystem;
            _mockFileSystem.Setup(x => x.GetTempDirectoryPath()).Returns(TestTempPath);
            _mockConfigServiceHttpClient = HttpClientTestHelper.CreateStandardFakeClient(new MockConfigService());
        }

        [TestCleanup]
        public void TearDown()
        {
            _mockFileSystemProvider.Reset();
        }

        [TestMethod]
        public void Retrieve_valid_configuration()
        {
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            var validators = new Dictionary<string, Func<string, bool>> { { string.Format("{0}.N1", TestComponentName), x => true }, { string.Format("{0}.N2", TestComponentName), x => true } };
            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            ConfigRoot configuration = configProvider.GetAndValidateConfiguration();
            Dictionary<string, string> dictionary = configuration.Data.ToDictionary();
            Assert.AreEqual("V1", dictionary[string.Format("{0}.N1", TestComponentName)]);
            Assert.AreEqual("V2", dictionary[string.Format("{0}.N2", TestComponentName)]);
        }

        [TestMethod]
        [ExpectedException(typeof(UnableToAccessConfigurationException))]
        public void Broken_validation_handlers_should_cause_config_validation_exception_to_be_thrown()
        {
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            var validators = new Dictionary<string, Func<string, bool>> { { "N1.N1", x => { throw new Exception(); } } };
            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            configProvider.GetAndValidateConfiguration();
        }

        [TestMethod]
        public void Config_should_always_come_from_server_if_it_returns_valid_data()
        {
            ConfigRoot configFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            configFromMockCache.Data.Children[0].Value = "fromcache";
            CreateMockDurableCacheEntry(configFromMockCache);

            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            IConfigProvider configProvider = testFactory.Create(TestComponentName, new Dictionary<string, Func<string, bool>>());
            ConfigRoot config = configProvider.GetAndValidateConfiguration();
            Dictionary<string, string> dictionary = config.Data.ToDictionary();
            Assert.AreEqual("V1", dictionary[string.Format("{0}.N1", TestComponentName)]);
        }

        [TestMethod]
        public void Config_should_always_come_from_cache_if_server_returns_invalid_data()
        {
            ConfigRoot configFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            configFromMockCache.Data.Children[0].Value = "fromcache";
            CreateMockDurableCacheEntry(configFromMockCache);

            _mockConfigServiceHttpClient = HttpClientTestHelper.CreateClientThatAlwaysReturnsGibberish();
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            IConfigProvider configProvider = testFactory.Create(TestComponentName, new Dictionary<string, Func<string, bool>>());
            ConfigRoot config = configProvider.GetAndValidateConfiguration();
            Dictionary<string, string> dictionary = config.Data.ToDictionary();
            Assert.AreEqual("fromcache", dictionary[string.Format("{0}.N1", TestComponentName)]);
        }

        [TestMethod]
        public void Config_validation_exception_contains_missing_values_when_server_config_is_invalid_and_cache_is_empty()
        {
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);
            var validators = new Dictionary<string, Func<string, bool>> { { "foo.N1", x => true }, { "foo.N2", x => true }, { "foo.N3", x => true } };

            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            try
            {
                ConfigRoot config = configProvider.GetAndValidateConfiguration();
                Assert.Fail("Invalid configuration passed validation");
            }
            catch (UnableToAccessConfigurationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigValidationException), "Expected a ConfigValidationException to be thrown");
                ConfigValidationException configValidationEx = e.InnerException as ConfigValidationException;
                Assert.IsTrue(Regex.IsMatch(configValidationEx.Message, "foo.N3"), "Missing invalid parameters from validation error message");
            }
        }

        [TestMethod]
        public void Config_validation_exception_contains_missing_values_when_server_and_cache_config_is_invalid()
        {
            ConfigRoot configFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            configFromMockCache.Data.Children[0].Value = "fromcache";
            configFromMockCache.Data.Children.Add(new ConfigNode() { Name = "N4", Value = "V4" });
            CreateMockDurableCacheEntry(configFromMockCache);

            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);
            var validators = new Dictionary<string, Func<string, bool>> { { "foo.N1", x => true }, { "foo.N2", x => true }, { "foo.N3", x => true }, { "foo.N4", x => true } };

            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            try
            {
                ConfigRoot config = configProvider.GetAndValidateConfiguration();
                Assert.Fail("Invalid configuration passed validation");
            }
            catch (UnableToAccessConfigurationException e)
            {
                Assert.IsInstanceOfType(e.InnerException, typeof(ConfigValidationException), "Expected a ConfigValidationException to be thrown");
                ConfigValidationException configValidationEx = e.InnerException as ConfigValidationException;
                Assert.IsTrue(Regex.IsMatch(configValidationEx.Message, "foo.N3, foo.N4"), "Missing invalid parameters from validation error message");
            }
        }

        [TestMethod]
        public void Config_should_always_come_from_cache_if_server_is_down()
        {
            ConfigRoot configFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            configFromMockCache.Data.Children[0].Value = "fromcache";
            CreateMockDurableCacheEntry(configFromMockCache);

            _mockConfigServiceHttpClient = HttpClientTestHelper.CreateClientSimulatingABadAddress();
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            IConfigProvider configProvider = testFactory.Create(TestComponentName, new Dictionary<string, Func<string, bool>>());
            ConfigRoot config = configProvider.GetAndValidateConfiguration();
            Dictionary<string, string> dictionary = config.Data.ToDictionary();
            Assert.AreEqual("fromcache", dictionary[string.Format("{0}.N1", TestComponentName)]);
        }

        [TestMethod]
        [ExpectedException(typeof(UnableToAccessConfigurationException))]
        public void Config_data_in_cache_should_always_be_validated()
        {
            ConfigRoot configFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            configFromMockCache.Data.Children[0].Value = "fromcache";
            CreateMockDurableCacheEntry(configFromMockCache);

            // In this case, we have to bypass validation of the service data because the service is offline
            _mockConfigServiceHttpClient = HttpClientTestHelper.CreateClientSimulatingABadAddress();
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            var validators = new Dictionary<string, Func<string, bool>> { { "N1.N1", x => false }, { "N1.N2", x => false } };
            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            configProvider.GetAndValidateConfiguration();
        }

        [TestMethod]
        public void Cache_should_always_be_updated_when_valid_data_is_retrieved_from_server()
        {
            ConfigRoot originalConfigFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            originalConfigFromMockCache.Data.Children[0].Value = "fromcache";
            CreateMockDurableCacheEntry(originalConfigFromMockCache);

            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            IConfigProvider configProvider = testFactory.Create(TestComponentName, new Dictionary<string, Func<string, bool>>());
            ConfigRoot configFromProvider = configProvider.GetAndValidateConfiguration();

            ConfigRoot updatedConfigFromMockCache = ReadMockDurableCacheEntry();

            Assert.IsFalse(_objectComparer.Compare(originalConfigFromMockCache, updatedConfigFromMockCache));
            Assert.IsTrue(_objectComparer.Compare(configFromProvider, updatedConfigFromMockCache));
        }

        [TestMethod]
        public void Cache_should_never_be_updated_when_invalid_data_is_retrieved_from_server()
        {
            ConfigRoot originalConfigFromMockCache = MockConfigService.GetConfigRoot(TestComponentName);
            originalConfigFromMockCache.Data.Children[0].Value = "fromcache";
            CreateMockDurableCacheEntry(originalConfigFromMockCache);

            _mockConfigServiceHttpClient = HttpClientTestHelper.CreateClientThatAlwaysReturnsGibberish();
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            IConfigProvider configProvider = testFactory.Create(TestComponentName, new Dictionary<string, Func<string, bool>>());
            ConfigRoot configFromProvider = configProvider.GetAndValidateConfiguration();

            ConfigRoot updatedConfigFromMockCache = ReadMockDurableCacheEntry();

            Assert.IsTrue(_objectComparer.Compare(originalConfigFromMockCache, updatedConfigFromMockCache));
            Assert.IsTrue(_objectComparer.Compare(configFromProvider, originalConfigFromMockCache));
        }

        [TestMethod]
        [ExpectedException(typeof(UnableToAccessConfigurationException))]
        public void Missing_config_settings_should_cause_config_validation_exception_to_be_thrown()
        {
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);
            var validators = new Dictionary<string, Func<string, bool>> { { "Taco", x => true } };
            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            configProvider.GetAndValidateConfiguration();
        }

        [TestMethod]
        [ExpectedException(typeof(UnableToAccessConfigurationException))]
        public void If_server_is_down_and_cache_is_empty_a_config_access_exception_should_be_thrown()
        {
            _mockConfigServiceHttpClient = HttpClientTestHelper.CreateClientSimulatingABadAddress();
            var testFactory = new ConfigProviderFactory(TestAssemblyName, _mockConfigServiceHttpClient, _mockFileSystem.Object);

            // This validator would cause an error, but the simulated connection issue above should prevent it from hitting that.
            var validators = new Dictionary<string, Func<string, bool>> { { "Taco", x => true } };
            IConfigProvider configProvider = testFactory.Create(TestComponentName, validators);
            configProvider.GetAndValidateConfiguration();
        }

        private ConfigRoot ReadMockDurableCacheEntry()
        {
            return DurableMemoryRepositoryHelper.ReadMockDurableCacheEntry<ConfigRoot>(_mockFileSystem, 1, TestPath);
        }

        private void CreateMockDurableCacheEntry(ConfigRoot configFromMockCache)
        {
            DurableMemoryRepositoryHelper.CreateMockDurableCacheEntry(configFromMockCache, _mockFileSystem, 1, TestPath);
        }
    }
}
