using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using orch.core;
using orch.core.report.Converters;
using orch.report.Generators;

namespace orch.report.Handler
{
    public abstract class ReportHandlerBase<ParamsType> : IReportHandler
    {
        protected readonly TransactionServiceCollection _services;
        protected readonly IHtmlToPdfConverter _converter;
        protected readonly IXlsGenerator _xlsGenerator;

        protected ParamsType? _paramsData;

        protected ReportHandlerBase(TransactionServiceCollection services)
        {
            _services = services;
            _converter = _services.TranService.Services.GetRequiredService<IHtmlToPdfConverter>();
            _xlsGenerator = _services.TranService.Services.GetRequiredService<IXlsGenerator>();
        }

        public void SetData(object? data) => _paramsData = (ParamsType?)data;

        protected virtual void Authorize()
        {
        }

        public virtual void Preprocess()
        {
        }

        public abstract ReportPreview GeneratePreview();

        ViewResult IReportHandler.GeneratePreview()
        {
            return PreviewGenerator.Generate(GeneratePreview());
        }

        Task<FileContentResult> IReportHandler.GeneratePDF()
        {
            return _converter.ConvertToPdfAsync(GeneratePDF());
        }

        Task<FileContentResult> IReportHandler.GenerateXLS()
        {
            return _xlsGenerator.Generate(GenerateXLS());
        }

        public virtual PdfRequest GeneratePDF()
        {
            throw new NotSupportedException($"PDF format is not supported.");
        }

        public virtual FileContentResult GenerateCSV()
        {
            throw new NotSupportedException($"CSV format is not supported.");
        }

        public virtual XlsRequest GenerateXLS()
        {
            throw new NotSupportedException($"XLS format is not supported.");
        }
    }
}