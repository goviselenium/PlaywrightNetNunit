using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Serilog;

namespace PlaywrightFramework.Reports
{
    /// <summary>
    /// Manages Extent Reports for test result reporting
    /// </summary>
    public static class ExtentReportManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(ExtentReportManager));
        private static readonly ThreadLocal<ExtentReports> _extent = new();
        private static readonly ThreadLocal<ExtentTest> _test = new();

        private static string _reportPath = "";
        private static string _timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        /// <summary>
        /// Initialize the Extent Report
        /// </summary>
        public static void InitReport(string reportDirectory = "test-output/extent-reports")
        {
            try
            {
                var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _reportPath = Path.Combine(reportDirectory, timeStamp, "index.html");
                Directory.CreateDirectory(Path.GetDirectoryName(_reportPath)!);

                var htmlReporter = new ExtentHtmlReporter(_reportPath);
                htmlReporter.Config.DocumentTitle = "Playwright Automation Report";
                htmlReporter.Config.ReportName = "Automation Test Results";

                var extent = new ExtentReports();
                extent.AttachReporter(htmlReporter);

                // System info
                extent.AddSystemInfo("Environment", ConfigManager.Instance.Env.ToUpper());
                extent.AddSystemInfo("Browser", ConfigManager.Instance.Browser);
                extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
                extent.AddSystemInfo("Base URL", ConfigManager.Instance.BaseUrl);

                _extent.Value = extent;
                Log.Information("Extent Report initialized: {path}", _reportPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize Extent Report");
            }
        }

        /// <summary>
        /// Start a new test in the report
        /// </summary>
        public static void StartTest(string testName, string className)
        {
            if (_extent.Value == null)
                InitReport();

            var test = _extent.Value!.CreateTest($"{className}::{testName}");
            test.Log(Status.Info, $"Started test: {testName}");
            _test.Value = test;
        }

        /// <summary>
        /// Log a step
        /// </summary>
        public static void LogStep(string message)
        {
            _test.Value?.Log(Status.Info, message);
            Log.Information("Step: {msg}", message);
        }

        /// <summary>
        /// Log info
        /// </summary>
        public static void Info(string message)
        {
            _test.Value?.Log(Status.Info, message);
            Log.Information(message);
        }

        /// <summary>
        /// Mark test as passed
        /// </summary>
        public static void PassTest(string message)
        {
            _test.Value?.Pass(message);
            Log.Information("Test PASSED: {msg}", message);
        }

        /// <summary>
        /// Mark test as failed
        /// </summary>
        public static void FailTest(Exception? exception, byte[]? screenshot = null)
        {
            var message = exception?.Message ?? "Test failed";
            _test.Value?.Fail(message);
            
            if (screenshot != null && screenshot.Length > 0)
                AttachScreenshot(screenshot, "Failure Screenshot");

            Log.Error(exception, "Test FAILED");
        }

        /// <summary>
        /// Mark test as skipped
        /// </summary>
        public static void SkipTest(string reason)
        {
            _test.Value?.Skip(reason);
            Log.Warning("Test SKIPPED: {reason}", reason);
        }

        /// <summary>
        /// Attach a screenshot to the report
        /// </summary>
        public static void AttachScreenshot(byte[] screenshot, string title)
        {
            if (_test.Value == null || screenshot == null || screenshot.Length == 0)
                return;

            try
            {
                var base64 = Convert.ToBase64String(screenshot);
                var imagePath = $"data:image/png;base64,{base64}";
                _test.Value.AddScreenCaptureFromPath(imagePath, title);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to attach screenshot to report");
            }
        }

        /// <summary>
        /// Attach a video link to the report
        /// </summary>
        public static void AttachVideo(string videoPath)
        {
            if (_test.Value == null || string.IsNullOrEmpty(videoPath))
                return;

            try
            {
                _test.Value.Log(Status.Info, $"Video: <a href='{videoPath}'>Click to view</a>");
                Log.Information("Video attached: {path}", videoPath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to attach video to report");
            }
        }

        /// <summary>
        /// Flush (write) the report to disk
        /// </summary>
        public static void FlushReport()
        {
            try
            {
                _extent.Value?.Flush();
                Log.Information("Extent Report flushed to: {path}", _reportPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to flush Extent Report");
            }
            finally
            {
                _extent.Value = null;
                _test.Value = null;
            }
        }
    }
}
