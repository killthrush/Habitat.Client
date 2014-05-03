using System.Collections.Generic;

namespace Habitat.Client.Tests.TestClasses
{
    /// <summary>
    /// Dummy Implementation of ApplicationConfigProviderBase
    /// </summary>
    internal class SampleApplicationConfigProvider : ApplicationConfigProviderBase<SampleApplicationConfig>
    {
        /// <summary>
        /// The base name of the component (e.g. service, application) to which this configuration applies.
        /// </summary>
        public override string ApplicationComponentName
        {
            get { return "ProTeck.Config.Client.Test"; }
        }

        /// <summary>
        /// Initializes a new instance of the SampleApplicationConfigProvider class.
        /// </summary>
        /// <param name="configProviderFactory">Factory method that can create a Config Provider instance</param>
        public SampleApplicationConfigProvider(IConfigProviderFactory configProviderFactory) : base(configProviderFactory)
        {
            AddEnvironmentMapping("RestUrl", IsValidUrl);
            AddEnvironmentMapping("ConnectionString", Exists);
            AddApplicationMapping("TimeOut", IsValidInteger);
            AddApplicationMapping("ConfigObject.Name", IsADeliciousTaco);
            AddApplicationMapping("ConfigObject.Number", IsValidInteger);
        }

        /// <summary>
        /// Converts a tree of name-value configuration settings into a strongly-typed application object.
        /// </summary>
        /// <param name="applicationConfig">Config details for the current application component in flattened (dictionary) format</param>
        /// <param name="environmentConfig">Config details for the ambient environment in flattened (dictionary) format</param>
        /// <returns>Configuration object</returns>
        protected override SampleApplicationConfig CreateTypedConfig(Dictionary<string, string> applicationConfig, Dictionary<string, string> environmentConfig)
        {
            return new SampleApplicationConfig
            {
                ConnectionString = environmentConfig["ConnectionString"],
                RestUrl = environmentConfig["RestUrl"],
                Timeout = int.Parse(applicationConfig["TimeOut"]),
                ConfigObject = new CompositeConfigObject
                {
                    Name = applicationConfig["ConfigObject.Name"],
                    Number = int.Parse(applicationConfig["ConfigObject.Number"])
                }
            };
        }

        /// <summary>
        /// Sample of a custom config validation routine
        /// </summary>
        /// <param name="configObject">The value to check</param>
        /// <returns>True if the config value contains "taco"</returns>
        private static bool IsADeliciousTaco(string configObject)
        {
            if (configObject != null)
            {
                return configObject.ToUpper() == "TACO";
            }
            return false;
        }
    }
}
