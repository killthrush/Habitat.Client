using System;
using System.Net;
using ProTeck.Config.Dto.V1;

namespace Habitat.Client
{
    /// <summary>
    /// Simple wrapper for data returned from Config service.
    /// </summary>
    internal class ConfigServiceResponse
    {
        /// <summary>
        /// The HTTP status code returned if a connection was established
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The content of the service response, if there was no error
        /// </summary>
        public ConfigRoot Config { get; set; }

        /// <summary>
        /// If there was a problem with the request or response, then the exception will be attached here.
        /// </summary>
        public Exception Exception { get; set; }
    }
}