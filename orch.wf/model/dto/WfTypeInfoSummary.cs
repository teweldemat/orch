namespace orch.wf.model.dto
{
    public class WfTypeInfoSummary
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }

        public WfTypeInfoSummary(WfTypeInformation typeInfo)
        {
            Id = typeInfo.Id;
            Key = typeInfo.Key;
            Name = typeInfo.Name;
        }
    }

}
