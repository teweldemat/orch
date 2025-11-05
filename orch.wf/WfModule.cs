using orch.core;
using System.Reflection;

namespace orch.wf
{
    public class WfTypeInformation
    {
        public Type Type;
        public string Key;
        public string Name;
        public Guid[] AllActions;
        public Type handler;
        public Guid Id { get; internal set; }
        public Type[] ConstructorParameters { get; set; }
    }
    public class WfModule
    {
        static private readonly Dictionary<Type, WfTypeInformation> s_workflowTypes = new();
        static private readonly Dictionary<Guid, WfTypeInformation> s_workflowTypesById = new();
        static private readonly Dictionary<string, WfTypeInformation> s_workflowTypeByKey = new();
        public static void LoadWorkflowTypes(Assembly a)
        {

            lock (s_workflowTypes)
            {
                var types = a.GetTypes();
                foreach (var t in types)
                {
                    var atr = t.GetCustomAttribute<WfTypeAttribute>(false);
                    if (atr != null)
                    {
                        atr.Info.Type = t;
                        if (atr.Info.handler != null)
                        {
                            var c = atr.Info.handler.GetConstructors();
                            if (c.Length == 0)
                                throw new Exception($"Workflow handler {atr.Info.handler} don't have constructor");
                            if (c.Length != 1)
                                throw new Exception($"Multiple constructors found for transaction handler {atr.Info.handler} and that is not allowed");

                            atr.Info.ConstructorParameters = c[0].GetParameters().Select(x => x.ParameterType).ToArray();
                        }
                        s_workflowTypeByKey.Add(atr.Info.Key, atr.Info);
                        s_workflowTypes.Add(t, atr.Info);
                        s_workflowTypesById.Add(atr.Info.Id, atr.Info);

                    }
                }
            }

        }
        public static WfTypeInformation GetWfTypeInfo(Type stateType)
        {
            if (s_workflowTypes.ContainsKey(stateType))
                return s_workflowTypes[stateType];
            return null;
        }
        public static IEnumerable<WfTypeInformation> GetActionWfTypes(Guid actionTypeID)
        {
            return s_workflowTypes.Values.Where(t => t.AllActions.Contains(actionTypeID));
        }
        public static WfTypeInformation GetWfTypeInfo(Guid typeId)
        {
            if (s_workflowTypesById.ContainsKey(typeId))
                return s_workflowTypesById[typeId];
            return null;
        }
        public static WfTypeInformation GetWfTypeInfo(string key)
        {
            if (s_workflowTypeByKey.ContainsKey(key))
                return s_workflowTypeByKey[key];
            return null;
        }

        public static WfTypeInformation GetWfTypeInfoByActionId(Guid actionId)
        {
            return s_workflowTypes.Values.FirstOrDefault(t => t.AllActions.Contains(actionId));
        }

        public static IList<WfTypeInformation> GetWorkflowTypes()
        {
            return s_workflowTypes.Values.ToList();
        }


        public static void InitializeModule()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QueryComposer.LoadViews(assembly);
            OTransactionService.LoadTTFromAssembly(assembly);
            OTransactionService.RegisterCommandFiltersFromAssembly(assembly);
        }

        public static void ResetModule()
        {
            s_workflowTypes.Clear();
            s_workflowTypeByKey.Clear();
            s_workflowTypesById.Clear();
        }

    }
}
