using NCrontab;

namespace orch.core.job
{
    public enum JobProcessType
    {
        Concurrent,
        Singleton,
        Recurring
    }
    public class JobTypeInfo
    {
        public Guid TypeId { get; set; }
        public string Key { get; set; }
        public string TypeName { get; set; }
        public JobProcessType ProcessType { get; set; }
        public Type Type { get; set; }
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public string Cron { get; set; } = string.Empty;

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
            string[] permissions = null,
            string? cron = null)
        {
            TypeInfo = new JobTypeInfo();

            if (!Guid.TryParse(typeId, out Guid parsedTypeId))
            {
                throw new ArgumentException($"Failed to parse '{typeId}' as a GUID for the job type '{typeName}'. Please ensure that the provided typeId is a valid GUID.");
            }

            TypeInfo.TypeId = parsedTypeId;
            TypeInfo.TypeName = typeName;
            TypeInfo.Key = key;
            TypeInfo.ProcessType = processType;

            if (permissions is not null)
            {
                TypeInfo.Permissions = permissions;
            }

            if (processType is JobProcessType.Recurring)
            {
                if (TypeInfo.Permissions.Any())
                    throw new ArgumentException($"Permissions should not be provided for recurring job '{TypeInfo.Key}'.");

                if (string.IsNullOrEmpty(cron))
                    throw new ArgumentException($"Cron expression should be provided for recurring job '{TypeInfo.Key}'.");

                _ = CrontabSchedule.TryParse(cron) ?? throw new ArgumentException($"Invalid cron expression '{cron}' provided for recurring job '{TypeInfo.Key}'.");

                TypeInfo.Cron = cron;
            }
            else
            {
                if (!string.IsNullOrEmpty(cron))
                    throw new ArgumentException($"Cron expression should not be provided for non-recurring job '{TypeInfo.Key}'.");
            }

            if (handler != null && !typeof(IJobHandler).IsAssignableFrom(handler))
            {
                throw new ArgumentException($"The provided handler type, {handler.Name}, does not implement IJobHandler.", nameof(handler));
            }

            Handler = handler;
        }
    }
}
