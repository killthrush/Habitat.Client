using System;
using System.Collections.Generic;
using System.Text;

namespace Habitat.Client
{
    /// <summary>
    /// Exception containing list of configuration settings that were either missing or invalid.
    /// </summary>
    public class ConfigValidationException : Exception
    {
        /// <summary>
        /// A list of config fields that failed validation
        /// </summary>
        private readonly List<string> _validationErrors;

        /// <summary>
        /// A list of config fields that failed validation
        /// </summary>
        public List<string> ValidationErrors
        {
            get { return _validationErrors; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="validationErrors">A list of config fields that failed validation</param>
        public ConfigValidationException(string message, List<string> validationErrors) : base(message)
        {
            _validationErrors = validationErrors;
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(base.ToString());
            builder.AppendLine("Config variables failing validation (ConfigValidationException.ValidationErrors): ");
            foreach (var validationError in ValidationErrors)
            {
                builder.AppendLine(validationError);
            }
            return builder.ToString();
        }
    }
}