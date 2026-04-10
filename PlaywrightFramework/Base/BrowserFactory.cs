using Microsoft.Playwright;
using Serilog;

namespace PlaywrightFramework.Base
{
    /// <summary>
    /// Factory for creating browser instances and contexts
    /// </summary>
    public static class BrowserFactory
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(BrowserFactory));

        /// <summary>
        /// Create a browser instance
        /// </summary>
        public static async Task<IBrowser> CreateBrowserAsync(IPlaywright? playwright, string browserName, bool headless)
        {
            if (playwright == null)
                throw new ArgumentNullException(nameof(playwright));

            Log.Information("Creating {browser} browser (headless: {headless})", browserName, headless);

            var browserType = browserName.ToLower() switch
            {
                "firefox" => playwright.Firefox,
                "webkit" => playwright.WebKit,
                _ => playwright.Chromium
            };

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = headless
            };

            var browser = await browserType.LaunchAsync(launchOptions);
            Log.Information("Browser launched successfully");
            return browser;
        }

        /// <summary>
        /// Create a browser context with video recording
        /// </summary>
        public static async Task<IBrowserContext> CreateContextAsync(IBrowser browser, string videoDirectory)
        {
            if (browser == null)
                throw new ArgumentNullException(nameof(browser));

            // Ensure video directory exists
            Directory.CreateDirectory(videoDirectory);

            var contextOptions = new BrowserNewContextOptions
            {
                RecordVideoDir = videoDirectory,
                IgnoreHTTPSErrors = true,
                AcceptDownloads = true
            };

            Log.Information("Creating browser context with video recording to: {dir}", videoDirectory);
            var context = await browser.NewContextAsync(contextOptions);
            return context;
        }
    }
}
