using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using orch.core.report.Generators;
using orch.core.report.Generators.ChromiumPdf;
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

        public static void AddOrchReport(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var chromiumSettings = new ChromiumSettings();
            configuration.GetSection(nameof(ChromiumSettings)).Bind(chromiumSettings);
            services.AddSingleton(chromiumSettings);

            services.AddScoped<IHtmlToPdfConverter, ChromiumHtmlToPdfConverter>();
            services.AddScoped<OReportService>();
        }
    }
}