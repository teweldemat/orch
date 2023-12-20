using System.Runtime.Serialization;

namespace orch.core.errors
{
    [Serializable]
    public class JobTypeIdNotFoundException : InvalidOperationException
    {
        public JobTypeIdNotFoundException(Guid typeId)
            : base($"'{typeId}' is not a valid Job Type Id.")
        {
        }

        protected JobTypeIdNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
