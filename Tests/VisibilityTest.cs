using NUnit.Framework;
using Serilog;
using PlaywrightFramework.Base;
using PlaywrightFramework.Pages;
using PlaywrightFramework.Reports;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Tests
{
    /// <summary>
    /// Dedicated visibility testing suite
    /// 
    /// Tests:
    ///  - Element visibility detection
    ///  - Bulk visibility checks with reports
    ///  - Viewport detection
    ///  - Element visibility state changes
    /// </summary>
    [TestFixture]
    [Category("Visibility")]
    public class VisibilityTest : BaseTest
    {
        // ────────────────────────────────────────────────────────────────────────
        //  TC01 — Login page element visibility
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Smoke")]
        [Description("Verify login form elements are visible and in correct state")]
        public async Task VerifyLoginFormElementsVisibility()
        {
            ExtentReportManager.LogStep("TC01 — Login form visibility test");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");

            // Check individual elements
            Assert.That(await loginPage.IsVisibleAsync("#username, input[name='username'], input[type='email']"), 
                Is.True, "Username input should be visible");

            Assert.That(await loginPage.IsVisibleAsync("#password, input[name='password'], input[type='password']"), 
                Is.True, "Password input should be visible");

            Assert.That(await loginPage.IsVisibleAsync("button[type='submit'], #login-btn, button:has-text('Login'), button:has-text('Sign in')"), 
                Is.True, "Login button should be visible");

            ExtentReportManager.LogStep("✅ TC01 PASSED: All login form elements are visible");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC02 — Bulk element visibility check
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Visibility")]
        [Description("Check multiple elements at once with detailed report")]
        public async Task VerifyBulkElementVisibility()
        {
            ExtentReportManager.LogStep("TC02 — Bulk element visibility check");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");

            var elements = new Dictionary<string, string>
            {
                { "Username Field", "#username" },
                { "Password Field", "#password" },
                { "Login Button", "button[type='submit']" },
                { "Page Heading", "h1" }
            };

            var report = await VisibilityValidator.CheckAllAsync(GetPage(), elements);
            
            Log.Information("Visibility Report:\n{report}", report.ToString());
            ExtentReportManager.Info($"Visibility Check Report:\n{report}");

            Assert.That(report.AllPassed, Is.True, 
                $"All elements should be visible. Failed: {string.Join(", ", report.Failed.Keys)}");

            ExtentReportManager.LogStep($"✅ TC02 PASSED: All {report.Passed.Count} elements are visible");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC03 — Viewport detection
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Visibility")]
        [Description("Check if elements are in viewport")]
        public async Task VerifyElementInViewport()
        {
            ExtentReportManager.LogStep("TC03 — Element in viewport detection");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");

            var selector = "button[type='submit'], #login-btn";
            var inViewport = await VisibilityValidator.IsInViewportAsync(GetPage(), selector);

            Log.Information("Login button in viewport: {inViewport}", inViewport);
            ExtentReportManager.LogStep($"Login button in viewport: {inViewport}");

            if (!inViewport)
            {
                await loginPage.ScrollIntoViewAsync(selector);
                var afterScroll = await VisibilityValidator.IsInViewportAsync(GetPage(), selector);
                Assert.That(afterScroll, Is.True, "Button should be in viewport after scroll");
                ExtentReportManager.LogStep("Button brought into viewport via scroll");
            }

            ExtentReportManager.LogStep("✅ TC03 PASSED: Viewport detection working correctly");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC04 — Element visibility state changes
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Visibility")]
        [Description("Verify element visibility changes during test execution")]
        public async Task VerifyElementVisibilityStateChanges()
        {
            ExtentReportManager.LogStep("TC04 — Element visibility state changes");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");

            // Login form should be visible initially
            var loginFormVisible = await loginPage.IsVisibleAsync("button[type='submit']");
            Assert.That(loginFormVisible, Is.True, "Login form should be visible initially");
            ExtentReportManager.LogStep("✅ Login form visible initially");

            // After login, error message might appear
            await loginPage.EnterUsernameAsync("invalid@example.com");
            await loginPage.EnterPasswordAsync("wrongpassword");
            await loginPage.ClickLoginButtonAsync();

            // Wait for error
            await WaitUtil.WaitUntilAsync(
                async () => await loginPage.IsErrorDisplayedAsync(),
                "Error should appear after invalid login",
                10_000, 500);

            var errorVisible = await loginPage.IsErrorDisplayedAsync();
            Assert.That(errorVisible, Is.True, "Error message should become visible");
            ExtentReportManager.LogStep("✅ Error message appeared as expected");

            ExtentReportManager.LogStep("✅ TC04 PASSED: Element visibility state changes work correctly");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC05 — Hidden element assertion
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Visibility")]
        [Description("Verify that hidden elements are correctly identified")]
        public async Task VerifyHiddenElementDetection()
        {
            ExtentReportManager.LogStep("TC05 — Hidden element detection");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");

            // Error message should NOT be visible initially
            var errorVisible = await loginPage.IsErrorDisplayedAsync();
            Assert.That(errorVisible, Is.False, "Error message should be hidden initially");
            ExtentReportManager.LogStep("✅ Error message correctly identified as hidden");

            // Test should not throw when asserting hidden elements
            await loginPage.AssertNotVisibleAsync(".error-message, .alert-danger, [data-testid='error-msg']", 
                "Error message");
            ExtentReportManager.LogStep("✅ AssertNotVisible passed for hidden error");

            ExtentReportManager.LogStep("✅ TC05 PASSED: Hidden element detection working correctly");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC06 — Multiple elements count
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Visibility")]
        [Description("Count visible elements and verify element collection")]
        public async Task VerifyElementCounting()
        {
            ExtentReportManager.LogStep("TC06 — Element counting");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");

            // Count visible buttons
            var visibleCount = await loginPage.CountVisibleAsync("button");
            Log.Information("Found {count} visible buttons", visibleCount);
            ExtentReportManager.LogStep($"Found {visibleCount} visible buttons on login page");

            Assert.That(visibleCount, Is.GreaterThan(0), "Should find at least one button");

            // Get all visible inputs
            var visibleInputs = await loginPage.GetAllVisibleAsync("input");
            Log.Information("Found {count} visible input fields", visibleInputs.Count);
            ExtentReportManager.LogStep($"Found {visibleInputs.Count} visible input fields");

            Assert.That(visibleInputs.Count, Is.GreaterThanOrEqualTo(2), 
                "Should find at least 2 input fields (username and password)");

            ExtentReportManager.LogStep("✅ TC06 PASSED: Element counting and collection works correctly");
        }
    }
}
