using Microsoft.Playwright;
using Serilog;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Utility for capturing screenshots
    /// </summary>
    public static class ScreenshotUtil
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(ScreenshotUtil));

        /// <summary>
        /// Take a full-page screenshot
        /// </summary>
        public static async Task<byte[]> TakeScreenshotAsync(IPage page, string name)
        {
            try
            {
                string filename = $"screenshots/{DateTime.Now:yyyyMMdd_HHmmss}_{name}.png";
                Directory.CreateDirectory("screenshots");

                var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    FullPage = true,
                    Path = filename
                });

                Log.Information("Screenshot saved: {filename}", filename);
                return screenshot;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to take screenshot: {name}", name);
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Take a screenshot of a specific element
        /// </summary>
        public static async Task<byte[]> TakeElementScreenshotAsync(ILocator locator, string name)
        {
            try
            {
                string filename = $"screenshots/{DateTime.Now:yyyyMMdd_HHmmss}_{name}.png";
                Directory.CreateDirectory("screenshots");

                var screenshot = await locator.ScreenshotAsync(new LocatorScreenshotOptions
                {
                    Path = filename
                });

                Log.Information("Element screenshot saved: {filename}", filename);
                return screenshot;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to take element screenshot: {name}", name);
                return Array.Empty<byte>();
            }
        }
    }
}
