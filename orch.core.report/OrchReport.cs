using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.DependencyInjection;
using orch.report.Generators;
using orch.report.libwkhtmltox;
using System.Reflection;
using System.Runtime.InteropServices;

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
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86
                    || RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                DinkToPdfLibrary.Load();
            }

            services.AddSingleton(typeof(IConverter), new STASynchronizedConverter(new PdfTools()));
            services.AddSingleton<PdfGenerator>();
            services.AddScoped<OReportService>();
        }
    }
}