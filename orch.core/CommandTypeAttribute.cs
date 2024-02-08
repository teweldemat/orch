using Newtonsoft.Json;

namespace orch.core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class OGeneratedDataAttribute : Attribute
    {
    }

    public class CommandTypeInfo
    {
        public Guid TypeId;
        public string Key { get; set; }
        public string TypeName { get; set; }

        [JsonIgnore]
        public Type Type { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandTypeAttribute : Attribute
    {
        internal CommandTypeInfo ttInfo;
        public Type handler;
        public Type initializer;

        public CommandTypeAttribute(
            string typeId,
            string key,
            string typeName,
            Type handler = null,
            Type initializer = null)
        {
            ttInfo = new CommandTypeInfo();

            if (!Guid.TryParse(typeId, out ttInfo.TypeId))
            {
                throw new InvalidOperationException($"Failed to parse '{typeId}' as a GUID for the command type '{typeName}'. Please ensure that the provided typeId is a valid GUID.");
            }

            ttInfo.TypeName = typeName;
            ttInfo.Key = key;

            if (handler != null && !typeof(ICommandHandler).IsAssignableFrom(handler))
            {
                throw new ArgumentException($"The provided handler type, {handler.Name}, does not implement ICommandHandler.", nameof(handler));
            }

            this.handler = handler;

            if (initializer != null && !typeof(ICommandInitializer).IsAssignableFrom(initializer))
            {
                throw new ArgumentException($"The provided initializer type, {initializer.Name}, does not implement ICommandInitializer.", nameof(initializer));
            }

            this.initializer = initializer;
        }
    }
}
