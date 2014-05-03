using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ProTeck.Config.Dto.V1;

namespace Habitat.Client
{
    /// <summary>
    /// Communicates with Habitat Server and provides access to strongly typed configuration object. 
    /// This is an abstract class.
    /// </summary>
    /// <typeparam name="T">The type of the strongly typed configuration package</typeparam>
    public abstract class ApplicationConfigProviderBase<T> where T : class, new()
    {
        /// <summary>
        /// The name of the "special" environment config component.  This is intended to be shared by all applications.
        /// </summary>
        private const string EnvironmentComponentName = "Environment";

        /// <summary>
        /// Factory method that can create a Config Provider instance
        /// </summary>
        private readonly IConfigProviderFactory _configProviderFactory;

        /// <summary>
        /// Used within IConfigProvider to ensure returned application config meets expectations.
        /// </summary>
        private readonly Dictionary<string, Func<string, bool>> _applicationValidationMappings = new Dictionary<string, Func<string, bool>>();

        /// <summary>
        /// Used within IConfigProvider to ensure returned environment config meets expectations.
        /// </summary>
        private readonly Dictionary<string, Func<string, bool>> _environmentValidationMappings = new Dictionary<string, Func<string, bool>>();

        /// <summary>
        /// The base name of the component (e.g. service, application) to which this configuration applies.
        /// </summary>
        public abstract string ApplicationComponentName { get; }

        /// <summary>
        /// Initializes a new instance of the ApplicationConfigProviderBase class.
        /// </summary>
        /// <param name="configProviderFactory">Factory method that can create a Config Provider instance</param>
        protected ApplicationConfigProviderBase(IConfigProviderFactory configProviderFactory = null)
        {
            _configProviderFactory = configProviderFactory;
        }

        /// <summary>
        /// When overridden in a descendant class, converts a tree of name-value configuration settings into a strongly-typed application object.
        /// </summary>
        /// <param name="applicationConfig">Config details for the current application component in flattened (dictionary) format</param>
        /// <param name="environmentConfig">Config details for the ambient environment in flattened (dictionary) format</param>
        /// <returns>Configuration object</returns>
        protected abstract T CreateTypedConfig(Dictionary<string, string> applicationConfig, Dictionary<string, string> environmentConfig);

        /// <summary>
        /// Adds a validation mapping for an application component config value
        /// </summary>
        /// <param name="configValueName">The name of the config value, e.g. X.Y.Z</param>
        /// <param name="validationMethod">The method that will be used to validate this value</param>
        protected void AddApplicationMapping(string configValueName, Func<string, bool> validationMethod)
        {
            AddMapping(ApplicationComponentName, configValueName, validationMethod, _applicationValidationMappings);
        }

        /// <summary>
        /// Adds a validation mapping for an environment component config value
        /// </summary>
        /// <param name="configValueName">The name of the config value, e.g. X.Y.Z</param>
        /// <param name="validationMethod">The method that will be used to validate this value</param>
        protected void AddEnvironmentMapping(string configValueName, Func<string, bool> validationMethod)
        {
            AddMapping(EnvironmentComponentName, configValueName, validationMethod, _environmentValidationMappings);
        }

        /// <summary>
        /// Generic method used to set up validation mappings
        /// </summary>
        /// <param name="componentName">The name of the component being mapped</param>
        /// <param name="configValueName">The name of the config value, e.g. X.Y.Z</param>
        /// <param name="validationMethod">The method that will be used to validate this value</param>
        /// <param name="mappingDictionary">The dictionary used to store the mapping</param>
        private static void AddMapping(string componentName, string configValueName, Func<string, bool> validationMethod, Dictionary<string, Func<string, bool>> mappingDictionary)
        {
            string fullConfigName = string.Format("{0}.{1}", componentName, configValueName);
            if (!mappingDictionary.ContainsKey(fullConfigName))
            {
                mappingDictionary[fullConfigName] = validationMethod;
            }
        }

        /// <summary>
        /// Uses CreateTypedConfig method to return strongly typed configuration object.
        /// </summary>
        /// <returns>A strongly typed configuration package</returns>
        /// <remarks>
        /// This method will always return the most recent configuration settings without the need to 
        /// restart the consuming application.  For best results, call this method often rather than calling once at startup.
        /// </remarks>
        public virtual T GetConfiguration()
        {
            string callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name;
            IConfigProviderFactory configProviderFactory = _configProviderFactory ?? new ConfigProviderFactory(callingAssemblyName);

            IConfigProvider applicationConfigProvider = configProviderFactory.Create(ApplicationComponentName, _applicationValidationMappings);
            ConfigRoot applicationConfig = applicationConfigProvider.GetAndValidateConfiguration();

            IConfigProvider environmentConfigProvider = configProviderFactory.Create(EnvironmentComponentName, _environmentValidationMappings);
            ConfigRoot environmentConfig = environmentConfigProvider.GetAndValidateConfiguration();

            // In order to keep the config names short and simple, we'll remove application/environment names as prefixes. The consuming application does not need this information.
            var originalApplicationConfigDictionary = applicationConfig.Data.ToDictionary();
            var originalEnvironmentConfigDictionary = environmentConfig.Data.ToDictionary();
            var modifiedApplicationConfigDictionary = originalApplicationConfigDictionary.ToDictionary(originalKey => RemoveComponentPrefix(originalKey.Key, string.Format("{0}.", ApplicationComponentName)), y => y.Value);
            var modifiedEnvironmentConfigDictionary = originalEnvironmentConfigDictionary.ToDictionary(originalKey => RemoveComponentPrefix(originalKey.Key, string.Format("{0}.", EnvironmentComponentName)), y => y.Value);

            return CreateTypedConfig(modifiedApplicationConfigDictionary, modifiedEnvironmentConfigDictionary);
        }

