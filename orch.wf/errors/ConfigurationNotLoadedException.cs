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

        protected ConfigurationNotLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
