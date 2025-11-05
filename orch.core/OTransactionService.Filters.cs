using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using orch.core.errors;
using orch.core.model;

namespace orch.core
{
    public partial class OTransactionService
    {
        private class CommandFilterInfo
        {
            public Type FilterType { get; init; }
            public Type[] ConstructorParameters { get; init; }
        }

        private static readonly Dictionary<Guid, List<CommandFilterInfo>> s_commandFilters = new();
        private static readonly object s_commandFilterLock = new();

        public static void RegisterCommandFilter(Guid commandTypeId, Type filterType)
        {
            if (filterType == null)
            {
                throw new ArgumentNullException(nameof(filterType));
            }

            if (!typeof(ICommandFilter).IsAssignableFrom(filterType))
            {
                throw new ArgumentException($"Command filter type {filterType.FullName} must implement {nameof(ICommandFilter)}.", nameof(filterType));
            }

            lock (s_commandFilterLock)
            {
                if (!s_transacticonTypes.ContainsKey(commandTypeId))
                {
                    throw new InvalidOperationException($"Command type id '{commandTypeId}' has not been registered.");
                }

                if (!s_commandFilters.TryGetValue(commandTypeId, out var filters))
                {
                    filters = new List<CommandFilterInfo>();
                    s_commandFilters[commandTypeId] = filters;
                }

                if (filters.Any(f => f.FilterType == filterType))
                {
                    return;
                }

                var constructors = filterType.GetConstructors();
                if (constructors.Length == 0)
                {
                    throw new InvalidOperationException($"Command filter {filterType.FullName} must expose a public constructor.");
                }

                if (constructors.Length > 1)
                {
                    throw new InvalidOperationException($"Command filter {filterType.FullName} must expose exactly one public constructor.");
                }

                var constructorParams = constructors[0].GetParameters().Select(p => p.ParameterType).ToArray();

                filters.Add(new CommandFilterInfo
                {
                    FilterType = filterType,
                    ConstructorParameters = constructorParams
                });
            }
        }

        public static void RegisterCommandFiltersFromAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes<CommandFilterAttribute>(inherit: false).ToArray();
                if (attributes.Length == 0)
                {
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    foreach (var commandTypeId in attribute.ResolveCommandTypeIds())
                    {
                        RegisterCommandFilter(commandTypeId, type);
                    }
                }
            }
        }

        private IEnumerable<ICommandFilter> GetCommandFilters(Guid commandTypeId)
        {
            List<CommandFilterInfo>? filters;
            lock (s_commandFilterLock)
            {
                s_commandFilters.TryGetValue(commandTypeId, out filters);
            }

            if (filters == null)
            {
                yield break;
            }

            foreach (var filterInfo in filters)
            {
                var parameters = filterInfo.ConstructorParameters.Select(parameterType =>
                {
                    if (parameterType.IsAssignableFrom(typeof(OTransactionService)))
                    {
                        return this;
                    }

                    return Services.GetService(parameterType);
                }).ToArray();

                yield return (ICommandFilter)Activator.CreateInstance(filterInfo.FilterType, parameters);
            }
        }

        private void EnsureCommandAllowed(OCommand command, object commandData)
        {
            foreach (var filter in GetCommandFilters(command.DataTypeID))
            {
                try
                {
                    var (available, message) = filter.CommandAvailable(command, commandData);
                    if (!available)
                    {
                        throw new CommandFilterException(string.IsNullOrWhiteSpace(message)
                            ? $"Command '{command.DataTypeID}' is currently not available."
                            : message);
                    }
                }
                finally
                {
                    if (filter is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
