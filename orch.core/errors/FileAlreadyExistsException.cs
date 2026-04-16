using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class FileAlreadyExistsException : Exception
    {
        public FileAlreadyExistsException()
        {
        }

        public FileAlreadyExistsException(string message) : base(message)
        {
        }

        public FileAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }



        [Obsolete]



        protected FileAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
