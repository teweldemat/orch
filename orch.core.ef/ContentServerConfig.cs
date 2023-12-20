namespace orch.core.ef
{
    public class ContentServerConfig
    {
        public string BaseDir { get; set; } = string.Empty;
        public string AppServer { get; set; } = string.Empty;
        public double MaxFileSize { get; set; } = 10_485_760; // 10 MB in bytes
    }
}