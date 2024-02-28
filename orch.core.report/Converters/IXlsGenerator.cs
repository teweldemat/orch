using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace orch.core.report.Converters
{
    public class XlsRequestHeader
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Subtitle2 { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int ImageHeight { get; set; } = 75;
    }
    public class XlsRequest
    {
        public string FileDownloadName { get; set; } = string.Empty;
        public IList<XlsWorksheet> Worksheets { get; set; } = new List<XlsWorksheet>();
        public XlsRequestHeader? Header { get; set; }
    }

    public interface IXlsGenerator
    {
        Task<FileContentResult> Generate(XlsRequest request);
    }

    public abstract class XlsWorksheet
    {
        public string Name { get; set; } = string.Empty;
    }

    public class XlsDataTableWorksheet : XlsWorksheet
    {
        public DataTable DataTable { get; set; } = new();
    }

    public class XlsCollectionWorksheet<T> : XlsWorksheet
    {
        public List<T> Data { get; set; } = new List<T>();
        public List<string> IncludedProperties { get; set; } = new List<string>();
    }

}
