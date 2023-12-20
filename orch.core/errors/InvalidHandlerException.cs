using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class InvalidHandlerException : InvalidOperationException, ISerializable
    {
        public InvalidHandlerException(string message) : base(message) { }

        protected InvalidHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
