using Microsoft.Playwright;
using Serilog;
using PlaywrightFramework.Config;

namespace PlaywrightFramework.Base
{
    /// <summary>
    /// Thread-safe holder for Playwright resources.
    /// One Playwright + Browser + Context + Page per test thread.
    /// </summary>
    public static class PlaywrightManager
    {
        private static readonly ThreadLocal<IPlaywright> _playwright = new();
        private static readonly ThreadLocal<IBrowser> _browser = new();
        private static readonly ThreadLocal<IBrowserContext> _context = new();
        private static readonly ThreadLocal<IPage> _page = new();

        /// <summary>
        /// Initialise Playwright resources for a test
        /// </summary>
        public static async Task InitAsync()
        {
            var cfg = ConfigManager.Instance;

            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browser = await BrowserFactory.CreateBrowserAsync(playwright, cfg.Browser, cfg.IsHeadless);

            string videoDir = Path.Combine("videos", Thread.CurrentThread.Name ?? "test");
            var context = await BrowserFactory.CreateContextAsync(browser, videoDir);

            // Global page timeouts
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(cfg.DefaultTimeout);
            page.SetDefaultNavigationTimeout(cfg.NavigationTimeout);

            _playwright.Value = playwright;
            _browser.Value = browser;
            _context.Value = context;
            _page.Value = page;

            Log.Information("PlaywrightManager initialised for thread: {thread}", Thread.CurrentThread.Name ?? "main");
        }

        /// <summary>
        /// Get the current page object
        /// </summary>
        public static IPage? GetPage() => _page.Value;

        /// <summary>
        /// Get the current browser
        /// </summary>
        public static IBrowser? GetBrowser() => _browser.Value;

        /// <summary>
        /// Get the current context
        /// </summary>
        public static IBrowserContext? GetContext() => _context.Value;

        /// <summary>
        /// Tear down all Playwright resources
        /// </summary>
        public static async Task QuitAsync()
        {
            try
            {
                if (_context.Value != null)
                    await _context.Value.CloseAsync();
                if (_browser.Value != null)
                    await _browser.Value.CloseAsync();
                if (_playwright.Value != null)
                    _playwright.Value.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error during PlaywrightManager teardown");
            }
            finally
            {
                _page.Value = null;
                _context.Value = null;
                _browser.Value = null;
                _playwright.Value = null;
                Log.Information("PlaywrightManager resources released for thread: {thread}", 
                    Thread.CurrentThread.Name ?? "main");
            }
        }
    }
}
