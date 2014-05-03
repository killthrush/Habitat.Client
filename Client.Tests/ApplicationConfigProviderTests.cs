using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using Habitat.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Habitat.Client.Tests.TestClasses;

namespace Habitat.Client.Tests
{
    [TestClass]
    public class ApplicationConfigProviderTests
    {
        private static readonly DateTime TestDate = DateTime.Today;
        private SampleApplicationConfig _expectedConfig;
        private SampleApplicationConfigProvider _applicationConfigProvider;

        [TestInitialize]
        public void Setup()
        {
            _expectedConfig = CreateExpectedConfig();
            _applicationConfigProvider = new SampleApplicationConfigProvider(null);
        }

        [TestMethod]
        public void Missing_application_configuration_should_throw_typed_exception()
        {
            ConfigRoot originalApplicationConfig = CreateCannedApplicationConfig();
            ConfigRoot originalEnvironmentConfig = CreateCannedEnvironmentConfig();
            originalApplicationConfig.Data.Children.RemoveAt(0);

            Mock<IConfigProviderFactory> mockFactory = CreateFactory(originalApplicationConfig, originalEnvironmentConfig);

            var configProvider = new SampleApplicationConfigProvider(mockFactory.Object);

            try
            {
                configProvider.GetConfiguration();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof (UnableToAccessConfigurationException));
                Assert.IsNotNull(ex.InnerException);
                Assert.IsInstanceOfType(ex.InnerException, typeof (ConfigValidationException));
                var validationException = (ConfigValidationException) ex.InnerException;
                Assert.IsNotNull(validationException.ValidationErrors);
                Assert.AreEqual(1, validationException.ValidationErrors.Count);
                var error = validationException.ValidationErrors[0];
                Assert.AreEqual(_applicationConfigProvider.ApplicationComponentName + ".TimeOut", error);
                Assert.IsTrue(ex.ToString().Contains(_applicationConfigProvider.ApplicationComponentName + ".TimeOut"));
            }
        }

