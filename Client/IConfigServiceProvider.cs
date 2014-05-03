namespace Habitat.Client
{
    /// <summary>
    /// Defines the operations needed to obtain raw data from a Habitat Server instance
    /// </summary>
    internal interface IConfigServiceProvider
    {
        /// <summary>
        /// Defines an operation which can make a call to retrieve config data from the Habitat Server.
        /// This call should block the executing thread until a response is given.
        /// </summary>
        /// <returns>The response from the Habitat Server</returns>
        ConfigServiceResponse GetConfig();
    }
}
