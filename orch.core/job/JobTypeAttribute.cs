namespace orch.core.job
{
    public enum JobProcessType
    {
        Concurrent,
        Singleton,
    }
    public class JobTypeInfo
    {
        public Guid TypeId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public JobProcessType ProcessType { get; set; }
        public Type Type { get; set; } = null!;
        public string[] Permissions { get; set; } = Array.Empty<string>(); 
    }

    
    [AttributeUsage(AttributeTargets.Class)]
    public class BackgroundJobAttribute : Attribute
    {
        internal JobTypeInfo TypeInfo { get; set; }
        public Type? Handler { get; set; }

        public BackgroundJobAttribute(
            string typeId,
            string key,
            string typeName,
            JobProcessType processType,
            Type? handler = null,
            string[]? permissions = null)
        {
            if (!Guid.TryParse(typeId, out var parsedTypeId))
            {
                throw new ArgumentException($"Failed to parse '{typeId}' as a GUID for the job '{key}'.");
            }

            TypeInfo = new JobTypeInfo
            {
                TypeId = parsedTypeId,
                TypeName = typeName,
                Key = key,
                ProcessType = processType
            };

            if (permissions is not null)
            {
                TypeInfo.Permissions = permissions;
            }

            if (handler != null && !typeof(IJobHandler).IsAssignableFrom(handler))
                throw new ArgumentException($"The provided handler type, {handler.Name}, does not implement IJobHandler.", nameof(handler));

            Handler = handler;
        }
    }
}
