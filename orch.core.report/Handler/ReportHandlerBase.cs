using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using orch.core;
using orch.report.Generators;

namespace orch.report.Handler
{
    /// <summary>
    /// Base class for report handlers that provides common functionality and implements the IReportHandler interface.
    /// </summary>
    /// <typeparam name="ParamsType">The type of data used for generating the report.</typeparam>
    public abstract class ReportHandlerBase<ParamsType> : IReportHandler
    {
        protected readonly TransactionServiceCollection _services;
        protected readonly PdfGenerator _pdfGenerator;

        public ParamsType? _paramsData;

        protected ReportHandlerBase(TransactionServiceCollection services, PdfGenerator pdfGenerator)
        {
            _services = services;
            _pdfGenerator = pdfGenerator;
        }

        public void SetData(object? data) => _paramsData = (ParamsType?)data;

        protected virtual void Authorize()
        {
        }

        public virtual void PreProcess()
        {
        }

        public abstract ReportPreview GeneratePreview();

        ViewResult IReportHandler.GeneratePreview()
        {
            return PreviewGenerator.Generate(GeneratePreview());
        }

        Task<FileContentResult> IReportHandler.GeneratePDF(HttpContext httpContext)
        {
            return _pdfGenerator.Generate(GeneratePDF(httpContext));
        }

        public virtual ReportPdf GeneratePDF(HttpContext httpContext)
        {
            throw new NotSupportedException($"PDF format is not supported.");
        }

        public virtual FileContentResult GenerateCSV()
        {
            throw new NotSupportedException($"CSV format is not supported.");
        }
    }
}