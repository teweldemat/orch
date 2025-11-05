namespace orch.core.errors
{
    public class CommandFilterException : InvalidOperationException
    {
        public CommandFilterException()
        {
        }

        public CommandFilterException(string message) : base(message)
        {
        }

        public CommandFilterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
