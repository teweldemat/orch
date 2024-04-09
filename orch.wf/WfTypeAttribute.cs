using orch.core;

namespace orch.wf
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WfTypeAttribute : Attribute
    {
        public readonly WfTypeInformation Info;

        public WfTypeAttribute(string name, string key, string id, Type handler = null, params string[] actionIs)
        {
            if(!Guid.TryParse(id,out var tid))
            {
                throw new InvalidOperationException($"Invalid GUID {id} set to workflow type attribute name:{name} key:{key}");
            }

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Wf type name must be set", nameof(name));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Wf type key must be set for: " + name, nameof(key));

            if (handler != null && !typeof(WfHandlerBase).IsAssignableFrom(handler))
                throw new ArgumentException(
                    $"Handler for {key} must extend WfHandlerBase. Type provided: {handler.FullName}", nameof(handler));
            
            Info = new WfTypeInformation
            {
                Key = key,
                Name = name,
                AllActions = actionIs.Select(Guid.Parse).ToArray(),
                Id=tid,
                handler= handler
            };
        }
    }
}
