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
        protected IHtmlToPdfConverter? _converter;

        protected ParamsType? _paramsData;

        protected ReportHandlerBase(TransactionServiceCollection services)
        {
            _services = services;
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

        Task<FileResult> IReportHandler.GeneratePDF()
        {
            _converter ??= _services.TranService.Services.GetService<IHtmlToPdfConverter>();

            if (_converter is null)
            {
                throw new NotSupportedException($"PDF format is not supported, converter is not available.");
            }
            
            return _converter.ConvertToPdfAsync(GeneratePDF());
        }

        public virtual PdfRequest GeneratePDF()
        {
            throw new NotSupportedException($"PDF format is not supported.");
        }
        
        public virtual FileResult GenerateCSV()
        {
            throw new NotSupportedException($"CSV format is not supported.");
        }
    }
}
