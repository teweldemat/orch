namespace orch.core.job
{
    public enum JobProcessType
    {
        Concurrent,
        Singleton
    }
    public class JobTypeInfo
    {
        public Guid TypeId { get; set; }
        public string Key { get; set; }
        public string TypeName { get; set; }
        public JobProcessType ProcessType { get; set; }
        public Type Type { get; set; }
        public string[] Permissions { get; set; }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BackgroundJobAttribute : Attribute
    {
        internal JobTypeInfo TypeInfo { get; set; }
        public Type Handler { get; set; }

        public BackgroundJobAttribute(
           string typeId,
           string key,
           string typeName,
           JobProcessType processType,
           Type handler = null,
           string[] permissions = null)
        {
            TypeInfo = new JobTypeInfo();

            if (!Guid.TryParse(typeId, out Guid parsedTypeId))
            {
                throw new InvalidOperationException($"Failed to parse '{typeId}' as a GUID for the job type '{typeName}'. Please ensure that the provided typeId is a valid GUID.");
            }

            TypeInfo.TypeId = parsedTypeId;
            TypeInfo.TypeName = typeName;
            TypeInfo.Key = key;
            TypeInfo.ProcessType = processType;
            TypeInfo.Permissions = permissions ?? Array.Empty<string>();


            if (handler != null && !typeof(IJobHandler).IsAssignableFrom(handler))
            {
                throw new ArgumentException($"The provided handler type, {handler.Name}, does not implement IJobHandler.", nameof(handler));
            }

            Handler = handler;
        }
    }
}
