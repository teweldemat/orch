using orch.core.report.Converters;
using orch.report;
using orch.report.Generators;
using orch.report.Handler;

namespace orch.core.report.Handler
{
    [ReportType(
        key: REPORT_TYPE_KEY,
        typeName: REPORT_TYPE_NAME,
        handler: typeof(TestReportHandler),
        permissions: new string[] { CoreModule.PERMISSION_SYSTEM_ROOT })]
    public class TestReport
    {
        public const string REPORT_TYPE_KEY = "TEST_REPORT";
        public const string REPORT_TYPE_NAME = "Framework Test Report";
        public static readonly string VIEW_NAME = Path.Combine("Pages", "TestReport.cshtml");

        public class TestReportHandler : ReportHandlerBase<TestReport>
        {
            public TestReportHandler(TransactionServiceCollection services) : base(services)
            {
            }

            public override ReportPreview GeneratePreview()
            {
                return new ReportPreview(VIEW_NAME, new Dictionary<string, object>());
            }

            public override PdfRequest GeneratePDF()
            {
                return new PdfRequest()
                {
                    RazorViewPath = VIEW_NAME,
                    FileDownloadName = REPORT_TYPE_NAME,
                    PageOrientation = Converters.Orientation.Portrait,
                    PaperSize = PaperSize.A4

                };
            }
        }
    }
}
