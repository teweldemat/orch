
namespace orch.wf.model.dto
{
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum TaskSortFields
    {
        Reference,
        CreateTime,
        UpdateTime
    }
}
