using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.DependencyInjection;
using orch.report.Generators;
using orch.report.libwkhtmltox;
using System.Reflection;

namespace orch.report
{
    public static class CoreReportModule
    {
        public static void InitializeModule()
        {
            OReportService.LoadReportTypesFromAssembly(Assembly.GetExecutingAssembly());
        }

        public static void ResetModule()
        {
            OReportService.Reset();
        }

        public static void AddOrchReport(this IServiceCollection services)
        {

            DinkToPdfLibrary.Load();


            services.AddSingleton(typeof(IConverter), new STASynchronizedConverter(new PdfTools()));
            services.AddSingleton<PdfGenerator>();
            services.AddScoped<OReportService>();
        }
    }
}