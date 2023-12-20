using orch.core;

namespace orch.wf
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WfTypeAttribute:Attribute
    {
        public WfTypeInformation Info;
        
        public WfTypeAttribute(String name, String key,String id, Type handler = null, params String[] actionIs)
        {
            if(!Guid.TryParse(id,out var tid))
            {
                throw new InvalidOperationException($"Invalid GUID {id} set to workflow type attribute name:{name} key:{key}");
            }
            
            Info = new WfTypeInformation
            {
                Key = key,
                Name = name,
                AllActions = actionIs.Select(x => Guid.Parse(x)).ToArray(),
                Id=tid,
                handler= handler
            };
            
        }
    }
}