        [TestMethod]
        public void Missing_environment_configuration_should_throw_typed_exception()
        {
            ConfigRoot originalApplicationConfig = CreateCannedApplicationConfig();
            ConfigRoot originalEnvironmentConfig = CreateCannedEnvironmentConfig();
            originalEnvironmentConfig.Data.Children.RemoveAt(1);

            Mock<IConfigProviderFactory> mockFactory = CreateFactory(originalApplicationConfig, originalEnvironmentConfig);
            var configProvider = new SampleApplicationConfigProvider(mockFactory.Object);

            try
            {
                configProvider.GetConfiguration();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof (UnableToAccessConfigurationException));
                Assert.IsNotNull(ex.InnerException);
                Assert.IsInstanceOfType(ex.InnerException, typeof (ConfigValidationException));
                var validationException = (ConfigValidationException) ex.InnerException;
                Assert.IsNotNull(validationException.ValidationErrors);
                Assert.AreEqual(1, validationException.ValidationErrors.Count);
                var error = validationException.ValidationErrors[0];
                Assert.AreEqual("Environment.ConnectionString", error);
                Assert.IsTrue(ex.ToString().Contains("Environment.ConnectionString"));
            }
        }

        [TestMethod]
        public void Test_successful_conversion_of_generic_config_root_to_typed_object_graph()
        {
            ConfigRoot originalApplicationConfig = CreateCannedApplicationConfig();
            ConfigRoot originalEnvironmentConfig = CreateCannedEnvironmentConfig();

            Mock<IConfigProviderFactory> mockFactory = CreateFactory(originalApplicationConfig, originalEnvironmentConfig);
            var configProvider = new SampleApplicationConfigProvider(mockFactory.Object);

            SampleApplicationConfig actualConfig = configProvider.GetConfiguration();
            Assert.AreEqual(_expectedConfig.RestUrl, actualConfig.RestUrl);
            Assert.AreEqual(_expectedConfig.ConnectionString, actualConfig.ConnectionString);
            Assert.AreEqual(_expectedConfig.Timeout, actualConfig.Timeout);
            Assert.AreEqual(_expectedConfig.ConfigObject.Name, actualConfig.ConfigObject.Name);
            Assert.AreEqual(_expectedConfig.ConfigObject.Number, actualConfig.ConfigObject.Number);
        }

        /// <summary>
        /// Builds a factory that knows how to create a Config Provider that acts like the real thing, but doesn't use real caches or service calls.
        /// </summary>
        /// <param name="applicationConfig">The canned application configuration that the provider should use</param>
        /// <param name="environmentConfig">The canned environment configuration that the provider should use</param>
        /// <returns>A mock factory</returns>
        private Mock<IConfigProviderFactory> CreateFactory(ConfigRoot applicationConfig, ConfigRoot environmentConfig)
        {
            var mockConfigServiceProviderForApplication = new Mock<IConfigServiceProvider>(MockBehavior.Strict);
            var cannedApplicationResponse = new ConfigServiceResponse
                                                {
                                                    Config = applicationConfig,
                                                    StatusCode = HttpStatusCode.OK
                                                };
            mockConfigServiceProviderForApplication.Setup(x => x.GetConfig()).Returns(cannedApplicationResponse);
            var mockApplicationConfig = new Mock<IJsonEntity<ConfigRoot>>(MockBehavior.Strict);
            mockApplicationConfig.SetupProperty(x => x.Contents);
            mockApplicationConfig.SetupProperty(x => x.JsonData);
            mockApplicationConfig.Object.Contents = applicationConfig;


            var mockConfigServiceProviderForEnvironment = new Mock<IConfigServiceProvider>(MockBehavior.Strict);
            var cannedEnvironmentResponse = new ConfigServiceResponse
                                                {
                                                    Config = environmentConfig,
                                                    StatusCode = HttpStatusCode.OK
                                                };
            mockConfigServiceProviderForEnvironment.Setup(x => x.GetConfig()).Returns(cannedEnvironmentResponse);
            var mockEnvironmentConfig = new Mock<IJsonEntity<ConfigRoot>>(MockBehavior.Strict);
            mockEnvironmentConfig.SetupProperty(x => x.Contents);
            mockEnvironmentConfig.SetupProperty(x => x.JsonData);
            mockEnvironmentConfig.Object.Contents = environmentConfig;


            var mockFactory = new Mock<IConfigProviderFactory>(MockBehavior.Strict);

            var cacheProvider = new Mock<IRepository<IJsonEntity<ConfigRoot>>>(MockBehavior.Strict);
            cacheProvider.SetupAllProperties();
            cacheProvider.SetupGet(x => x.Entities).Returns(new[] {mockApplicationConfig.Object, mockEnvironmentConfig.Object}.AsQueryable());
            cacheProvider.Setup(x => x.Save());
            cacheProvider.Setup(x => x.Update(It.IsAny<IJsonEntity<ConfigRoot>>()));

            mockFactory.Setup(
                x => x.Create(_applicationConfigProvider.ApplicationComponentName, It.IsAny<Dictionary<string, Func<string, bool>>>()))
                .Returns<string, Dictionary<string, Func<string, bool>>>((name, validators) => { return new ConfigProvider(name, validators, mockConfigServiceProviderForApplication.Object, cacheProvider.Object); });

            mockFactory.Setup(
                x => x.Create("Environment", It.IsAny<Dictionary<string, Func<string, bool>>>()))
                .Returns<string, Dictionary<string, Func<string, bool>>>((name, validators) => { return new ConfigProvider(name, validators, mockConfigServiceProviderForEnvironment.Object, cacheProvider.Object); });


            return mockFactory;
        }

        private static SampleApplicationConfig CreateExpectedConfig()
        {
            var expectedConfig = new SampleApplicationConfig
                                     {
                                         RestUrl = "http://fake",
                                         ConnectionString = "This is a connection string.",
                                         Timeout = 500,
                                         ConfigObject = new CompositeConfigObject
                                                            {
                                                                Name = "Taco",
                                                                Number = 6
                                                            }
                                     };
            return expectedConfig;
        }


        private ConfigRoot CreateCannedEnvironmentConfig()
        {
            ConfigRoot config = new ConfigRoot { ComponentName = "Environment", LastModified = TestDate };
            config.Data = new ConfigNode
                              {
                                  Name = "Environment",
                                  Children = new List<ConfigNode>
                                                 {
                                                     new ConfigNode {Name = "RestUrl", Value = _expectedConfig.RestUrl},
                                                     new ConfigNode {Name = "ConnectionString", Value = _expectedConfig.ConnectionString},
                                                 }
                              };
            return config;
        }

        private ConfigRoot CreateCannedApplicationConfig()
        {
            ConfigRoot config = new ConfigRoot {ComponentName = _applicationConfigProvider.ApplicationComponentName, LastModified = TestDate};
            config.Data = new ConfigNode
                              {
                                  Name = _applicationConfigProvider.ApplicationComponentName,
                                  Children = new List<ConfigNode>
                                                 {
                                                     new ConfigNode {Name = "TimeOut", Value = _expectedConfig.Timeout.ToString()},
                                                     new ConfigNode
                                                         {
                                                             Name = "ConfigObject",
                                                             Children = new List<ConfigNode>
                                                                            {
                                                                                new ConfigNode {Name = "Name", Value = "Taco"},
                                                                                new ConfigNode {Name = "Number", Value = "6"}
                                                                            }
                                                         }
                                                 }
                              };
            return config;
        }
    }
}