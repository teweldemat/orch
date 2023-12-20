using Microsoft.Extensions.DependencyInjection;

namespace orch.common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsInterfaceAndSelf<TService, TImplementation>(
           this IServiceCollection services,
           ServiceLifetime lifetime = ServiceLifetime.Scoped)
           where TService : class
           where TImplementation : class, TService
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TService, TImplementation>();
                    services.AddSingleton<TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<TService, TImplementation>();
                    services.AddTransient<TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<TService, TImplementation>();
                    services.AddScoped<TImplementation>();
                    break;
            }

            return services;
        }
    }
}
