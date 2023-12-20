using Microsoft.Extensions.DependencyInjection;

namespace orch.core.tenant
{
    public class MultiTenantServiceProvider
    {
        private readonly ServiceCollection _serviceCollection = new();

        private IServiceProvider? _serviceProvider;

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
    }

}