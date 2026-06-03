namespace orch.report
{
    public class ReportTypeInfo
    {
        public string Key { get; set; } = string.Empty;
        public string? TypeName { get; set; }
        public Type Type { get; set; } = null!;
        public string[] Permissions { get; set; } = Array.Empty<string>();

        public ReportTypeInfo(string key, string[]? permissions = null)
        {
            Key = key;
            Permissions = permissions ?? Array.Empty<string>();
        }
    }
}
