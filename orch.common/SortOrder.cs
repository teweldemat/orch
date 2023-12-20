namespace orch.common
{
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum SortOrder
    {
        Asc,
        Desc
    }
}
