using orch.common;
using orch.core.model;

namespace orch.core.logging
{
    public interface IEventLogDatabase
    {
        EventLog? GetById(Guid id);
        PagedList<EventLog> GetAll(int index, int count);
        PagedList<EventLog> GetByLevel(EventLogProps.LogLevel level, int index, int count);
        PagedList<EventLog> GetByTransactionId(Guid transactionId, int index, int count);
        PagedList<EventLog> GetByCommandId(Guid commandId, int index, int count);
        PagedList<EventLog> GetByJobId(string jobId, int index, int count);
        PagedList<EventLog> GetByReference(string reference, int index, int count);
        void Add(EventLog log);
        void Update(EventLog log);
        void Delete(Guid id);
    }
}
