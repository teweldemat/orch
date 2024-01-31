using Microsoft.AspNetCore.Mvc;

namespace orch.core.report.Generators
{
    public interface IHtmlToPdfConverter
    {
        Task<FileContentResult> ConvertToPdfAsync(PdfConversionArgs args);
    }

    public class PdfMargins
    {
        public string Top { get; set; } = "0cm";
        public string Bottom { get; set; } = "0cm";
        public string Left { get; set; } = "0cm";
        public string Right { get; set; } = "0cm";

        public static PdfMargins StandardA4 => new()
        {
            Top = "2.54cm",
            Bottom = "2.54cm",
            Left = "2.54cm",
            Right = "2.54cm"
        };
    }

    public enum Orientation
    {
        Portrait,
        Landscape
    }

    public class PdfConversionArgs
    {
        public string RazorViewPath { get; set; } = string.Empty;
        public object? Model { get; set; }
        public string FileDownloadName { get; set; } = string.Empty;
        public PdfMargins Margins { get; set; } = new PdfMargins();
        public Orientation PageOrientation { get; set; } = Orientation.Portrait;
    }
}
