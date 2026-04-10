using System.Text.Json;
using Serilog;

namespace PlaywrightFramework.Config
{
    /// <summary>
    /// Singleton configuration manager.
    /// 
    /// Resolution order for environment name:
    ///   1. Environment variable ENV
    ///   2. Falls back to "qa"
    /// 
    /// Usage:
    ///   ConfigManager.Instance.BaseUrl
    /// </summary>
    public sealed class ConfigManager
    {
        private static readonly Lazy<ConfigManager> _instance = new(() => new ConfigManager());
        public static ConfigManager Instance => _instance.Value;

        private readonly EnvironmentConfig _config;
        private readonly string _browser;
        private readonly bool _headless;
        private readonly int _retryCount;
        private readonly string _env;

        private ConfigManager()
        {
            _env = (Environment.GetEnvironmentVariable("ENV") ?? "qa").ToLower();
            _browser = (Environment.GetEnvironmentVariable("BROWSER") ?? "chromium").ToLower();
            _headless = bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var h) && h;
            _retryCount = int.TryParse(Environment.GetEnvironmentVariable("RETRY_COUNT"), out var r) ? r : 2;

            Log.Information("╔══════════════════════════════════════╗");
            Log.Information("║  Initialising Framework Config        ║");
            Log.Information("╠══════════════════════════════════════╣");
            Log.Information("║  ENV        : {env,22}║", _env.ToUpper());
            Log.Information("║  BROWSER    : {browser,22}║", _browser);
            Log.Information("║  HEADLESS   : {headless,22}║", _headless);
            Log.Information("║  RETRY CNT  : {retry,22}║", _retryCount);
            Log.Information("╚══════════════════════════════════════╝");

            _config = LoadConfig(_env);
        }

        // ── Public getters ───────────────────────────────────────────────────
        public string BaseUrl => _config.BaseUrl;
        public string Username => _config.Username;
        public string Password => _config.Password;
        public int DefaultTimeout => _config.DefaultTimeout;
        public int NavigationTimeout => _config.NavigationTimeout;
        public string Browser => _browser;
        public bool IsHeadless => _headless;
        public int RetryCount => _retryCount;
        public string Env => _env;
        public EnvironmentConfig EnvConfig => _config;

        // ── Internal helpers ─────────────────────────────────────────────────
        private EnvironmentConfig LoadConfig(string environment)
        {
            string resourcePath = $"Resources/Config/{environment}.json";
            Log.Information("Loading environment config from: {path}", resourcePath);

            try
            {
                // Look for the config file relative to the executing assembly
                var baseDir = AppContext.BaseDirectory;
                var configPath = Path.Combine(baseDir, resourcePath);

                // Fallback: look in project's Resources folder if running in dev
                if (!File.Exists(configPath))
                {
                    var projectResourcePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Tests", "Resources", "Config", $"{environment}.json");
                    if (File.Exists(projectResourcePath))
                        configPath = projectResourcePath;
                }

                if (!File.Exists(configPath))
                    throw new FileNotFoundException($"Config file not found: {configPath}. Valid environments: qa, staging");

                var json = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<EnvironmentConfig>(json, options)
                    ?? throw new InvalidOperationException($"Failed to deserialize config from {configPath}");

                Log.Information("Config loaded → baseUrl: {url}", config.BaseUrl);
                return config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load config for environment: {env}", environment);
                throw new InvalidOperationException($"Failed to load config for environment: {environment}", ex);
            }
        }
    }
}
