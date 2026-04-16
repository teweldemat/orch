using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class InvalidInitializerException : InvalidOperationException, ISerializable
    {
        public InvalidInitializerException(string message) : base(message) { }



        [Obsolete]



        protected InvalidInitializerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }



        [Obsolete]



        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
