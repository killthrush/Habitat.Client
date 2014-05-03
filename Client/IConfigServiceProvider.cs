namespace Habitat.Client
{
    /// <summary>
    /// Defines the operations needed to obtain raw data from a Config Service instance
    /// </summary>
    internal interface IConfigServiceProvider
    {
        /// <summary>
        /// Defines an operation which can make a call to retrieve config data from the Config Service.
        /// This call should block the executing thread until a response is given.
        /// </summary>
        /// <returns>The response from the Config Service</returns>
        ConfigServiceResponse GetConfig();
    }
}
