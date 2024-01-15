using orch.core.errors;
using System.Reflection;

namespace orch.core
{
    public partial class OTransactionService
    {
        static readonly Dictionary<Guid, CommandTypeInfo> s_transacticonTypes = new();
        static readonly Dictionary<Type, CommandTypeInfo> s_transactionTypesByType = new();
        static readonly Dictionary<string, CommandTypeInfo> s_transactionTypesByKey = new();
        static readonly Dictionary<string, List<CommandTypeInfo>> s_transactionTypesByAssembly = new();
        static readonly Dictionary<Guid, CommandHandlerInfo> s_transactionHandlerInfos = new();

        class CommandHandlerInfo
        {
            public Type HandlerType;
            public Type[] ConstructorParameters;
        }

        static readonly Dictionary<Guid, CommandInitializerInfo> s_transactionInitializerInfos = new();

        class CommandInitializerInfo
        {
            public Type InitializerType;
            public Type[] ConstructorParameters;
        }

        public static void Reset()
        {
            s_transacticonTypes.Clear();
            s_transactionTypesByType.Clear();
            s_transactionTypesByKey.Clear();
            s_transactionTypesByAssembly.Clear();
            s_transactionHandlerInfos.Clear();

            QueryComposer.Reset();
        }

        public static void LoadTTFromAssembly(Assembly assembly)
        {
            lock (s_transacticonTypes)
            {
                var assemblyName = assembly.GetName().Name;
                if (s_transactionTypesByAssembly.ContainsKey(assemblyName))
                {
                    throw new InvalidOperationException($"Transaction Types for {assemblyName} have already registered.");
                }

                var types = assembly.GetTypes();
                s_transactionTypesByAssembly[assemblyName] = new List<CommandTypeInfo>();
                foreach (var type in types)
                {
                    ProcessType(type, assemblyName);
                }
            }
        }

        private static void ProcessType(Type t, string assemblyName)
        {

            var cmdAtr = t.GetCustomAttribute<CommandTypeAttribute>(false);

            if (cmdAtr == null) return;

            cmdAtr.ttInfo.Type = t;
            cmdAtr.ttInfo.TypeName ??= t.FullName;

            ValidateAttribute(cmdAtr);

            s_transacticonTypes.Add(cmdAtr.ttInfo.TypeId, cmdAtr.ttInfo);

            s_transactionTypesByType[t] = cmdAtr.ttInfo;
            s_transactionTypesByKey[cmdAtr.ttInfo.Key] = cmdAtr.ttInfo;

            if (cmdAtr.initializer != null && cmdAtr.initializer.GetInterface(typeof(ICommandInitializer).FullName) != null)
            {
                ValidateInitializer(cmdAtr);
            }

            if (cmdAtr.handler != null && cmdAtr.handler.GetInterface(typeof(ICommandHandler).FullName) != null)
            {
                ValidateHandler(cmdAtr);
            }

            s_transactionTypesByAssembly[assemblyName].Add(cmdAtr.ttInfo);
        }

        private static void ValidateAttribute(CommandTypeAttribute atr)
        {
            if (s_transacticonTypes.ContainsKey(atr.ttInfo.TypeId))
            {
                throw new InvalidOperationException($"id {atr.ttInfo.TypeId} that is assigned to {atr.ttInfo.Type} is already assigned to {s_transacticonTypes[atr.ttInfo.TypeId].TypeName}");
            }
        }

        private static void ValidateHandler(CommandTypeAttribute atr)
        {
            var constructors = atr.handler.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidHandlerException($"Command handler {atr.handler} doesn't have a constructor");
            }

            if (constructors.Length > 1)
            {
                throw new InvalidHandlerException($"Multiple constructors found for transaction handler {atr.handler} and that is not allowed");
            }

            var constructorParams = constructors[0].GetParameters().Select(p => p.ParameterType).ToArray();
            s_transactionHandlerInfos[atr.ttInfo.TypeId] = new CommandHandlerInfo
            {
                HandlerType = atr.handler,
                ConstructorParameters = constructorParams
            };
        }
        private static void ValidateInitializer(CommandTypeAttribute atr)
        {
            var constructors = atr.initializer.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidInitializerException($"Command initializer {atr.initializer} doesn't have a constructor");
            }

            if (constructors.Length > 1)
            {
                throw new InvalidInitializerException($"Multiple constructors found for transaction initializer {atr.initializer} and that is not allowed");
            }

            var constructorParams = constructors[0].GetParameters().Select(p => p.ParameterType).ToArray();
            s_transactionInitializerInfos[atr.ttInfo.TypeId] = new CommandInitializerInfo
            {
                InitializerType = atr.initializer,
                ConstructorParameters = constructorParams
            };
        }

        public ICommandHandler GetHandler(Guid typeId)
        {
            if (!s_transactionHandlerInfos.ContainsKey(typeId)) return null;

            var handlerInfo = s_transactionHandlerInfos[typeId];
            var parameters = handlerInfo.ConstructorParameters.Select(x =>
            {
                if (x.IsAssignableFrom(typeof(OTransactionService)))
                {
                    return this;
                }

                return Services.GetService(x);

            }).ToArray();

            return Activator.CreateInstance(handlerInfo.HandlerType, parameters) as ICommandHandler;
        }

        public ICommandInitializer GetInitializer(Guid typeId)
        {
            if (!s_transactionInitializerInfos.ContainsKey(typeId)) return null;

            var initializerInfo = s_transactionInitializerInfos[typeId];
            var parameters = initializerInfo.ConstructorParameters.Select(x =>
            {
                if (x.IsAssignableFrom(typeof(OTransactionService)))
                {
                    return this;
                }

                return Services.GetService(x);

            }).ToArray();

            return Activator.CreateInstance(initializerInfo.InitializerType, parameters) as ICommandInitializer;
        }

        public static CommandTypeInfo GetTypeIdByType(Type t)
            => s_transactionTypesByType.GetValueOrDefault(t);

        public static CommandTypeInfo GetTypeIdByKey(string key)
            => s_transactionTypesByKey.GetValueOrDefault(key);

        public static CommandTypeInfo GetTypeInfoById(Guid id)
            => s_transacticonTypes.GetValueOrDefault(id);

        public static IList<string> GetAllTransactionTypeKeys()
            => s_transacticonTypes.Values.Select(x => x.Key).ToList();

        public static IList<CommandTypeInfo> GetAllCommandTypes()
            => s_transacticonTypes.Values.ToList();


        public static IList<string> GetAllAssemblyNames()
            => s_transactionTypesByAssembly.Keys.ToList();

        public static IList<CommandTypeInfo> GetTransactionTypesByAssembly(string assemblyName)
            => s_transactionTypesByAssembly.ContainsKey(assemblyName)
                ? s_transactionTypesByAssembly[assemblyName]
                : throw new ArgumentException($"No types found for assembly {assemblyName}");


    }
}
