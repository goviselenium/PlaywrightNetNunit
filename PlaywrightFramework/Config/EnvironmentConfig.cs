using System.Text.Json.Serialization;

namespace PlaywrightFramework.Config
{
    /// <summary>
    /// Maps to config/{env}.json
    /// </summary>
    public class EnvironmentConfig
    {
        [JsonPropertyName("environment")]
        public string Environment { get; set; } = string.Empty;

        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("defaultTimeout")]
        public int DefaultTimeout { get; set; } = 30000;

        [JsonPropertyName("navigationTimeout")]
        public int NavigationTimeout { get; set; } = 60000;

        [JsonPropertyName("apiBaseUrl")]
        public string ApiBaseUrl { get; set; } = string.Empty;

        [JsonPropertyName("db")]
        public DbConfig? Database { get; set; }

        /// <summary>
        /// Database configuration
        /// </summary>
        public class DbConfig
        {
            [JsonPropertyName("host")]
            public string Host { get; set; } = string.Empty;

            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }
    }
}
