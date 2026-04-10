using Microsoft.Playwright;
using NUnit.Framework;
using Serilog;
using PlaywrightFramework.Config;
using PlaywrightFramework.Reports;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Base
{
    /// <summary>
    /// BaseTest wires together:
    ///  - Playwright lifecycle (init / quit)
    ///  - Extent Reports (start / pass / fail / skip)
    ///  - Screenshot on failure
    ///  - Video path attachment
    ///  - Logging
    /// </summary>
    [SetUpFixture]
    public abstract class BaseTest
    {
        protected readonly ILogger Log;
        protected ConfigManager Config;
        protected IPage? Page;

        protected BaseTest()
        {
            Log = Serilog.Log.ForContext(GetType());
            Config = ConfigManager.Instance;
        }

        // ── Suite lifecycle ──────────────────────────────────────────────────────

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ExtentReportManager.InitReport();
            Log.Information("═══════════════════════════════════════════════");
            Log.Information("  SUITE STARTED — ENV: {env}", ConfigManager.Instance.Env.ToUpper());
            Log.Information("═══════════════════════════════════════════════");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ExtentReportManager.FlushReport();
            Log.Information("═══════════════════════════════════════════════");
            Log.Information("  SUITE FINISHED — Report written.");
            Log.Information("═══════════════════════════════════════════════");
        }

        // ── Test lifecycle ───────────────────────────────────────────────────────

        [SetUp]
        public async Task SetUpTest()
        {
            Config = ConfigManager.Instance;
            await PlaywrightManager.InitAsync();
            Page = PlaywrightManager.GetPage();

            var testName = TestContext.CurrentContext.Test.Name;
            var className = GetType().Name;
            ExtentReportManager.StartTest(testName, className);

            Log.Information("▶ TEST STARTED : {className} :: {testName}", className, testName);
            Log.Information("  Environment  : {env}", Config.Env.ToUpper());
            Log.Information("  Base URL     : {baseUrl}", Config.BaseUrl);
        }

        [TearDown]
        public async Task TearDownTest()
        {
            Page = PlaywrightManager.GetPage();
            var testName = TestContext.CurrentContext.Test.Name;
            var testStatus = TestContext.CurrentContext.Result.Outcome.Status;

            try
            {
                switch (testStatus)
                {
                    case NUnit.Framework.Interfaces.TestStatus.Passed:
                        Log.Information("✅ PASSED  : {testName}", testName);
                        ExtentReportManager.PassTest("Test Passed");
                        break;

                    case NUnit.Framework.Interfaces.TestStatus.Failed:
                        Log.Error("❌ FAILED  : {testName}", testName);
                        
                        if (Page != null)
                        {
                            // Screenshot
                            var screenshot = await ScreenshotUtil.TakeScreenshotAsync(Page, $"{testName}_failure");
                            if (screenshot.Length > 0)
                                ExtentReportManager.AttachScreenshot(screenshot, "Failure Screenshot");

                            // Video path
                            var videoPath = await VideoUtil.GetVideoPathAsync(Page);
                            if (!string.IsNullOrEmpty(videoPath))
                            {
                                ExtentReportManager.AttachVideo(videoPath);
                                Log.Information("  Video saved: {videoPath}", videoPath);
                            }
                        }

                        var exception = TestContext.CurrentContext.Result.Message;
                        ExtentReportManager.FailTest(new Exception(exception));
                        break;

                    case NUnit.Framework.Interfaces.TestStatus.Skipped:
                        Log.Warning("⏭ SKIPPED : {testName}", testName);
                        ExtentReportManager.SkipTest(TestContext.CurrentContext.Result.Message ?? "Test skipped");
                        break;

                    default:
                        Log.Warning("⚠ UNKNOWN STATUS : {testName}", testName);
                        break;
                }
            }
            finally
            {
                await PlaywrightManager.QuitAsync();
                Log.Information("◼ TEST ENDED  : {testName}", testName);
            }
        }

        // ── Convenience methods for sub-classes ─────────────────────────────────

        protected IPage GetPage()
        {
            var page = PlaywrightManager.GetPage();
            if (page == null)
                throw new InvalidOperationException("Page is not initialized");
            return page;
        }

        /// <summary>
        /// Get the retry count from config
        /// </summary>
        protected int GetRetryCount() => Config.RetryCount;
    }
}
