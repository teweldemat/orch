using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace orch.core.tenant
{
    public interface ITenantConnectionStringProvider
    {
        string GetConnectionString();
    }

    public static class MultiTenantNpgsql
    {
        public static IServiceCollection AddMultiTenantNpgsqlDataSource(
        this IServiceCollection serviceCollection,
        ITenantConnectionStringProvider connectionStringProvider,
        Action<NpgsqlDataSourceBuilder> dataSourceBuilderAction,
        ServiceLifetime connectionLifetime = ServiceLifetime.Scoped,
        ServiceLifetime dataSourceLifetime = ServiceLifetime.Singleton)
        {
            serviceCollection.TryAddScoped(provider => connectionStringProvider);

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(NpgsqlDataSource),
                    sp =>
                    {
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStringProvider.GetConnectionString());
                        dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                        dataSourceBuilderAction?.Invoke(dataSourceBuilder);
                        return dataSourceBuilder.Build();
                    },
                    dataSourceLifetime));

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(NpgsqlMultiHostDataSource),
                    sp =>
                    {
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStringProvider.GetConnectionString());
                        dataSourceBuilder.UseLoggerFactory(sp.GetService<ILoggerFactory>());
                        dataSourceBuilderAction?.Invoke(dataSourceBuilder);
                        return dataSourceBuilder.BuildMultiHost();
                    },
                    dataSourceLifetime));

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(NpgsqlDataSource),
                    sp => sp.GetRequiredService<NpgsqlMultiHostDataSource>(),
                    dataSourceLifetime));

            AddCommonServices(serviceCollection, connectionLifetime, dataSourceLifetime);

            return serviceCollection;
        }

        static void AddCommonServices(
       IServiceCollection serviceCollection,
       ServiceLifetime connectionLifetime,
       ServiceLifetime dataSourceLifetime)
        {
            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(NpgsqlConnection),
                    sp => sp.GetRequiredService<NpgsqlDataSource>().CreateConnection(),
                    connectionLifetime));

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(DbDataSource),
                    sp => sp.GetRequiredService<NpgsqlDataSource>(),
                    dataSourceLifetime));

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(DbConnection),
                    sp => sp.GetRequiredService<NpgsqlConnection>(),
                    connectionLifetime));
        }
    }
}
