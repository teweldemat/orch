using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using orch.core;
using orch.report.Handler;

namespace orch.report
{
    public partial class OReportService
    {
        private readonly OTransactionService _tranService;
        private readonly IServiceProvider _services;

        public OReportService(OTransactionService tranService, IServiceProvider services)
        {
            _tranService = tranService;
            _services = services;
        }

        private void Authorize(ReportTypeInfo typeInfo, Guid userId)
        {
            if (typeInfo?.Permissions?.Length != null
                && typeInfo.Permissions.Length > 0 && !_tranService.IsRootUser(userId))
            {
                if (!_tranService.Db.IsPermitted(userId, typeInfo.Permissions, out var notGrantedPermissions))
                {
                    var notGrantedPermissionsStr = string.Join(", ", notGrantedPermissions);
                    throw new UnauthorizedAccessException($"You are not authorized to view the report: {typeInfo.TypeName}. Missing permissions: {notGrantedPermissionsStr}");
                }
            }
        }

        public ViewResult GeneratePreview(string report_type, object? data, Guid userId)
        {
            var reportTypeInfo = GetTypeInfoByKey(report_type);
            if (reportTypeInfo?.Key is null)
            {
                throw new InvalidOperationException($"Report type {report_type} does not exist");
            }

            Authorize(reportTypeInfo, userId);

            var handler = GetHandler(reportTypeInfo.Key)
                ?? throw new InvalidOperationException($"Handler for report {reportTypeInfo.Key} has not be implemented");

            handler.SetData(data);

            handler.Preprocess();

            return handler.GeneratePreview();
        }

        public Task<FileResult> GenerateFile(string report_type, ReportFormat report_format, object? data, HttpContext httpContext, Guid userId)
        {
            var reportTypeInfo = GetTypeInfoByKey(report_type);
            if (reportTypeInfo?.Key is null)
            {
                throw new InvalidOperationException($"Report type {report_type} does not exist");
            }

            Authorize(reportTypeInfo, userId);

            var reportHandlerInfo = GetHandlerInfoByKey(reportTypeInfo.Key);

            var handler = GetHandler(reportTypeInfo.Key);
            if (handler is null)
            {
                throw new InvalidOperationException($"Handler for report {reportTypeInfo.Key} has not be implemented");
            }

            var notSupportedException = new InvalidOperationException(
                    $"Report format {report_format} is not supported by report type {reportTypeInfo.Key}");

            if (!(reportHandlerInfo?.SupportedFormats?.Contains(report_format) ?? false))
            {
                throw notSupportedException;
            }

            handler.SetData(data);

            handler.Preprocess();

            return report_format switch
            {
                ReportFormat.CSV => Task.FromResult(handler.GenerateCSV()),
                ReportFormat.PDF => handler.GeneratePDF(),
                _ => throw notSupportedException,
            };
        }
    }
}