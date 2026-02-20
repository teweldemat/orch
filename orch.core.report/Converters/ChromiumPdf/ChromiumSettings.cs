namespace orch.core.report.Converters.ChromiumPdf
{
    public class ChromiumSettings
    {
        public string WindowsPath { get; set; } = string.Empty;
        public string MacOSPath { get; set; } = string.Empty;
        public string LinuxPath { get; set; } = string.Empty;
        public int MaxConcurrentPdfJobs { get; set; } = 2;
        public int PdfOperationTimeoutSeconds { get; set; } = 120;
        public int BrowserCloseTimeoutSeconds { get; set; } = 10;

        public string GetChromiumPath()
        {
            if (OperatingSystem.IsWindows())
            {
                if (string.IsNullOrEmpty(WindowsPath))
                    throw new InvalidOperationException("Chromium path has not been configured for Windows.");
                return WindowsPath;
            }

            if (OperatingSystem.IsMacOS())
            {
                if (string.IsNullOrEmpty(MacOSPath))
                    throw new InvalidOperationException("Chromium path has not been configured for macOS.");
                return MacOSPath;
            }

            if (OperatingSystem.IsLinux())
            {
                if (string.IsNullOrEmpty(LinuxPath))
                    throw new InvalidOperationException("Chromium path has not been configured for Linux.");
                return LinuxPath;
            }

            throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        public int GetMaxConcurrentPdfJobs()
        {
            return MaxConcurrentPdfJobs <= 0 ? 1 : MaxConcurrentPdfJobs;
        }

        public TimeSpan GetPdfOperationTimeout()
        {
            return TimeSpan.FromSeconds(PdfOperationTimeoutSeconds <= 0 ? 120 : PdfOperationTimeoutSeconds);
        }

        public TimeSpan GetBrowserCloseTimeout()
        {
            return TimeSpan.FromSeconds(BrowserCloseTimeoutSeconds <= 0 ? 10 : BrowserCloseTimeoutSeconds);
        }
    }

}
