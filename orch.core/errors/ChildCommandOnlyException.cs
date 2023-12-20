using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class ChildCommandOnlyException : InvalidOperationException, ISerializable
    {
        public ChildCommandOnlyException(string commandType)
            : base($"'{commandType}' should only be run as a child command.")
        {
        }

        protected ChildCommandOnlyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);
        }
    }
}
