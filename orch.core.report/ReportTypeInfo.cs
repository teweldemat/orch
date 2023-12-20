namespace orch.report
{
    public class ReportTypeInfo
    {
        public string Key { get; set; }
        public string? TypeName { get; set; }
        public Type Type { get; set; }
        public string[] Permissions { get; set; }

        public ReportTypeInfo(string key, string[]? permissions = null)
        {
            Key = key;
            Permissions = permissions ?? Array.Empty<string>();
        }
    }
}
