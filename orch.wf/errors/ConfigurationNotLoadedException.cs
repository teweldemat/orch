using System.Runtime.Serialization;

namespace orch.wf.errors
{
    [Serializable]
    public class ConfigurationNotLoadedException : InvalidOperationException, ISerializable
    {
        public ConfigurationNotLoadedException(Type type)
            : base($"Configuration for {type} not loaded")
        {
        }



        [Obsolete]



        protected ConfigurationNotLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }



        [Obsolete]



        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
