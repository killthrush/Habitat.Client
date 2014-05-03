using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Habitat.Core;

namespace Habitat.Client
{
    /// <summary>
    /// Factory implementation that knows how to create and initialize ConfigProvider instances.
    /// </summary>
    internal class ConfigProviderFactory : IConfigProviderFactory
    {
        /// <summary>
        /// Default URL used to access the Habitat server.  It does not include the component name - this is included in the requests sent by ConfigServiceProvider.
        /// </summary>
        /// <remarks>
        /// This URL adopts a fixed convention, mainly because it is the basis for all other environment config settings. 
        /// This URL must resolve properly on all machines in the domain.
        /// </remarks>
        public const string DefaultServerUrl = "http://HabitatServer/Habitat.Server.Data/";

        /// <summary>
        /// HTTP client implementation that will be used internally by the ConfigProvider to talk to Habitat Server
        /// </summary>
        private readonly HttpClient _configServiceHttpClient;

        /// <summary>
        /// A wrapper for filesystem access.  This is used by the ConfigProvider to manage local storage of config data
        /// </summary>
        private readonly IFileSystemFacade _fileSystem;

        /// <summary>
        /// A string containing the assembly name of the application using the Factory.  Used for caching results from the server.
        /// </summary>
        private readonly string _applicationAssemblyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigProviderFactory"/> class.
        /// </summary>
        /// <param name="applicationAssemblyName">A string containing the assembly name of the application using the Factory</param>
        /// <param name="configServiceHttpClient">HTTP client implementation that will be used internally by the ConfigProvider to talk to Habitat Server</param>
        /// <param name="fileSystem">A wrapper for filesystem access.  This is used by the ConfigProvider to manage local storage of config data</param>
        internal ConfigProviderFactory(string applicationAssemblyName = null, HttpClient configServiceHttpClient = null, IFileSystemFacade fileSystem = null)
        {
            _applicationAssemblyName = applicationAssemblyName ?? GetType().Assembly.GetName().Name;
            _configServiceHttpClient = configServiceHttpClient;
            _fileSystem = fileSystem ?? new FileSystemFacade();
            if (_configServiceHttpClient == null)
            {
                _configServiceHttpClient = new HttpClient();
                _configServiceHttpClient.BaseAddress = new Uri(DefaultServerUrl);
            }
        }

        /// <summary>
        /// Creates a concrete Config Provider instance
        /// </summary>
        /// <param name="componentName">The name of the component that this Provider will manage</param>
        /// <param name="validationHandlers">A collection of functions that will validate config data for use with the specified Component</param>
        /// <returns>A new instance of the Config Provider</returns>
        public IConfigProvider Create(string componentName, Dictionary<string, Func<string, bool>> validationHandlers)
        {
            string repositoryDataDirectory = Path.Combine(_fileSystem.GetTempDirectoryPath(), _applicationAssemblyName);

            var configServiceProvider = new ConfigServiceProvider(componentName, _configServiceHttpClient);
            var cacheProvider = new DurableMemoryRepository<ConfigRoot>(repositoryDataDirectory, _fileSystem);
            var configProvider = new ConfigProvider(componentName, validationHandlers, configServiceProvider, cacheProvider);
            return configProvider;
        }
    }
}
