using Microsoft.Extensions.Configuration;

internal partial class Program
{
    public static IConfiguration Configuration;

    private static void Main(string[] args)
    {
        // Set up the configuration builder
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.console.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.console.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Build the configuration
        Configuration = builder.Build();

        if (args.Length > 1)
        {
        }

        Console.WriteLine("Welcome to the Orchastrator command line");
        Console.WriteLine("Loading orch application");

        RunCommandLine(args);
    }
}