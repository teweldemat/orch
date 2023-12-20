using DinkToPdf;
using Microsoft.AspNetCore.Http;
using orch.report;
using orch.report.Generators;
using orch.report.Handler;

namespace orch.core.report.Handler
{
    [ReportType(
        key: REPORT_TYPE_KEY,
        typeName: REPORT_TYPE_NAME,
        handler: typeof(TestReportHandler)
        , permissions: new string[] { CoreModule.PERMISSION_SYSTEM_ROOT }
        )
        ]
    public class TestReport
    {
        public const string REPORT_TYPE_KEY = "TEST_REPORT";
        public const string REPORT_TYPE_NAME = "Framework Test Report";
        public static readonly string VIEW_NAME = Path.Combine("Pages", "TestReport.cshtml");


        public class TestReportHandler : ReportHandlerBase<TestReport>
        {
            public TestReportHandler(TransactionServiceCollection services, PdfGenerator pdfGenerator) : base(services, pdfGenerator)
            {
            }

            public override ReportPreview GeneratePreview()
            {
                return new ReportPreview(VIEW_NAME, new Dictionary<string, object>());
            }

            public override ReportPdf GeneratePDF(HttpContext httpContext)
            {
                return new ReportPdf(
                    REPORT_TYPE_NAME,
                    VIEW_NAME,
                    new Dictionary<string, object>(),
                    httpContext,
                    orientation: Orientation.Portrait,
                    customPaperSize: new PechkinPaperSize("80mm", "500mm"));
            }
        }
    }
}
