using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace orch.core.report.Generators.ChromiumPdf
{
    public class ChromiumHtmlToPdfConverter : IHtmlToPdfConverter
    {
        private readonly ChromiumSettings _chromiumSettings;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChromiumHtmlToPdfConverter(
            ChromiumSettings chromiumSettings,
            ICompositeViewEngine viewEngine,
            IHttpContextAccessor httpContextAccessor,
            ITempDataProvider tempDataProvider,
            IWebHostEnvironment webHostEnvironment)
        {
            _chromiumSettings = chromiumSettings ?? throw new ArgumentNullException(nameof(chromiumSettings));
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        public async Task<FileContentResult> ConvertToPdfAsync(PdfConversionArgs args)
        {
            string htmlContent = await RenderRazorViewAsync(args.RazorViewPath, args.Model);
            htmlContent = InlineStylesheets(htmlContent);
            htmlContent = await EmbedImagesAsBase64(htmlContent, _webHostEnvironment, _httpContextAccessor?.HttpContext);

            var chromiumPath = _chromiumSettings.GetChromiumPath();

            using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, ExecutablePath = chromiumPath, Args = new[] { "--no-sandbox" } });
            using var page = await browser.NewPageAsync();

            await page.EmulateMediaTypeAsync((args.MediaType) switch
            {
                MediaType.Screen => PuppeteerSharp.Media.MediaType.Screen,
                MediaType.Print => PuppeteerSharp.Media.MediaType.Print,
                _ => throw new InvalidDataException($"Unsupported media type: {args.MediaType}")
            });

            await page.SetContentAsync(htmlContent);

            var pdfOptions = new PdfOptions
            {
                Format = SizeToFormat(args.PaperSize),
                PrintBackground = args.PrintBackground,
                PreferCSSPageSize = false,
                MarginOptions = args.Margins is null ? new() : new MarginOptions
                {
                    Top = args.Margins.Top,
                    Bottom = args.Margins.Bottom,
                    Left = args.Margins.Left,
                    Right = args.Margins.Right
                },
                Landscape = args.PageOrientation == Orientation.Landscape
            };

            var pdfStream = await page.PdfStreamAsync(pdfOptions);
            byte[] pdfBytes;
            using (var memoryStream = new MemoryStream())
            {
                pdfStream.CopyTo(memoryStream);
                pdfBytes = memoryStream.ToArray();
            }

            return new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = args.FileDownloadName
            };
        }

        private async Task<string> RenderRazorViewAsync(string viewName, object? model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _httpContextAccessor?.HttpContext?.RequestServices };
            var actionDescriptor = new ActionDescriptor();
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var viewResult = _viewEngine.GetView(null, viewName, false);

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"Couldn't find view '{viewName}'");
            }

            var view = viewResult.View;
            using var sw = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);
            return sw.ToString();

        }

        private string InlineStylesheets(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var linkNodes = htmlDoc.DocumentNode.SelectNodes("//link[@rel='stylesheet']");
            if (linkNodes == null) return htmlContent;

            foreach (var linkNode in linkNodes)
            {
                string hrefValue = linkNode.GetAttributeValue("href", "").TrimStart('/');

                if (hrefValue.StartsWith("wwwroot/"))
                {
                    hrefValue = hrefValue["wwwroot/".Length..];
                }

                if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
                {
                    throw new InvalidOperationException($"'{nameof(_webHostEnvironment.WebRootPath)}' is null or empty. Are you missing the 'wwwroot' folder?");
                }

                string cssFilePath = Path.Combine(_webHostEnvironment.WebRootPath, hrefValue);
                IFileProvider fileProvider = _webHostEnvironment.WebRootFileProvider;
                IFileInfo fileInfo = fileProvider.GetFileInfo(hrefValue);

                string? cssContent = null;

                if (File.Exists(cssFilePath))
                {
                    cssContent = File.ReadAllText(cssFilePath);
                }
                else if (fileInfo.Exists)
                {
                    using var streamReader = new StreamReader(fileInfo.CreateReadStream());
                    cssContent = streamReader.ReadToEnd();
                }

                if (cssContent != null)
                {
                    string styleTag = $"<style>{cssContent}</style>";
                    int headCloseTagIndex = htmlContent.IndexOf("</head>", StringComparison.Ordinal);
                    if (headCloseTagIndex >= 0)
                    {
                        htmlContent = htmlContent.Insert(headCloseTagIndex, styleTag);
                    }
                }
            }

            return htmlContent;
        }

        private static async Task<string> EmbedImagesAsBase64(string originalHtml, IWebHostEnvironment webHostEnvironment, HttpContext httpContext)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originalHtml);

            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img");

            if (imgNodes != null)
            {
                foreach (var imgNode in imgNodes)
                {
                    string srcValue = imgNode.GetAttributeValue("src", "").TrimStart('/');
                    byte[]? imageBytes = Array.Empty<byte>();

                    if (srcValue.StartsWith("http"))
                    {
                        try
                        {
                            using HttpClient httpClient = new();
                            imageBytes = await httpClient.GetByteArrayAsync(srcValue);
                        }
                        catch
                        {
                            imageBytes = Array.Empty<byte>();
                        }
                    }
                    else
                    {
                        string fullSrcPath = Path.Combine(webHostEnvironment.WebRootPath, srcValue);

                        IFileProvider fileProvider = webHostEnvironment.WebRootFileProvider;
                        IFileInfo fileInfo = fileProvider.GetFileInfo(srcValue);

                        if (fileInfo.Exists)
                        {
                            using (var stream = fileInfo.CreateReadStream())
                            {
                                imageBytes = new byte[stream.Length];
                                stream.Read(imageBytes, 0, imageBytes.Length);
                            }
                        }
                        else if (File.Exists(fullSrcPath))
                        {
                            imageBytes = File.ReadAllBytes(fullSrcPath);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    string base64String = Convert.ToBase64String(imageBytes);
                    imgNode.SetAttributeValue("src", "data:image/png;base64," + base64String);
                }
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }

        static PaperFormat SizeToFormat(PaperSize size)
        {
            return size switch
            {
                PaperSize.A3 => PaperFormat.A3,
                PaperSize.A4 => PaperFormat.A4,
                PaperSize.A5 => PaperFormat.A5,
                PaperSize.Legal => PaperFormat.Legal,
                PaperSize.Letter => PaperFormat.Letter,
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }
}
