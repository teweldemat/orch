namespace orch.core.report.Converters.ChromiumPdf
{
    public class ChromiumSettings
    {
        public string WindowsPath { get; set; } = string.Empty;
        public string MacOSPath { get; set; } = string.Empty;
        public string LinuxPath { get; set; } = string.Empty;

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
    }

}
