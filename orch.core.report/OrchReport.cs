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

            switch (options.Converter)
            {
                case PdfConverter.wkhtmltopdf:

                    if (RuntimeInformation.ProcessArchitecture is not (Architecture.X86 or Architecture.X64))
                    {
                        // throw new InvalidOperationException(
                        //     "wkhtmltopdf is supported only on x86 and x64 architectures");

                        // ! Temporary solution to allow the application to run on other architectures without PDF support
                        break;
                    }
                    
                    services.AddSingleton(typeof(IConverter), new STASynchronizedConverter(new PdfTools()));
                    services.AddScoped<IHtmlToPdfConverter, DinkToPdfConverter>();

                    DinkToPdfLibrary.Load();
                    break;
                case PdfConverter.Chromium:
                {
                    var settings = configuration.GetSection(nameof(ChromiumSettings)).Get<ChromiumSettings>()
                                   ?? throw new InvalidOperationException("'ChromiumSettings' are not configured");

                    services.AddSingleton(settings!);
                    services.AddScoped<IHtmlToPdfConverter, ChromiumHtmlToPdfConverter>();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }
}