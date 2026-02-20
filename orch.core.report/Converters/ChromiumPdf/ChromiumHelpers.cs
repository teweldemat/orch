using PuppeteerSharp;

namespace orch.core.report.Converters.ChromiumPdf
{
    public class ChromiumHelpers
    {
        private static readonly object s_semaphoreLock = new();
        private static SemaphoreSlim? s_semaphore;
        private static int s_semaphoreSize;

        private readonly ChromiumSettings _settings;

        public ChromiumHelpers(ChromiumSettings settings)
        {
            _settings = settings;
        }

        public async Task<int> CalculateHtmlHeightAsync(string htmlContent, int width)
        {
            var chromiumPath = _settings.GetChromiumPath();
            var semaphore = GetSemaphore();
            await semaphore.WaitAsync();

            IBrowser? browser = null;
            IPage? page = null;

            try
            {
                browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = chromiumPath,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-gpu",
                        "--no-zygote",
                        "--disable-crash-reporter"
                    },
                    Timeout = (int)_settings.GetPdfOperationTimeout().TotalMilliseconds
                });

                page = await browser.NewPageAsync();
                page.DefaultTimeout = (int)_settings.GetPdfOperationTimeout().TotalMilliseconds;
                page.DefaultNavigationTimeout = (int)_settings.GetPdfOperationTimeout().TotalMilliseconds;

                await page.SetViewportAsync(new ViewPortOptions { Width = width });

                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    Timeout = (int)_settings.GetPdfOperationTimeout().TotalMilliseconds,
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }
                });

                var height = await page.EvaluateExpressionAsync<int>("document.documentElement.scrollHeight");

                return height;
            }
            finally
            {
                await TryClosePageAsync(page);
                await TryCloseBrowserAsync(browser);
                semaphore.Release();
            }
        }

        private SemaphoreSlim GetSemaphore()
        {
            var maxJobs = _settings.GetMaxConcurrentPdfJobs();
            lock (s_semaphoreLock)
            {
                if (s_semaphore is null || s_semaphoreSize != maxJobs)
                {
                    s_semaphore?.Dispose();
                    s_semaphore = new SemaphoreSlim(maxJobs, maxJobs);
                    s_semaphoreSize = maxJobs;
                }
                return s_semaphore;
            }
        }

        private async Task TryClosePageAsync(IPage? page)
        {
            if (page is null) return;
            try
            {
                using var cts = new CancellationTokenSource(_settings.GetBrowserCloseTimeout());
                await page.CloseAsync().WaitAsync(cts.Token);
            }
            catch
            {
            }
        }

        private async Task TryCloseBrowserAsync(IBrowser? browser)
        {
            if (browser is null) return;

            try
            {
                using var cts = new CancellationTokenSource(_settings.GetBrowserCloseTimeout());
                await browser.CloseAsync().WaitAsync(cts.Token);
            }
            catch
            {
            }

            try
            {
                if (browser.Process is { HasExited: false } process)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(5000);
                }
            }
            catch
            {
            }

            try
            {
                browser.Dispose();
            }
            catch
            {
            }
        }
    }
}
