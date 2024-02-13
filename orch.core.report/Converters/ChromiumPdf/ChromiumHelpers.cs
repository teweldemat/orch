using PuppeteerSharp;

namespace orch.core.report.Converters.ChromiumPdf
{
    public class ChromiumHelpers
    {
        private readonly ChromiumSettings _settings;

        public ChromiumHelpers(ChromiumSettings settings)
        {
            _settings = settings;
        }

        public async Task<int> CalculateHtmlHeightAsync(string htmlContent, int width)
        {
            var chromiumPath = _settings.GetChromiumPath();

            // Launch the browser in headless mode
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromiumPath,
                Args = new[] { "--no-sandbox" }
            });

            // Create a new page
            using var page = await browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions { Width = width });

            // Set the HTML content
            await page.SetContentAsync(htmlContent, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

            var height = await page.EvaluateExpressionAsync<int>("document.documentElement.scrollHeight");

            return height;
        }
    }
}
