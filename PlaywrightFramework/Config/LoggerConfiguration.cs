using Serilog;
using Serilog.Events;

namespace PlaywrightFramework.Config
{
    /// <summary>
    /// Initializes Serilog logging
    /// </summary>
    public static class LoggerConfiguration
    {
        public static void Configure()
        {
            var logPath = Path.Combine("logs", "test-run-{Date}.log");
            Directory.CreateDirectory("logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logPath,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "PlaywrightFramework")
                .CreateLogger();

            Log.Information("═══════════════════════════════════════════════");
            Log.Information("  Logger initialized");
            Log.Information("═══════════════════════════════════════════════");
        }

        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }
    }
}
