using Habitat.Core;

namespace Habitat.Client
{
    /// <summary>
    /// Defines the operations needed to obtain structure (but generic) config data and deliver it for use in an Application.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Obtains config data and ensures that each item is unique and has an appropriate format for the application
        /// </summary>
        /// <returns>The root of a tree of config data nodes</returns>
        ConfigRoot GetAndValidateConfiguration();
    }
}
