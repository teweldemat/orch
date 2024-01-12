using orch.common;

namespace orch.core.model
{
    public class EventLogProps
    {
        public enum LogLevel
        {
            Trace,
            Debug,
            Information,
            Warning,
            Error,
            Critical
        }

        public Guid Id { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid? CommandId { get; set; }
        public string? JobId { get; set; }
        public string Message { get; set; }
        public long Time { get; set; }
        public LogLevel Level { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? Data { get; set; }
    }

    public class EventLog : EventLogProps
    {
        public EventLog() { }
        public EventLog(EventLogProps props)
        {
            this.MapFromBase(props);
        }
    }
}
