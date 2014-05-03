using System;
using System.Collections.Generic;

namespace Habitat.Client
{
    /// <summary>
    /// Defines a factory implementation that knows how to create IConfigProvider instances.
    /// </summary>
    public interface IConfigProviderFactory
    {
        /// <summary>
        /// Defines an operation to create a Config Provider instance
        /// </summary>
        /// <param name="componentName">The name of the component that this Provider will manage</param>
        /// <param name="validationHandlers">A collection of functions that will validate config data for use with the specified Component</param>
        /// <returns>A new instance of the Config Provider</returns>
        IConfigProvider Create(string componentName, Dictionary<string, Func<string, bool>> validationHandlers);
    }
}
