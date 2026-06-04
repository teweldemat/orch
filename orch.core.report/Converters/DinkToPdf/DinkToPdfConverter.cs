using DinkToPdf;
using DinkToPdf.Contracts;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using orch.core.report.Converters;

namespace orch.report.Generators
{

    public class DinkToPdfConverter : IHtmlToPdfConverter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConverter _pdfConverter;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DinkToPdfConverter(
            IHttpContextAccessor httpContextAccessor,
            IConverter pdfConverter,
            ICompositeViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IWebHostEnvironment webHostEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _pdfConverter = pdfConverter;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<FileResult> ConvertToPdfAsync(PdfRequest request)
        {
            var httpContext = (_httpContextAccessor?.HttpContext)
                ?? throw new InvalidOperationException($"'{nameof(_httpContextAccessor.HttpContext)}' is null. Are you missing the '{nameof(IHttpContextAccessor)}' middleware?");

            string html = RenderRazorViewToString(request.RazorViewPath, request.Model, httpContext);
            html = EmbedLinkedCssIntoHtml(html, _webHostEnvironment);
            html = await EmbedImagesAsBase64(html, _webHostEnvironment, httpContext);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = request.PageOrientation == core.report.Converters.Orientation.Portrait ? DinkToPdf.Orientation.Portrait: DinkToPdf.Orientation.Landscape,
                        PaperSize = PaperSizeToPaperKind(request.PaperSize, request.CustomPaperSize),
                        DocumentTitle = FormatFileName(request.FileDownloadName),
                    },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8", LoadImages = true },
                        // ! Specifying Margins may interfere with the rendering of the header HTML
                        FooterSettings = request.ShowPageNumbers ? new FooterSettings() { FontSize = 9, Right = "Page [page] of [toPage]", Line = true } : null,
                    },
                 }
            };

            var fileContents = _pdfConverter.Convert(doc);
            return new FileContentResult(fileContents, "application/pdf")
            {
                FileDownloadName = request.FileDownloadName
            };
        }

        private string RenderRazorViewToString(string viewName, object? data, HttpContext httpContext)
        {
            var viewResult = _viewEngine.GetView(null, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} not found.");
            }

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = data };
            var viewContext = new ViewContext(
                   new ActionContext(httpContext, httpContext.GetRouteData(), new ControllerActionDescriptor()),
                   viewResult.View,
                   viewData,
                   new TempDataDictionary(httpContext, _tempDataProvider),
                   new StringWriter(),
                   new HtmlHelperOptions()
               );

            viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
            return viewContext.Writer.ToString() ?? throw new InvalidOperationException($"Error occured while rendering {viewName}.");
        }

        private static string EmbedLinkedCssIntoHtml(string originalHtml, IWebHostEnvironment _webHostEnvironment)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originalHtml);

            var linkNodes = htmlDoc.DocumentNode.SelectNodes("//link[@rel='stylesheet']");

            if (linkNodes != null)
            {
                foreach (var linkNode in linkNodes)
                {
                    string hrefValue = linkNode.GetAttributeValue("href", "").TrimStart('/');

                    // Remove 'wwwroot' if present
                    if (hrefValue.StartsWith("wwwroot/"))
                    {
                        hrefValue = hrefValue["wwwroot/".Length..];
                    }

                    // Check in main project's wwwroot
                    string mainCssPath = Path.Combine(_webHostEnvironment.WebRootPath, hrefValue);

                    // Check in RCLs' wwwroot
                    IFileProvider rclProvider = _webHostEnvironment.WebRootFileProvider;
                    IFileInfo fileInfo = rclProvider.GetFileInfo(hrefValue);

                    if (File.Exists(mainCssPath))
                    {
                        string cssContent = File.ReadAllText(mainCssPath);
                        string styleTag = $"<style>{cssContent}</style>";
                        originalHtml = originalHtml.Insert(originalHtml.IndexOf("</head>", StringComparison.Ordinal), styleTag);
                    }
                    else if (fileInfo.Exists)
                    {
                        using var streamReader = new StreamReader(fileInfo.CreateReadStream());
                        string cssContent = streamReader.ReadToEnd();
                        string styleTag = $"<style>{cssContent}</style>";
                        originalHtml = originalHtml.Insert(originalHtml.IndexOf("</head>", StringComparison.Ordinal), styleTag);
                    }
                }
            }

            return originalHtml;
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
                                stream.ReadExactly(imageBytes);
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

        private PechkinPaperSize PaperSizeToPaperKind(PaperSize paperSize, CustomPaperSize? customPaperSize)
        {
            if (customPaperSize is not null)
            {
                if (string.IsNullOrEmpty(customPaperSize.Width) || string.IsNullOrEmpty(customPaperSize.Height))
                    throw new InvalidOperationException($"Width and Height must be provided to {nameof(CustomPaperSize)}");

                return new PechkinPaperSize(customPaperSize.Width, customPaperSize.Height);
            }

            return paperSize switch
            {
                PaperSize.Custom => PaperKind.Custom,
                PaperSize.A2 => PaperKind.A2,
                PaperSize.A3 => PaperKind.A3,
                PaperSize.A4 => PaperKind.A4,
                PaperSize.A5 => PaperKind.A5,
                PaperSize.Legal => PaperKind.Legal,
                PaperSize.Letter => PaperKind.Letter,
                _ => throw new ArgumentOutOfRangeException(nameof(paperSize), paperSize, null)
            };
        }

        private static string FormatFileName(string fileName)
        {
            return string.IsNullOrEmpty(fileName) || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".pdf";
        }
    }
}
