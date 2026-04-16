using System.Runtime.Serialization;

namespace orch.wf.errors
{
    [Serializable]
    public class TaskNotFoundException : ArgumentException
    {
        public TaskNotFoundException(Guid taskId)
            : base($"Task with Id '{taskId}' not found")
        {
        }



        [Obsolete]



        protected TaskNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
