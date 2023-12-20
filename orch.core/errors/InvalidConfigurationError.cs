using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class InvalidConfigurationError : Exception
    {
        public InvalidConfigurationError()
        {
        }

        public InvalidConfigurationError(string message) : base(message)
        {
        }

        public InvalidConfigurationError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidConfigurationError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}