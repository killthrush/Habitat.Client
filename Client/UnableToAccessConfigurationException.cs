using System;
using System.Text;

namespace Habitat.Client
{
    /// <summary>
    /// Exception thrown in the event that no valid configuration could be loaded,
    /// either from Config Service of from the cache.  This is generally going to be a fatal error.  Without configuration, an application can't function.
    /// </summary>
    public class UnableToAccessConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToAccessConfigurationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public UnableToAccessConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToAccessConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public UnableToAccessConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
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
            if (InnerException != null)
            {
                builder.AppendLine(string.Format("Inner Exception ({0}): ", InnerException.GetType()));
                builder.AppendLine(InnerException.ToString());
            }
            return builder.ToString();
        }
    }
}