using Microsoft.AspNetCore.Mvc;

namespace orch.core.report.Generators
{
    public interface IHtmlToPdfConverter
    {
        Task<FileContentResult> ConvertToPdfAsync(PdfConversionArgs args);
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
        A3,
        A4,
        A5,
        Legal,
        Letter
    }

    public enum MediaType
    {
        Screen,
        Print
    }

    public class PdfConversionArgs
    {
        public string RazorViewPath { get; set; } = string.Empty;
        public object? Model { get; set; }
        public string FileDownloadName { get; set; } = string.Empty;
        public PdfMargins? Margins { get; set; } = new();
        public Orientation PageOrientation { get; set; } = Orientation.Portrait;
        public PaperSize PaperSize { get; set; } = PaperSize.A4;
        public MediaType MediaType { get; set; } = MediaType.Print;
        public bool PrintBackground { get; set; } = false;
    }
}
