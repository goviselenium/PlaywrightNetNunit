using NUnit.Framework;
using Serilog;
using PlaywrightFramework.Base;
using PlaywrightFramework.Pages;
using PlaywrightFramework.Reports;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Tests
{
    /// <summary>
    /// Login test suite
    /// 
    /// Covers:
    ///  - Successful login
    ///  - Invalid credentials
    ///  - Login page visibility checks
    ///  - Post-login dashboard validation
    ///  - Session / logout flow
    /// </summary>
    [TestFixture]
    [Category("Login")]
    public class LoginTest : BaseTest
    {
        // ────────────────────────────────────────────────────────────────────────
        //  TC01 — Login page element visibility
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Smoke")]
        [Category("Visibility")]
        [Description("Verify all login page elements are visible")]
        public async Task VerifyLoginPageVisibility()
        {
            ExtentReportManager.LogStep("TC01 — Login page visibility test");
            Log.Information("Starting login page visibility test on: {url}", Config.BaseUrl);

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");
            await loginPage.AssertLoginPageElementsAsync();

            var ss = await loginPage.CaptureScreenshotAsync("login_page_visible");
            ExtentReportManager.AttachScreenshot(ss, "Login page — all elements visible");

            ExtentReportManager.LogStep("✅ TC01 PASSED: Login page visibility verified");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC02 — Successful login
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Smoke")]
        [Category("Regression")]
        [Description("Valid credentials should navigate to Dashboard")]
        public async Task VerifySuccessfulLogin()
        {
            ExtentReportManager.LogStep("TC02 — Valid login test");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");
            await loginPage.AssertLoginFormVisibleAsync();

            var beforeSS = await loginPage.CaptureScreenshotAsync("before_login");
            ExtentReportManager.AttachScreenshot(beforeSS, "Before Login");

            var dashboard = await loginPage.LoginAsAsync(Config.Username, Config.Password);
            await dashboard.WaitForLoadAsync();

            var afterSS = await dashboard.CaptureScreenshotAsync("after_login");
            ExtentReportManager.AttachScreenshot(afterSS, "After Login — Dashboard");

            // Verify we navigated away from login
            string currentUrl = dashboard.GetCurrentUrl();
            Log.Information("Post-login URL: {url}", currentUrl);
            ExtentReportManager.Info($"Post-login URL: {currentUrl}");

            Assert.That(currentUrl, Does.Not.Contain("/login"),
                "Should not still be on login page after successful login");

            await dashboard.AssertDashboardElementsVisibleAsync();
            await dashboard.AssertOptionalElementsVisibleAsync();

            ExtentReportManager.LogStep("✅ TC02 PASSED: Successful login and dashboard verified");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC03 — Invalid credentials
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Regression")]
        [Category("Negative")]
        [Description("Invalid credentials should show error message")]
        public async Task VerifyInvalidLoginShowsError()
        {
            ExtentReportManager.LogStep("TC03 — Invalid credentials test");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");
            await loginPage.EnterUsernameAsync("invalid@example.com");
            await loginPage.EnterPasswordAsync("wrongpassword123");
            await loginPage.ClickLoginButtonAsync();

            // Wait for error to appear
            await WaitUtil.WaitUntilAsync(
                async () => await loginPage.IsErrorDisplayedAsync(),
                "Error message should appear after invalid login", 10_000, 500);

            var ss = await loginPage.CaptureScreenshotAsync("invalid_login_error");
            ExtentReportManager.AttachScreenshot(ss, "Invalid Login Error");

            Assert.That(await loginPage.IsErrorDisplayedAsync(),
                "Error message should be displayed for invalid credentials");

            var errorText = await loginPage.GetErrorMessageAsync();
            Log.Information("Error displayed: '{error}'", errorText);
            ExtentReportManager.Info($"Error message: {errorText}");

            // Should still be on login page
            Assert.That(loginPage.GetCurrentUrl(), Does.Contain("/login"),
                "User should remain on login page after failed login");

            ExtentReportManager.LogStep("✅ TC03 PASSED: Error message shown for invalid credentials");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC04 — Empty credentials
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Regression")]
        [Category("Negative")]
        [Description("Empty credentials should prevent form submission")]
        public async Task VerifyEmptyCredentialsValidation()
        {
            ExtentReportManager.LogStep("TC04 — Empty credentials validation");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");
            await loginPage.ClickLoginButtonAsync();  // Submit with empty fields

            // Either validation error or still on login page
            bool onLoginPage = loginPage.GetCurrentUrl().Contains("/login");
            bool errorVisible = await loginPage.IsErrorDisplayedAsync();
            bool usernameInvalid = await VisibilityValidator.IsVisibleAsync(GetPage(),
                "input:invalid, .field-error, [aria-invalid='true']");

            var ss = await loginPage.CaptureScreenshotAsync("empty_credentials_validation");
            ExtentReportManager.AttachScreenshot(ss, "Empty Credentials Validation");

            Assert.That(onLoginPage || errorVisible || usernameInvalid, Is.True,
                "Form should not submit with empty credentials");

            ExtentReportManager.LogStep("✅ TC04 PASSED: Empty credential validation works");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC05 — Logout flow
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Regression")]
        [Category("Smoke")]
        [Description("User should be able to logout and return to login page")]
        public async Task VerifyLogoutFlow()
        {
            ExtentReportManager.LogStep("TC05 — Logout flow test");

            // Login first
            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");
            var dashboard = await loginPage.LoginAsAsync(Config.Username, Config.Password);
            await dashboard.WaitForLoadAsync();

            var dashSS = await dashboard.CaptureScreenshotAsync("dashboard_before_logout");
            ExtentReportManager.AttachScreenshot(dashSS, "Dashboard before logout");

            // Logout
            var logoutLoginPage = await dashboard.LogoutAsync();
            await WaitUtil.WaitForPageLoadAsync(GetPage());

            var logoutSS = await logoutLoginPage.CaptureScreenshotAsync("after_logout");
            ExtentReportManager.AttachScreenshot(logoutSS, "After Logout");

            // Should be back at login or home
            string urlAfterLogout = logoutLoginPage.GetCurrentUrl();
            Log.Information("URL after logout: {url}", urlAfterLogout);
            ExtentReportManager.Info($"URL after logout: {urlAfterLogout}");

            bool backAtLogin = urlAfterLogout.Contains("/login")
                            || urlAfterLogout.Contains("/home")
                            || urlAfterLogout == Config.BaseUrl + "/";

            Assert.That(backAtLogin, Is.True,
                $"After logout should redirect to login or home page. Actual: {urlAfterLogout}");

            ExtentReportManager.LogStep("✅ TC05 PASSED: Logout flow completed successfully");
        }

        // ────────────────────────────────────────────────────────────────────────
        //  TC06 — Dashboard post-login visibility (detailed)
        // ────────────────────────────────────────────────────────────────────────
        [Test]
        [Category("Regression")]
        [Category("Visibility")]
        [Description("Dashboard should display all key UI components after login")]
        public async Task VerifyDashboardVisibilityAfterLogin()
        {
            ExtentReportManager.LogStep("TC06 — Post-login dashboard visibility test");

            var loginPage = new LoginPage();
            await loginPage.OpenAsync(Config.BaseUrl + "/login");
            var dashboard = await loginPage.LoginAsAsync(Config.Username, Config.Password);
            await dashboard.WaitForLoadAsync();

            // Full visibility check
            await dashboard.AssertDashboardElementsVisibleAsync();
            await dashboard.AssertOptionalElementsVisibleAsync();

            // In-viewport check for main content
            bool mainContentInView = await VisibilityValidator.IsInViewportAsync(
                GetPage(), "main, .main-content, [data-testid='main-content']");

            ExtentReportManager.Info($"Main content in viewport: {mainContentInView}");

            var ss = await dashboard.CaptureScreenshotAsync("dashboard_visibility_full");
            ExtentReportManager.AttachScreenshot(ss, "Dashboard Full Visibility");

            Assert.That(await dashboard.IsUserLoggedInAsync(),
                "User should appear as logged in on dashboard");

            ExtentReportManager.LogStep("✅ TC06 PASSED: Dashboard visibility after login verified");
        }
    }
}
