using Microsoft.Playwright;
using Serilog;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Utility for custom wait strategies
    /// </summary>
    public static class WaitUtil
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(WaitUtil));
        private const int DefaultPollMs = 500;
        private const int DefaultTimeoutMs = 30_000;

        // ── Custom condition polling ─────────────────────────────────────────────

        /// <summary>
        /// Polls condition every 500ms until it returns true or times out
        /// </summary>
        public static async Task WaitUntilAsync(Func<bool> condition, string description)
        {
            await WaitUntilAsync(condition, description, DefaultTimeoutMs, DefaultPollMs);
        }

        public static async Task WaitUntilAsync(Func<bool> condition, string description, int timeoutMs, int pollMs)
        {
            Log.Debug("Polling [{pollMs}ms/{timeoutMs}ms]: {desc}", pollMs, timeoutMs, description);
            long deadline = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + timeoutMs;

            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < deadline)
            {
                try
                {
                    if (condition())
                    {
                        Log.Debug("Condition met: {desc}", description);
                        return;
                    }
                    await Task.Delay(pollMs);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error checking condition: {desc}", description);
                    await Task.Delay(pollMs);
                }
            }

            throw new TimeoutException($"Condition not met within {timeoutMs}ms: {description}");
        }

        /// <summary>
        /// Async version for async conditions
        /// </summary>
        public static async Task WaitUntilAsync(Func<Task<bool>> condition, string description)
        {
            await WaitUntilAsync(condition, description, DefaultTimeoutMs, DefaultPollMs);
        }

        public static async Task WaitUntilAsync(Func<Task<bool>> condition, string description, int timeoutMs, int pollMs)
        {
            Log.Debug("Polling async [{pollMs}ms/{timeoutMs}ms]: {desc}", pollMs, timeoutMs, description);
            long deadline = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + timeoutMs;

            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < deadline)
            {
                try
                {
                    if (await condition())
                    {
                        Log.Debug("Async condition met: {desc}", description);
                        return;
                    }
                    await Task.Delay(pollMs);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error checking async condition: {desc}", description);
                    await Task.Delay(pollMs);
                }
            }

            throw new TimeoutException($"Async condition not met within {timeoutMs}ms: {description}");
        }

        // ── Page load waits ──────────────────────────────────────────────────────

        public static async Task WaitForPageLoadAsync(IPage page)
        {
            Log.Debug("Waiting for page load state");
            await page.WaitForLoadStateAsync(LoadState.Load);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }

        public static async Task WaitForNetworkIdleAsync(IPage page)
        {
            Log.Debug("Waiting for network idle");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // ── Fixed delay (use sparingly) ──────────────────────────────────────────

        /// <summary>
        /// Hard wait. Use only when there is no reliable element/condition to poll
        /// </summary>
        public static async Task HardWaitAsync(int millis)
        {
            Log.Warning("HardWait({ms}ms) — prefer condition-based waits", millis);
            await Task.Delay(millis);
        }

        // ── URL / Title waits ────────────────────────────────────────────────────

        public static async Task WaitForUrlContainsAsync(IPage page, string fragment)
        {
            Log.Debug("Waiting for URL to contain: {fragment}", fragment);
            await page.WaitForURLAsync($"**{fragment}**");
        }

        public static async Task WaitForTitleContainsAsync(IPage page, string title)
        {
            await WaitUntilAsync(() => page.Title.Contains(title), $"Title contains: {title}");
        }
    }
}
