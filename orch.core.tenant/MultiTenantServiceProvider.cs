using Microsoft.Extensions.DependencyInjection;

namespace orch.core.tenant
{
    public class MultiTenantServiceProvider : IDisposable
    {
        private readonly ServiceCollection _serviceCollection = new();
        private IServiceProvider? _serviceProvider;
        private bool disposed = false; // To detect redundant calls

        private IServiceProvider ServiceProvider
        {
            get
            {
                return _serviceProvider ??= _serviceCollection.BuildServiceProvider();
            }
        }

        public void RegisterServices(Action<ServiceCollection> registrationAction)
        {
            registrationAction?.Invoke(_serviceCollection);
        }

        public T? GetService<T>()
        {
            var service = ServiceProvider.GetService<T>();

            if (service == null)
            {
                throw new InvalidOperationException($"{typeof(T).Name} has not been registered.");
            }

            return service;
        }

        public OTransactionService GetOTransactionService()
        {
            var service = ServiceProvider.GetService<OTransactionService>();

            if (service == null)
            {
                throw new InvalidOperationException($"{nameof(OTransactionService)} has not been registered.");
            }

            return service;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && _serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                _serviceProvider = null;
                disposed = true;
            }
        }

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MultiTenantServiceProvider()
        // {
        //     // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //     Dispose(false);
        // }
    }
}
