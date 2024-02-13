using Microsoft.AspNetCore.Mvc;

namespace orch.core.report.Converters
{
    public interface IHtmlToPdfConverter
    {
        Task<FileContentResult> ConvertToPdfAsync(PdfRequest request);
    }

    public class PdfMargins
    {
        public string Top { get; set; } = "1cm";
        public string Bottom { get; set; } = "1cm";
        public string Left { get; set; } = "1cm";
        public string Right { get; set; } = "1cm";
    }

    public enum Orientation
    {
        Portrait,
        Landscape
    }

    public enum PaperSize
    {
        A2,
        A3,
        A4,
        A5,
        Legal,
        Letter,
        Custom = -1
    }

    public class CustomPaperSize
    {
        public string? Width { get; set; }
        public string? Height { get; set; }
    }

    public enum MediaType
    {
        Screen,
        Print
    }

    public class PdfRequest
    {
        public string RazorViewPath { get; set; } = string.Empty;
        public object? Model { get; set; }
        public string FileDownloadName { get; set; } = string.Empty;
        public PdfMargins? Margins { get; set; } = new();
        public Orientation PageOrientation { get; set; } = Orientation.Portrait;
        public PaperSize PaperSize { get; set; } = Converters.PaperSize.A4;
        public CustomPaperSize? CustomPaperSize { get; set; }
        public MediaType MediaType { get; set; } = MediaType.Print;
        public bool PrintBackground { get; set; } = false;
        public bool ShowPageNumbers { get; set; } = true;
    }
}