        /// <summary>
        /// Helper method to remove a specific prefix from the name of a config variable
        /// </summary>
        /// <param name="originalKey">The original key name of the config variable</param>
        /// <param name="componentName">The prefix to remove</param>
        /// <returns>The name with the prefix removed</returns>
        private string RemoveComponentPrefix(string originalKey, string componentName)
        {
            string namePrefix = string.Format("{0}.", componentName);
            int startIndex = originalKey.IndexOf(namePrefix, StringComparison.Ordinal);
            return originalKey.Substring(startIndex + namePrefix.Length);
        }

        /// <summary>
        /// Returns true if the supplied value is a boolean
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a boolean, else false</returns>
        /// <remarks>This method will not throw exceptions</remarks>
        protected internal static bool IsValidBoolean(object configValue)
        {
            bool b;
            var result = bool.TryParse(configValue.ToString(), out b);
            return result;
        }

        /// <summary>
        /// Returns true if the supplied value is an integer
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is an integer, else false</returns>
        /// <remarks>This method will not throw exceptions</remarks>
        protected internal static bool IsValidInteger(object configValue)
        {
            int i;
            var cv = configValue.ToString();
            var result = int.TryParse(cv, out i);
            return result;
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted URL
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted URL, else false</returns>
        /// <remarks>This will not check to see if the URL is reachable, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsValidUrl(object configValue)
        {
            try
            {
                new Uri(configValue.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted hostname
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted hostname, else false</returns>
        /// <remarks>This will not check to see if the address is reachable, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsValidHostname(object configValue)
        {
            try
            {
                bool isHostName = Regex.IsMatch(configValue.ToString(), @"\b((?=[a-z0-9-]{1,63}\.)[a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,63}\b", RegexOptions.IgnoreCase); // From regexbuddy library
                return isHostName;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted hostname or IP address
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted hostname or IP address, else false</returns>
        /// <remarks>This will not check to see if the address is reachable, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsValidHostnameOrIp(object configValue)
        {
            return IsValidHostname(configValue) || IsValidIpAddress(configValue);
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted IP address
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted IP address, else false</returns>
        /// <remarks>This will not check to see if the address is reachable, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsValidIpAddress(object configValue)
        {
            if (configValue != null)
            {
                UriHostNameType uriHostNameType = Uri.CheckHostName(configValue.ToString().Trim());
                return (uriHostNameType == UriHostNameType.IPv4);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted email address
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted email address, else false</returns>
        /// <remarks>This will not check to see if the address can receive mail, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsValidEmailAddress(object configValue)
        {
            try
            {
                new MailAddress(configValue.ToString().Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the supplied value represents a list of properly formatted email addresses
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <param name="delimiter">A value to use as a delimiter between email address values</param>
        /// <returns>True if the input is a properly formatted list of email addresses, else false</returns>
        /// <remarks>This will not check to see if the address can receive mail, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool AreAllValidEmailAddresses(object configValue, char delimiter)
        {
            var addresses = configValue.ToString();
            var addressList = addresses.Split(delimiter);
            return addressList.All(IsValidEmailAddress);
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted XML document fragment
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted XML document fragment, else false</returns>
        /// <remarks>This will not check XML validity, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsWellFormedXml(object configValue)
        {
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(configValue.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the supplied value is a properly formatted path
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted path, else false</returns>
        /// <remarks>This will not check to see if the path actually exists, only that it is formatted properly.  This method will not throw exceptions.</remarks>
        protected internal static bool IsWellFormedPath(object configValue)
        {
            try
            {
                return Path.IsPathRooted(configValue.ToString());
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Placeholder method that returns true and is used to make sure a required value is present
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>Always returns true</returns>
        protected internal static bool Exists(object configValue)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the supplied value evaluates to a valid .NET time interval
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted interval, else false</returns>
        protected internal static bool IsValidTimeInterval(object configValue)
        {
            TimeSpan interval;
            return configValue != null && TimeSpan.TryParse(configValue.ToString().Trim(), out interval);
        }

        /// <summary>
        /// Returns true if the supplied value evaluates to a valid Microsoft SQL Server connection string
        /// </summary>
        /// <param name="configValue">The value to check</param>
        /// <returns>True if the input is a properly formatted connection string, else false</returns>
        /// <remarks>
        /// Do not use this if you are connecting to something other than MS SQL Server.  For those cases, provide your own validation routine by deriving from this class.
        /// </remarks>
        public static bool IsValidSqlServerConnectionString(object configValue)
        {
            if (configValue == null)
            {
                return false;
            }

            string connectionString = configValue.ToString().Trim();
            if (string.IsNullOrWhiteSpace(connectionString)) // This check is necessary because for some reason an empty connection string is considered acceptable to Microsoft.
            {
                return false; 
            }

            try
            {   
                new SqlConnectionStringBuilder(connectionString);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

