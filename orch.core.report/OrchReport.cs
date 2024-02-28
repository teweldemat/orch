using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using orch.core.report.Converters;
using orch.core.report.Converters.ChromiumPdf;
using orch.report.Generators;
using orch.report.libwkhtmltox;
using System.Reflection;
using System.Runtime.InteropServices;

namespace orch.report
{
    public enum PdfConverter
    {
        wkhtmltopdf,
        Chromium
    }

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

        public class OrchReportOptions
        {
            public PdfConverter Converter { get; set; } = PdfConverter.Chromium;
        }

        public static void AddReportServices(this IServiceCollection services, IConfiguration configuration, OrchReportOptions options)
        {
            services.AddScoped<OReportService>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            #region pdf
            if (options.Converter == PdfConverter.Chromium)
            {
                var settings = configuration.GetSection(nameof(ChromiumSettings)).Get<ChromiumSettings>()
                ?? throw new InvalidOperationException("'ChromiumSettings' are not configured");

                services.AddSingleton(settings!);
                services.AddScoped<IHtmlToPdfConverter, ChromiumHtmlToPdfConverter>();
            }
            else if (options.Converter == PdfConverter.wkhtmltopdf)
            {
                services.AddSingleton(typeof(IConverter), new STASynchronizedConverter(new PdfTools()));
                services.AddScoped<IHtmlToPdfConverter, DinkToPdfConverter>();

                // DinkToPdf is supported only on x86 and x64 architectures
                var architecture = RuntimeInformation.ProcessArchitecture;
                if (architecture is (Architecture.X86 or Architecture.X64))
                {
                    DinkToPdfLibrary.Load();
                }
            }
            #endregion
        }
    }
}