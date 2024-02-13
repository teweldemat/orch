using Microsoft.EntityFrameworkCore;
using orch.common;
using orch.core.ef.Logging.Entities;
using orch.core.ef.Transaction;
using orch.core.logging;
using orch.core.model;

namespace orch.core.ef.Logging
{
    [OView("event_logs")]
    public class EFEventLogDatabase : IEventLogDatabase
    {
        private readonly OTransactionDbContext _context;

        public EFEventLogDatabase(OTransactionDbContext context)
        {
            _context = context;
        }

        [OViewFunction]
        public EventLog? GetById(Guid id)
        {
            var dalEventLog = _context.EventLogs
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id);

            return dalEventLog is null ? null : new EventLog(dalEventLog);
        }

        [OViewFunction]
        public PagedList<EventLog> GetAll(int index, int count)
        {
            var query = _context.EventLogs.AsNoTracking();

            return GetPagedList(query, index, count);
        }

        [OViewFunction]
        public PagedList<EventLog> GetByLevel(EventLogProps.LogLevel level, int index, int count)
        {
            var query = _context.EventLogs
                .AsNoTracking()
                .Where(e => e.Level == level);

            return GetPagedList(query, index, count);
        }

        [OViewFunction]
        public PagedList<EventLog> GetByTransactionId(Guid transactionId, int index, int count)
        {
            var transactionExists = _context.Transactions.Any(t => t.Id == transactionId);
            if (!transactionExists)
            {
                throw new InvalidOperationException($"Transaction with ID '{transactionExists}' does not exist.");
            }

            var query = _context.EventLogs
                .AsNoTracking()
                .Where(e => e.TransactionId == transactionId);

            return GetPagedList(query, index, count);
        }

        [OViewFunction]
        public PagedList<EventLog> GetByCommandId(Guid commandId, int index, int count)
        {
            var commandExists = _context.Commands.Any(c => c.Id == commandId);
            if (!commandExists)
            {
                throw new InvalidOperationException($"Command with ID '{commandId}' does not exist.");
            }

            var query = _context.EventLogs
                .AsNoTracking()
                .Where(e => e.CommandId == commandId);

            return GetPagedList(query, index, count);
        }

        [OViewFunction]
        public PagedList<EventLog> GetByJobId(string jobId, int index, int count)
        {
            var jobExists = _context.Jobs.Any(j => j.Id == jobId);
            if (!jobExists)
            {
                throw new InvalidOperationException($"Job with ID '{jobId}' does not exist.");
            }

            var query = _context.EventLogs
                .AsNoTracking()
                .Where(e => e.JobId == jobId);

            return GetPagedList(query, index, count);
        }

        [OViewFunction]
        public PagedList<EventLog> GetByReference(string reference, int index, int count)
        {
            var query = _context.EventLogs
                .AsNoTracking()
                .Where(e => e.Reference == reference);

            return GetPagedList(query, index, count);
        }

        public void Add(EventLog log)
        {
            var dalEventLog = new DALEventLog(log);
            _context.EventLogs.Add(dalEventLog);
            _context.SaveChanges();

            _context.Entry(dalEventLog).State = EntityState.Detached;
        }

        public void Update(EventLog log)
        {
            var dalEventLog = new DALEventLog(log);
            _context.EventLogs.Update(dalEventLog);
            _context.SaveChanges();

            _context.Entry(dalEventLog).State = EntityState.Detached;
        }

        public void Delete(Guid id)
        {
            var log = _context.EventLogs.Find(id)
                ?? throw new InvalidOperationException($"EventLog with ID {id} does not exist.");

            _context.EventLogs.Remove(log);
            _context.SaveChanges();

            _context.Entry(log).State = EntityState.Detached;
        }

        private static PagedList<EventLog> GetPagedList(IQueryable<DALEventLog> query, int index, int count)
        {
            query = query.OrderByDescending(e => e.Time);

            var total = query.Count();
            var items = query.Skip(index * count).Take(count).Select(e => new EventLog(e)).ToList();
            return new PagedList<EventLog>
            {
                List = items,
                Count = total
            };
        }
    }
}
