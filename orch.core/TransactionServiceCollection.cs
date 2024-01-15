using Microsoft.Extensions.DependencyInjection;

namespace orch.core
{
    public class TransactionServiceCollection : IDisposable
    {
        private bool _disposed = false;

        protected OTransactionService _tranService;
        public OTransactionService TranService => _tranService;
        public IOHost Host { get; }
        public ITransactionDatabase TranDb =>
            _tranService.Services.GetService<ITransactionDatabase>()
            ?? throw new NullReferenceException($"{nameof(ITransactionDatabase)} service couldn't be retrieved");

        public TransactionServiceCollection(IOHost host, OTransactionService tranService)
        {
            _tranService = tranService;
            Host = host;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _tranService?.Dispose();
            }

            _disposed = true;
        }

        ~TransactionServiceCollection()
        {
            Dispose(disposing: false);
        }
    }
}
