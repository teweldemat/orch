namespace orch.report
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReportTypeAttribute : Attribute
    {
        public ReportTypeInfo? Info { get; set; }
        public Type? Handler { get; set; }

        public ReportTypeAttribute(string key, string typeName, Type handler, string[]? permissions = null)
        {
            Info = new ReportTypeInfo(key)
            {
                TypeName = typeName,
                Permissions = permissions ?? Array.Empty<string>(),
            };
            Handler = handler;
        }
    }
}
