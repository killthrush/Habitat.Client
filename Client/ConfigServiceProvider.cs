using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Habitat.Core;
using Newtonsoft.Json;

namespace Habitat.Client
{
    /// <summary>
    /// Concrete class that provides access to the Habitat Server over HTTP
    /// </summary>
    internal class ConfigServiceProvider : IConfigServiceProvider
    {
        /// <summary>
        /// The name of the component for which config is being read
        /// </summary>
        private readonly string _componentName;

        /// <summary>
        /// HttpClient implementation used to communicate with the Habitat Server
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="componentName">The name of the component for which config is being read</param>
        /// <param name="client">HttpClient implementation used to communicate with the Habitat Server</param>
        public ConfigServiceProvider(string componentName, HttpClient client)
        {
            _componentName = componentName;
            _client = client;
        }

        /// <summary>
        /// Makes a call to retrieve config data from the Habitat Server.
        /// This call blocks the executing thread until a response is given.
        /// </summary>
        /// <returns>The response from the Habitat Server</returns>
        public ConfigServiceResponse GetConfig()
        {
            var serviceResponse = new ConfigServiceResponse
            {
                Config = new ConfigRoot { ComponentName = _componentName }
            };

            var serviceCallTask = _client.GetAsync(string.Format("Config/{0}", _componentName));
            var handleResponseTask = serviceCallTask.ContinueWith<ConfigServiceResponse>(HandleConfigServiceResponse);
            handleResponseTask.Wait();
            if (!handleResponseTask.IsFaulted)
            {
                serviceResponse = handleResponseTask.Result;    
            }
            
            return serviceResponse;
        }

        /// <summary>
        /// Helper method to handle all responses from Habitat Server (including errors) in a consistent manner
        /// </summary>
        /// <param name="task">The response task from the HttpClient</param>
        private ConfigServiceResponse HandleConfigServiceResponse(Task<HttpResponseMessage> task)
        {
            var configResults = new ConfigServiceResponse
                                    {
                                        Config = new ConfigRoot { ComponentName = _componentName }
                                    };

            try
            {
                HttpResponseMessage response = task.Result;
                configResults.StatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode != HttpStatusCode.NoContent)
                    {
                        var dataReadTask = response.Content.ReadAsStringAsync().ContinueWith(x => ReadConfigServiceJson(x));
                        dataReadTask.Wait();
                        configResults.Config = dataReadTask.Result.Config;
                    }
                }
                else
                {
                    configResults.Exception = new UnableToAccessConfigurationException(string.Format("Could not retrieve config.  Service returned status code {0}", response.StatusCode));
                }
            }
            catch (Exception exception)
            {
                configResults.Exception = new UnableToAccessConfigurationException(string.Format("Could not retrieve config.  Error in web request."), exception);
            }
            return configResults;
        }

        /// <summary>
        /// Helper method to parse server responses as the appropriate JSON type and format for readability.
        /// </summary>
        /// <typeparam name="T">The expected type of JSON being returned (e.g. array)</typeparam>
        /// <param name="readTask">The Habitat Server response task that provides the JSON values</param>
        private ConfigServiceResponse ReadConfigServiceJson<T>(Task<T> readTask)
            where T : class
        {
            ConfigServiceResponse configResults = new ConfigServiceResponse();
            configResults.Config = JsonConvert.DeserializeObject<ConfigRoot>(readTask.Result.ToString());
            return configResults;
        }
    }
}
