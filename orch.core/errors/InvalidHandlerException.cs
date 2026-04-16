using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class InvalidHandlerException : InvalidOperationException, ISerializable
    {
        public InvalidHandlerException(string message) : base(message) { }



        [Obsolete]



        protected InvalidHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }



        [Obsolete]



        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
