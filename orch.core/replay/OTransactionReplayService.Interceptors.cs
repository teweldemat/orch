using System.Reflection;

namespace orch.core.replay
{
    public static class OTransactionReplayService
    {
        static readonly Dictionary<Type, object> s_genericReplayInterceptors = new();

        public static void Reset()
        {
            lock (s_genericReplayInterceptors)
            {
                s_genericReplayInterceptors.Clear();
            }
        }

        public static void LoadRITypes(Assembly assembly)
        {
            lock (s_genericReplayInterceptors)
            {
                var interceptorTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReplayInterceptor<>)))
                    .ToList();

                foreach (var interceptorType in interceptorTypes)
                {
                    var interfaceType = interceptorType.GetInterfaces()
                        .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReplayInterceptor<>));
                    var dataType = interfaceType.GetGenericArguments()[0];
                    RegisterInterceptor(interceptorType, dataType);
                }
            }
        }

        private static void RegisterInterceptor(Type interceptorType, Type dataType)
        {
            if (s_genericReplayInterceptors.ContainsKey(dataType))
            {
                throw new InvalidOperationException($"An interceptor for type {dataType.FullName} is already registered.");
            }

            var interceptorInstance = Activator.CreateInstance(interceptorType);
            s_genericReplayInterceptors[dataType] = interceptorInstance
                ?? throw new InvalidOperationException($"Failed to create instance of {interceptorType.FullName}.");
        }

        public static IReplayInterceptor<T>? GetInterceptor<T>()
        {
            if (s_genericReplayInterceptors.TryGetValue(typeof(T), out var interceptor))
            {
                return (IReplayInterceptor<T>)interceptor;
            }

            return null;
        }

        public static IList<IReplayInterceptor<dynamic>> GetAllReplayInterceptors()
        {
            return s_genericReplayInterceptors.Values.Cast<IReplayInterceptor<dynamic>>().ToList();
        }
    }
}
