using PlaywrightFramework.Base;
using PlaywrightFramework.Reports;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Page Object for the Login page
    /// </summary>
    public class LoginPage : BasePage
    {
        // ── Selectors ────────────────────────────────────────────────────────────
        private const string UsernameInput = "#username, input[name='username'], input[type='email']";
        private const string PasswordInput = "#password, input[name='password'], input[type='password']";
        private const string LoginButton = "button[type='submit'], #login-btn, button:has-text('Login'), button:has-text('Sign in')";
        private const string ErrorMessage = ".error-message, .alert-danger, [data-testid='error-msg']";
        private const string RememberMe = "#rememberMe, input[name='rememberMe']";
        private const string ForgotPassword = "a:has-text('Forgot'), a:has-text('Reset Password')";
        private const string PageHeading = "h1, .login-title, [data-testid='login-heading']";
        private const string Logo = ".logo, img[alt*='logo'], header img";

        // ── Actions ──────────────────────────────────────────────────────────────

        public async Task<LoginPage> OpenAsync(string url)
        {
            ExtentReportManager.LogStep($"Opening login URL: {url}");
            await NavigateToAsync(url);
            await WaitForVisibleAsync(UsernameInput);
            return this;
        }

        public async Task<LoginPage> EnterUsernameAsync(string username)
        {
            ExtentReportManager.LogStep($"Entering username: {username}");
            await TypeAsync(UsernameInput, username);
            return this;
        }

        public async Task<LoginPage> EnterPasswordAsync(string password)
        {
            ExtentReportManager.LogStep("Entering password");
            await TypeAsync(PasswordInput, password);
            return this;
        }

        public async Task<LoginPage> ClickLoginButtonAsync()
        {
            ExtentReportManager.LogStep("Clicking Login button");
            await ClickAsync(LoginButton);
            return this;
        }

        public async Task<DashboardPage> LoginAsAsync(string username, string password)
        {
            Log.Information("Logging in as: {user}", username);
            await EnterUsernameAsync(username);
            await EnterPasswordAsync(password);
            await ClickLoginButtonAsync();
            return new DashboardPage();
        }

        public async Task<LoginPage> CheckRememberMeAsync()
        {
            ExtentReportManager.LogStep("Checking 'Remember me'");
            var locator = Page.Locator(RememberMe);
            if (!(await locator.IsCheckedAsync()))
            {
                await ClickAsync(RememberMe);
            }
            return this;
        }

        // ── Visibility checks ────────────────────────────────────────────────────

        /// <summary>
        /// Asserts that all mandatory login-page elements are rendered correctly
        /// </summary>
        public async Task<LoginPage> AssertLoginPageElementsAsync()
        {
            Log.Information("Running login page visibility checks");
            ExtentReportManager.LogStep("Verifying login page element visibility");

            var elements = new Dictionary<string, string>
            {
                { "Username input", UsernameInput },
                { "Password input", PasswordInput },
                { "Login button", LoginButton },
                { "Page logo", Logo }
            };

            var report = await VisibilityValidator.CheckAllAsync(Page, elements);
            ExtentReportManager.Info(report.ToString());

            if (!report.AllPassed)
            {
                var ss = await CaptureScreenshotAsync("login_visibility_fail");
                ExtentReportManager.AttachScreenshot(ss, "Login Visibility Failures");
                throw new AssertionException($"Login page visibility check failed:\n{report.FailureSummary()}");
            }

            ExtentReportManager.LogStep("✅ All login page elements are visible");
            return this;
        }

        // ── Assertions ───────────────────────────────────────────────────────────

        public async Task<bool> IsErrorDisplayedAsync()
        {
            return await IsVisibleAsync(ErrorMessage);
        }

        public async Task<string> GetErrorMessageAsync()
        {
            if (await IsErrorDisplayedAsync())
                return await GetTextAsync(ErrorMessage);
            return string.Empty;
        }

        public async Task<bool> IsForgotPasswordLinkVisibleAsync()
        {
            return await IsVisibleAsync(ForgotPassword);
        }

        public async Task<string> GetHeadingTextAsync()
        {
            if (await IsVisibleAsync(PageHeading))
                return await GetTextAsync(PageHeading);
            return string.Empty;
        }

        public async Task<LoginPage> AssertLoginFormVisibleAsync()
        {
            await AssertVisibleAsync(UsernameInput, "Username field");
            await AssertVisibleAsync(PasswordInput, "Password field");
            await AssertVisibleAsync(LoginButton, "Login button");
            return this;
        }

        public async Task<LoginPage> AssertErrorVisibleAsync(string expectedMessage)
        {
            await AssertVisibleAsync(ErrorMessage, "Error message");
            var actual = await GetErrorMessageAsync();
            if (!actual.Contains(expectedMessage))
            {
                throw new AssertionException(
                    $"Expected error: '{expectedMessage}' but got: '{actual}'");
            }
            ExtentReportManager.LogStep($"Error message verified: {actual}");
            return this;
        }
    }
}
