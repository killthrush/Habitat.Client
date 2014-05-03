using System;
using System.Collections.Generic;
using System.Linq;
using Habitat.Core;

namespace Habitat.Client
{
    /// <summary>
    /// Core class that knows how to connect to a Habitat Server, read its stored values,
    /// and cache the responses in case a connection to the server is unavailable.
    /// </summary>
    internal class ConfigProvider : IConfigProvider
    {
        /// <summary>
        /// The base name of the component (e.g. service, application) to which this configuration provider applies.
        /// </summary>
        private readonly string _componentName;

        /// <summary>
        /// The set of all validation methods that will be run for each unique config value
        /// </summary>
        private readonly Dictionary<string, Func<string, bool>> _validationHandlers;

        /// <summary>
        /// Instance of a class that will communicate with the Habitat Server
        /// </summary>
        private readonly IConfigServiceProvider _serviceProvider;

        /// <summary>
        /// A durable cache that will keep our last known good config data
        /// </summary>
        private readonly IRepository<IJsonEntity<ConfigRoot>> _cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigProvider"/> class.
        /// </summary>
        /// <param name="componentName">The name of the application this Provider is responsible for.</param>
        /// <param name="validationHandlers">The set of all validation methods that will be run for each unique config value</param>
        /// <param name="serviceProvider">The class that will handle the actual communication with the Habitat Server</param>
        /// <param name="cacheProvider">The class that will make sure that this Provider can store config data locally without hitting the Habitat Server constantly</param>
        internal ConfigProvider(string componentName, Dictionary<string, Func<string, bool>> validationHandlers, IConfigServiceProvider serviceProvider, IRepository<IJsonEntity<ConfigRoot>> cacheProvider)
        {
            _componentName = componentName;
            _validationHandlers = validationHandlers;
            _serviceProvider = serviceProvider;
            _cacheProvider = cacheProvider;
        }

        /// <summary>
        /// Loads the configuration root entity from the durable cache.
        /// </summary>
        /// <returns>The root node and its children, or null if it cannot be found.</returns>
        protected ConfigRoot LoadConfigFromCache()
        {
            IJsonEntity<ConfigRoot> configRoot = _cacheProvider.Entities.FirstOrDefault(y => y.Contents.ComponentName == _componentName);
            if (configRoot != null)
            {
                return configRoot.Contents;
            }
            return null;
        }

        /// <summary>
        /// Saves the configuration root entity to the durable cache.
        /// </summary>
        /// <param name="config">The node to store</param>
        protected void SaveConfigToCache(ConfigRoot config)
        {
            IJsonEntity<ConfigRoot> configEntity = _cacheProvider.Entities.FirstOrDefault(y => y.Contents.ComponentName == _componentName);
            if (configEntity == null)
            {
                configEntity = _cacheProvider.Create();
            }
            configEntity.Contents = config;
            _cacheProvider.Update(configEntity);
            _cacheProvider.Save();
        }

        /// <summary>
        /// Method to run through a subset of the available config setings and validate them based on 
        /// the set of configured validation handlers.
        /// </summary>
        /// <param name="configDictionary">A dictionary derived from the current set of config settings</param>
        /// <returns>A dictionary containing an entry for each expected config setting paired with an indicator that says whether the setting is valid</returns>
        protected Dictionary<string, bool> Validate(IDictionary<string, string> configDictionary)
        {
            var validationResults = new Dictionary<string, bool>();
            foreach (var validationHandler in _validationHandlers)
            {
                if (configDictionary.ContainsKey(validationHandler.Key))
                {
                    var val = configDictionary[validationHandler.Key];
                    bool test;
                    try
                    {
                        test = validationHandler.Value(val);
                    }
                    catch
                    {
                        test = false;
                    }
                    validationResults.Add(validationHandler.Key, test);
                }
                else
                {
                    validationResults.Add(validationHandler.Key, false);
                }
            }
            return validationResults;
        }


        /// <summary>
        /// Obtains a copy of the current configuration settings, and ensures they are valid.
        /// </summary>
        /// <returns>The config settings, as a tree</returns>
        /// <exception cref="ConfigValidationException">if one or more expected configuration settings are invalid or missing</exception>
        public ConfigRoot GetAndValidateConfiguration()
        {
            ConfigRoot configRoot;

            try
            {
                ConfigServiceResponse responseFromService = _serviceProvider.GetConfig();

                bool hasValidConfigFromServer = false;
                string[] errors = new string[0];
                if (responseFromService.Config != null && responseFromService.Config.Data != null)
                {
                    Dictionary<string, bool> serverConfigValidationResults = Validate(responseFromService.Config.Data.ToDictionary());
                    hasValidConfigFromServer = (serverConfigValidationResults.Count(x => x.Value == false) == 0);
                    if (hasValidConfigFromServer)
                    {
                        SaveConfigToCache(responseFromService.Config);
                    }
                    else
                    {
                        errors = (from r in serverConfigValidationResults where r.Value == false select r.Key).ToArray();
                    }
                }

                bool hasValidConfigFromCache = false;
                configRoot = LoadConfigFromCache();
                if (configRoot != null)
                {
                    Dictionary<string, bool> cachedConfigValidationResults = Validate(configRoot.Data.ToDictionary());
                    hasValidConfigFromCache = (cachedConfigValidationResults.Count(x => x.Value == false) == 0);

                    // Only report the cache errors if the server data was missing or valid
                    if (!hasValidConfigFromCache && errors.Length == 0)
                    {
                        errors = (from r in cachedConfigValidationResults where r.Value == false select r.Key).ToArray();
                    }
                }

                if (!hasValidConfigFromCache && !hasValidConfigFromServer)
                {
                    if (responseFromService.Exception != null)
                    {
                        throw responseFromService.Exception;
                    }

                    throw new ConfigValidationException(string.Format("One or more config settings invalid or missing ({0}).", string.Join(", ", errors)), errors.ToList());
                }
            }
            catch (Exception e)
            {
                throw new UnableToAccessConfigurationException(string.Format("Config can not be retrieved for application '{0}'.", _componentName), e);
            }

            return configRoot;
        }
    }
}