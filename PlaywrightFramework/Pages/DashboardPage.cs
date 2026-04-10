using PlaywrightFramework.Base;
using PlaywrightFramework.Reports;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Page Object for the Dashboard page (post-login landing page)
    /// </summary>
    public class DashboardPage : BasePage
    {
        // ── Selectors ────────────────────────────────────────────────────────────
        private const string NavMenu = "nav, .navbar, .sidebar, [data-testid='nav']";
        private const string UserAvatar = ".user-avatar, .profile-icon, [data-testid='user-avatar']";
        private const string WelcomeMessage = ".welcome, h1, [data-testid='welcome']";
        private const string LogoutLink = "a:has-text('Logout'), a:has-text('Sign out'), button:has-text('Logout')";
        private const string DashboardHeader = ".dashboard-header, header, [data-testid='dashboard-header']";
        private const string MainContent = "main, .main-content, [data-testid='main-content']";
        private const string LoadingSpinner = ".spinner, .loading, [data-testid='loading']";
        private const string NotificationBell = ".notification, [data-testid='notification-bell']";
        private const string SearchBar = "input[type='search'], .search-input, [data-testid='search']";

        // ── Loaded Check ─────────────────────────────────────────────────────────

        public async Task<DashboardPage> WaitForLoadAsync()
        {
            Log.Information("Waiting for dashboard to fully load");
            await WaitForHiddenAsync(LoadingSpinner);
            await WaitForVisibleAsync(MainContent);
            await WaitForPageLoadAsync();
            ExtentReportManager.LogStep("Dashboard loaded successfully");
            return this;
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        public async Task<LoginPage> LogoutAsync()
        {
            ExtentReportManager.LogStep("Logging out");
            Log.Information("Clicking logout");
            await ClickAsync(LogoutLink);
            await WaitForPageLoadAsync();
            return new LoginPage();
        }

        public async Task<DashboardPage> SearchAsync(string query)
        {
            ExtentReportManager.LogStep($"Searching for: {query}");
            await TypeAsync(SearchBar, query);
            await PressKeyAsync(SearchBar, "Enter");
            await WaitForNetworkIdleAsync();
            return this;
        }

        // ── Visibility Assertions ────────────────────────────────────────────────

        /// <summary>
        /// Comprehensive dashboard element visibility check.
        /// Reports pass/fail per element in Extent Report.
        /// </summary>
        public async Task<DashboardPage> AssertDashboardElementsVisibleAsync()
        {
            Log.Information("Running dashboard visibility assertions");
            ExtentReportManager.LogStep("Verifying dashboard element visibility");

            var required = new Dictionary<string, string>
            {
                { "Navigation menu", NavMenu },
                { "Dashboard header", DashboardHeader },
                { "Main content area", MainContent },
                { "Logout link", LogoutLink }
            };

            var report = await VisibilityValidator.CheckAllAsync(Page, required);
            ExtentReportManager.Info(report.ToString());

            if (!report.AllPassed)
            {
                var ss = await CaptureScreenshotAsync("dashboard_visibility_fail");
                ExtentReportManager.AttachScreenshot(ss, "Dashboard Visibility Failures");
                throw new AssertionException($"Dashboard visibility FAILED:\n{report.FailureSummary()}");
            }

            ExtentReportManager.LogStep("✅ All dashboard elements visible");
            return this;
        }

        /// <summary>
        /// Optional elements — logs warnings but does NOT throw
        /// </summary>
        public async Task<DashboardPage> AssertOptionalElementsVisibleAsync()
        {
            ExtentReportManager.LogStep("Checking optional dashboard elements");

            var optional = new Dictionary<string, string>
            {
                { "User avatar", UserAvatar },
                { "Notification bell", NotificationBell },
                { "Search bar", SearchBar }
            };

            var report = await VisibilityValidator.CheckAllAsync(Page, optional);
            ExtentReportManager.Info($"Optional elements: {report}");

            if (!report.AllPassed)
            {
                ExtentReportManager.LogStep(
                    $"{report.Failed.Count} optional elements not found:\n{report.FailureSummary()}");
            }

            return this;
        }

        // ── Getters ──────────────────────────────────────────────────────────────

        public async Task<string> GetWelcomeMessageAsync()
        {
            if (await IsVisibleAsync(WelcomeMessage))
                return await GetTextAsync(WelcomeMessage);
            return string.Empty;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            return (await IsVisibleAsync(NavMenu)) || (await IsVisibleAsync(UserAvatar));
        }

        public async Task<bool> IsSearchBarVisibleAsync()
        {
            return await IsVisibleAsync(SearchBar);
        }
    }
}
