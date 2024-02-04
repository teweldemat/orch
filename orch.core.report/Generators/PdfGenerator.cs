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

namespace orch.report.Generators
{
    public class ReportPdf
    {
        public string FileDownloadName { get; }
        public string ViewName { get; }
        public object? Data { get; }
        public string? HeaderViewName { get; }
        public IDictionary<string, object> HeaderViewData { get; }
        public HttpContext HttpContext { get; }
        public Orientation Orientation { get; }
        public PaperKind PaperKind { get; }
        public PechkinPaperSize? CustomPaperSize { get; set; }

        public ReportPdf(
              string fileDownloadName,
              string viewName,
              IDictionary<string, object> data,
              HttpContext httpContext,
              Orientation orientation = Orientation.Landscape,
              PaperKind paperKind = PaperKind.A4,
              string? headerViewName = null,
              IDictionary<string, object>? headerViewData = null,
              PechkinPaperSize? customPaperSize = null)
        {

            FileDownloadName = FormatFileName(fileDownloadName);
            ViewName = viewName;
            Data = data;
            HttpContext = httpContext;
            HeaderViewName = headerViewName;
            HeaderViewData = headerViewData ?? new Dictionary<string, object>();
            Orientation = orientation;
            PaperKind = paperKind;
            CustomPaperSize = customPaperSize;
        }

        public ReportPdf(
         string fileDownloadName,
         string viewName,
         object data,
         HttpContext httpContext,
         Orientation orientation = Orientation.Landscape,
         string? headerViewName = null,
         PaperKind paperKind = PaperKind.A4,
         IDictionary<string, object>? headerViewData = null,
         PechkinPaperSize? customPaperSize = null)
        {
            FileDownloadName = FormatFileName(fileDownloadName);
            ViewName = viewName;
            Data = data;
            HttpContext = httpContext;
            HeaderViewName = headerViewName != null ? headerViewName : null;
            HeaderViewData = headerViewData ?? new Dictionary<string, object>();
            Orientation = orientation;
            PaperKind = paperKind;
            CustomPaperSize = customPaperSize;
        }

        private static string FormatFileName(string fileName)
        {
            return string.IsNullOrEmpty(fileName) || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".pdf";
        }
    }

    public class PdfGenerator
    {
        private readonly IConverter _pdfConverter;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PdfGenerator(
            IConverter pdfConverter,
            ICompositeViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IWebHostEnvironment webHostEnvironment)
        {
            _pdfConverter = pdfConverter;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<FileContentResult> Generate(ReportPdf pdf)
        {

            string originalHtml = RenderRazorViewToString(pdf.ViewName, pdf.Data, pdf.HttpContext);
            string cssEmbeddedHtml = EmbedLinkedCssIntoHtml(originalHtml, _webHostEnvironment);
            string fullyEmbeddedHtml = await EmbedImagesAsBase64(cssEmbeddedHtml, _webHostEnvironment, pdf.HttpContext);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = pdf.Orientation,
                        PaperSize = pdf.PaperKind,
                        DocumentTitle = pdf.FileDownloadName,

                    },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = fullyEmbeddedHtml,
                        WebSettings = { DefaultEncoding = "utf-8", LoadImages = true },
                        // ! Specifying Margins may interfere with the rendering of the header HTML
                        // Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                        HeaderSettings = { HtmUrl =  RenderHeaderViewToTempFile(pdf.HeaderViewName, pdf.HeaderViewData, pdf.HttpContext) },
                        FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    },
                 }
            };

            if (pdf.CustomPaperSize != null)
            {
                doc.GlobalSettings.PaperSize = pdf.CustomPaperSize;
            }

            var fileContents = _pdfConverter.Convert(doc);
            return new FileContentResult(fileContents, "application/pdf")
            {
                FileDownloadName = pdf.FileDownloadName
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



        private string RenderHeaderViewToTempFile(string? headerViewName, IDictionary<string, object> headerViewData, HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(headerViewName)) return string.Empty;

            var headerHtml = RenderRazorViewToString(headerViewName, headerViewData, httpContext);

            var tempHeaderHtmlFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".html");

            File.WriteAllText(tempHeaderHtmlFilePath, headerHtml);

            return tempHeaderHtmlFilePath;
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
                            using (HttpClient httpClient = new HttpClient())
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
    }
}
