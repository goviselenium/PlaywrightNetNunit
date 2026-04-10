using Microsoft.Playwright;
using Serilog;
using System.Text;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Utility for validating element visibility in bulk
    /// </summary>
    public static class VisibilityValidator
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(VisibilityValidator));

        /// <summary>
        /// Check visibility of multiple elements and return a report
        /// </summary>
        public static async Task<VisibilityReport> CheckAllAsync(IPage page, Dictionary<string, string> elements, int timeoutMs = 30_000)
        {
            Log.Information("Running visibility checks on {count} elements", elements.Count);

            var report = new VisibilityReport();
            var startTime = DateTime.Now;

            foreach (var item in elements)
            {
                var name = item.Key;
                var selector = item.Value;

                try
                {
                    var locator = page.Locator(selector);
                    await locator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = timeoutMs
                    });
                    
                    report.Passed.Add(name);
                    Log.Information("✅ {name} is VISIBLE", name);
                }
                catch (TimeoutException)
                {
                    report.Failed.Add(name, $"Element NOT VISIBLE or timeout exceeded");
                    Log.Error("❌ {name} is NOT VISIBLE (selector: {selector})", name, selector);
                }
                catch (Exception ex)
                {
                    report.Failed.Add(name, $"Error: {ex.Message}");
                    Log.Error(ex, "❌ Error checking visibility of {name}", name);
                }
            }

            report.Duration = DateTime.Now - startTime;
            return report;
        }

        /// <summary>
        /// Quick check if element is visible (no wait)
        /// </summary>
        public static async Task<bool> IsVisibleAsync(IPage page, string selector)
        {
            try
            {
                return await page.Locator(selector).IsVisibleAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if element is in viewport
        /// </summary>
        public static async Task<bool> IsInViewportAsync(IPage page, string selector)
        {
            try
            {
                var result = await page.EvaluateAsync<bool>(
                    @"(selector) => {
                        const element = document.querySelector(selector);
                        if (!element) return false;
                        const rect = element.getBoundingClientRect();
                        return (
                            rect.top >= 0 &&
                            rect.left >= 0 &&
                            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
                            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
                        );
                    }", 
                    selector);
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Report class for visibility checks
        /// </summary>
        public class VisibilityReport
        {
            public List<string> Passed { get; } = new();
            public Dictionary<string, string> Failed { get; } = new();
            public TimeSpan Duration { get; set; }

            public bool AllPassed => Failed.Count == 0;

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Visibility Report (Duration: {Duration.TotalMilliseconds}ms)");
                sb.AppendLine($"├─ Passed: {Passed.Count}");
                
                if (Passed.Count > 0)
                {
                    foreach (var item in Passed)
                        sb.AppendLine($"│  ✅ {item}");
                }

                sb.AppendLine($"└─ Failed: {Failed.Count}");
                
                if (Failed.Count > 0)
                {
                    foreach (var item in Failed)
                        sb.AppendLine($"   ❌ {item.Key}: {item.Value}");
                }

                return sb.ToString();
            }

            public string FailureSummary()
            {
                if (AllPassed)
                    return "All elements are visible";

                var sb = new StringBuilder();
                foreach (var item in Failed)
                    sb.AppendLine($"  • {item.Key}: {item.Value}");

                return sb.ToString();
            }
        }
    }
}
