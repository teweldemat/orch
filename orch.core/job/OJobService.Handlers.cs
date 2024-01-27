using orch.core.errors;
using orch.core.job;
using System.Reflection;

namespace orch.core
{
    public sealed partial class OJobService
    {
        static readonly Dictionary<Guid, JobTypeInfo> s_jobTypes = new();
        static readonly Dictionary<Type, JobTypeInfo> s_jobTypesByType = new();
        static readonly Dictionary<string, JobTypeInfo> s_jobTypesByKey = new();
        static readonly Dictionary<string, List<JobTypeInfo>> s_jobTypesByAssembly = new();
        static readonly Dictionary<Guid, JobHandlerInfo> s_jobHandlerInfos = new();

        class JobHandlerInfo
        {
            public Type HandlerType;
            public Type[] ConstructorParameters;
        }

        public static void Reset()
        {
            s_jobTypes.Clear();
            s_jobTypesByType.Clear();
            s_jobTypesByKey.Clear();
            s_jobTypesByAssembly.Clear();
            s_jobHandlerInfos.Clear();
        }

        public static void LoadJTFromAssembly(Assembly assembly)
        {
            lock (s_jobTypes)
            {
                var assemblyName = assembly.GetName().Name;
                if (s_jobTypesByAssembly.ContainsKey(assemblyName))
                {
                    throw new InvalidOperationException($"Job Types for {assemblyName} have already registered.");
                }

                var types = assembly.GetTypes();
                s_jobTypesByAssembly[assemblyName] = new List<JobTypeInfo>();
                foreach (var type in types)
                {
                    ProcessType(type, assemblyName);
                }
            }
        }

        private static void ProcessType(Type t, string assemblyName)
        {

            var jobAttr = t.GetCustomAttribute<BackgroundJobAttribute>(false);

            if (jobAttr == null) return;

            ValidateAttribute(jobAttr);

            s_jobTypes.Add(jobAttr.TypeInfo.TypeId, jobAttr.TypeInfo);
            jobAttr.TypeInfo.Type = t;
            jobAttr.TypeInfo.TypeName ??= t.FullName;

            s_jobTypesByType[t] = jobAttr.TypeInfo;
            s_jobTypesByKey[jobAttr.TypeInfo.Key] = jobAttr.TypeInfo;

            if (jobAttr.Handler != null && jobAttr.Handler.GetInterface(typeof(IJobHandler).FullName) != null)
            {
                ValidateHandler(jobAttr);
            }

            s_jobTypesByAssembly[assemblyName].Add(jobAttr.TypeInfo);
        }

        private static void ValidateAttribute(BackgroundJobAttribute atr)
        {
            if (s_jobTypes.ContainsKey(atr.TypeInfo.TypeId))
            {
                throw new InvalidOperationException($"id {atr.TypeInfo.TypeId} that is assigned to {atr.TypeInfo.Type} is already assigned to {s_jobTypes[atr.TypeInfo.TypeId].TypeName}");
            }
        }

        private static void ValidateHandler(BackgroundJobAttribute atr)
        {
            var constructors = atr.Handler.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidHandlerException($"Job handler {atr.Handler} doesn't have a constructor");
            }

            if (constructors.Length > 1)
            {
                throw new InvalidHandlerException($"Multiple constructors found for job handler {atr.Handler} and that is not allowed");
            }

            var constructorParams = constructors[0].GetParameters().Select(p => p.ParameterType).ToArray();
            s_jobHandlerInfos[atr.TypeInfo.TypeId] = new JobHandlerInfo
            {
                HandlerType = atr.Handler,
                ConstructorParameters = constructorParams
            };
        }


        public IJobHandler GetHandler(Guid typeId)
        {
            if (!s_jobHandlerInfos.ContainsKey(typeId)) return null;

            var handlerInfo = s_jobHandlerInfos[typeId];
            var parameters = handlerInfo.ConstructorParameters.Select(x =>
            {
                if (x.IsAssignableFrom(typeof(OJobService)))
                {
                    return this;
                }

                return Services.GetService(x);

            }).ToArray();

            return Activator.CreateInstance(handlerInfo.HandlerType, parameters) as IJobHandler;
        }

        public static JobTypeInfo? GetTypeIdByType(Type t)
            => s_jobTypesByType.GetValueOrDefault(t);

        public static JobTypeInfo? GetTypeIdByKey(string key)
            => s_jobTypesByKey.GetValueOrDefault(key);

        public static JobTypeInfo? GetTypeInfoById(Guid id)
            => s_jobTypes.GetValueOrDefault(id);

        public static IList<string> GetAllJobTypeKeys()
            => s_jobTypes.Values.Select(x => x.Key).ToList();

        public static IList<JobTypeInfo> GetAllJobTypes()
            => s_jobTypes.Values.ToList();

        public static IList<string> GetAllAssemblyNames()
            => s_jobTypesByAssembly.Keys.ToList();

        public static IList<JobTypeInfo> GetJobTypesByAssembly(string assemblyName)
            => s_jobTypesByAssembly.ContainsKey(assemblyName)
                ? s_jobTypesByAssembly[assemblyName]
                    : throw new ArgumentException($"No types found for assembly {assemblyName}");
    }
}
