# Playwright .NET NUnit Automation Framework

A robust, production-grade test automation framework built with Playwright, C#, NUnit, Extent Reports, and the Page Object Model (POM) pattern.

Converted from the original Java/TestNG project: [goviselenium/playwrightJavaPOMs](https://github.com/goviselenium/playwrightJavaPOMs)

## 📁 Project Structure

```
PlaywrightFramework/
├── PlaywrightFramework.sln                    ← Solution file
├── .runsettings                               ← Test execution settings
│
├── PlaywrightFramework/                       ← Framework library
│   ├── PlaywrightFramework.csproj
│   ├── Base/
│   │   ├── BasePage.cs                        ← All page objects extend this
│   │   ├── BaseTest.cs                        ← All test classes extend this
│   │   ├── BrowserFactory.cs                  ← Browser/context creation
│   │   └── PlaywrightManager.cs               ← ThreadLocal Playwright resources
│   ├── Config/
│   │   ├── ConfigManager.cs                   ← Singleton config loader
│   │   ├── EnvironmentConfig.cs               ← POJO for JSON config
│   │   └── LoggerConfiguration.cs             ← Serilog setup
│   ├── Pages/
│   │   ├── LoginPage.cs
│   │   └── DashboardPage.cs
│   ├── Reports/
│   │   └── ExtentReportManager.cs             ← HTML report generation
│   └── Utils/
│       ├── ScreenshotUtil.cs                  ← Full-page screenshots
│       ├── VideoUtil.cs                       ← Video path resolution
│       ├── VisibilityValidator.cs             ← Element visibility toolkit
│       └── WaitUtil.cs                        ← Custom wait strategies
│
├── Tests/                                     ← Test project
│   ├── Tests.csproj
│   ├── AssemblySetup.cs                       ← Assembly-level setup
│   ├── LoginTest.cs                           ← Login test suite
│   ├── Resources/
│   │   ├── Config/
│   │   │   ├── qa.json                        ← QA environment config
│   │   │   └── staging.json                   ← Staging environment config
│   │   └── Extent/
│   │       └── extent-config.xml              ← Report theme config
│
├── screenshots/                               ← Auto-generated
├── videos/                                    ← Auto-generated
├── logs/                                      ← Auto-generated
└── test-output/extent-reports/                ← Auto-generated HTML reports
```

## 🚀 Prerequisites

- **.NET 8 SDK** or higher
- **Visual Studio Code** or **Visual Studio 2022+**
- **Playwright** (installed automatically via NuGet)

Playwright downloads its own browser binaries automatically on first run.

## ⚡ Running Tests

### Using .NET CLI

```bash
# QA environment (default)
dotnet test Tests/Tests.csproj

# Staging environment
dotnet test Tests/Tests.csproj -- --configuration=Release
```

### Using Environment Variables

```bash
# Windows PowerShell
$env:ENV="qa"
$env:BROWSER="chromium"
$env:HEADLESS="false"
dotenv test Tests/Tests.csproj

# Windows Command Prompt
set ENV=staging
set BROWSER=firefox
set HEADLESS=true
dotnet test Tests/Tests.csproj

# Linux/Mac
export ENV=qa
export BROWSER=webkit
export HEADLESS=false
dotnet test Tests/Tests.csproj
```

### Using Visual Studio Test Explorer

1. Open `PlaywrightFramework.sln` in Visual Studio
2. Open **Test Explorer** (Test > Test Explorer)
3. Select tests by category or run all
4. Click **Run** or **Run Selected**

### Using NUnit Console (if installed)

```bash
nunit3-console Tests/Tests.csproj --where="cat==Smoke"
```

## ⚙️ Configuration

Edit `Tests/Resources/Config/{env}.json` to change environment settings:

### QA Configuration (`qa.json`)
```json
{
  "environment": "QA",
  "baseUrl": "https://qa.example.com",
  "username": "qa_user@example.com",
  "password": "QaPassword@123",
  "defaultTimeout": 30000,
  "navigationTimeout": 60000,
  "apiBaseUrl": "https://api.qa.example.com",
  "db": {
    "host": "qa-db.example.com",
    "port": 5432,
    "name": "qa_db"
  }
}
```

### Environment Variables

| Variable | Default | Options |
|----------|---------|---------|
| `ENV` | qa | qa, staging |
| `BROWSER` | chromium | chromium, firefox, webkit |
| `HEADLESS` | false | true, false |
| `RETRY_COUNT` | 2 | 0-5 |

## 🔑 Key Features

### ✅ Dual Environment Support
- Config-driven via JSON files per environment
- Switch via environment variables
- Simple to add new environments

### ✅ Extent Reports (v5)
- One time-stamped HTML report per run: `test-output/extent-reports/<timestamp>/`
- Embedded screenshots on failure (base64 inline)
- Video link attached on failure
- System info (env, browser, URL, OS) in header

### ✅ Screenshots
- Automatic full-page screenshot on test failure
- Embedded in Extent Report as base64
- Saved to disk: `screenshots/`

### ✅ Video Recording
- Every test context records video automatically
- Saved to `videos/<thread-name>/`
- Path linked in Extent Report on failure

### ✅ Logging (Serilog)
- Colored console output
- Rolling file logs: `logs/`
- Per-run log with timestamp

### ✅ Visibility Testing
- `VisibilityValidator` — bulk element checks with pass/fail report
- CSS computed style checks
- Viewport/in-view detection via JavaScript
- In-group element counts

### ✅ Smart Waits
- `WaitForVisibleAsync` / `WaitForHiddenAsync` / `WaitForAttachedAsync`
- `WaitForPageLoadAsync` / `WaitForNetworkIdleAsync`
- Custom polling: `WaitUtil.WaitUntilAsync(condition, description)`

### ✅ Thread Safety
- `PlaywrightManager` uses `ThreadLocal` — safe for parallel test execution
- `ExtentReportManager` uses `ThreadLocal<ExtentTest>` per thread

### ✅ Page Object Model (POM)
- Centralized selectors
- Fluent method chaining for better readability
- Base class provides all common Playwright operations

## 🧪 Test Categories

| Category | Purpose |
|----------|---------|
| `Smoke` | Quick sanity checks |
| `Regression` | Full regression suite |
| `Visibility` | UI element visibility tests |
| `Negative` | Invalid input / error cases |

### Run a Specific Category

```bash
# Using NUnit filter
dotnet test Tests/Tests.csproj -- --where="cat==Smoke"

# Using Visual Studio Test Explorer
# Select category from dropdown
```

## 🔧 Adding a New Page Object

```csharp
using PlaywrightFramework.Base;
using PlaywrightFramework.Reports;

namespace PlaywrightFramework.Pages
{
    public class MyPage : BasePage
    {
        private const string MyElement = "[data-testid='my-element']";

        public async Task<MyPage> DoSomethingAsync()
        {
            ExtentReportManager.LogStep("Doing something");
            await ClickAsync(MyElement);
            return this;
        }

        public async Task AssertMyElementAsync()
        {
            await AssertVisibleAsync(MyElement, "My Element");
        }
    }
}
```

## 🧪 Adding a New Test

```csharp
using NUnit.Framework;
using PlaywrightFramework.Base;
using PlaywrightFramework.Pages;
using PlaywrightFramework.Reports;

namespace PlaywrightFramework.Tests
{
    [TestFixture]
    [Category("MyTests")]
    public class MyTest : BaseTest
    {
        [Test]
        [Category("Smoke")]
        [Description("My test description")]
        public async Task MyTestMethod()
        {
            ExtentReportManager.LogStep("Starting my test");
            
            var myPage = new MyPage();
            await myPage.DoSomethingAsync();
            await myPage.AssertMyElementAsync();
            
            ExtentReportManager.LogStep("✅ Test completed successfully");
        }
    }
}
```

## 📊 Reports Location

| Output | Location |
|--------|----------|
| HTML Report | `test-output/extent-reports/<timestamp>/` |
| Screenshots | `screenshots/` |
| Videos | `videos/<thread>/` |
| Logs | `logs/` |

## 🏗️ Building the Project

```bash
# Build both projects
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Clean build
dotnet clean
dotnet build
```

## 🔍 Debugging

### Run Tests in Debug Mode

```bash
# Visual Studio
Debug > Debug All Tests

# Or with .NET CLI
dotnet test Tests/Tests.csproj --logger:"console;verbosity=detailed" -c Debug
```

### Enable Verbose Logging

Set environment variable before running tests:
```bash
# Windows
set SERILOG__MINIMUMLEVEL=Debug

# Linux/Mac
export SERILOG__MINIMUMLEVEL=Debug
```

## 📝 Code Style

- **Language:** C# 11+
- **Naming:** PascalCase for public members, camelCase for local variables
- **Async:** All Playwright operations are async (use `await`)
- **Null Safety:** Enabled with nullable reference types

## 🤝 Contributing

1. Create a new branch for your feature
2. Follow the existing code style
3. Add tests for new functionality
4. Update this README if needed
5. Submit a pull request

## 📄 License

This project is open-source under the MIT License.

## 🔗 Original Project

This is a C# conversion of: [goviselenium/playwrightJavaPOMs](https://github.com/goviselenium/playwrightJavaPOMs)

## 🆘 Troubleshooting

### Tests won't run
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`
- Check environment variables are set correctly

### Playwr ight browsers not installed
- Run: `playwright install`
- Or: `dotnet build` (should trigger automatic install)

### Config file not found
- Ensure config files exist in: `Tests/Resources/Config/`
- Check file names match environment variable (qa.json, staging.json)
- Verify `CopyToOutputDirectory` is set in .csproj

### Reports not generated
- Check `test-output/` directory permissions
- Ensure `ExtentReportManager.FlushReport()` is called in teardown (BaseTest handles this)

## 📞 Support

For issues or questions, refer to:
- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Extent Reports Documentation](https://www.extentreports.com/)
