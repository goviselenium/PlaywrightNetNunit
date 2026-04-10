# Quick Start Guide

## 🚀 Get Up and Running in 5 Minutes

### 1. Prerequisites
- Install **.NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- Install a **code editor**: VS Code, Visual Studio 2022, or JetBrains Rider

### 2. Clone/Open the Project
```bash
cd d:\playwrightNET\PlayWrightNETNunit
```

### 3. Install Playwright Browsers
```bash
# Windows
pwsh -Command "& {$(iwr https://aka.ms/playwright-cli -UseBasicParsing).Content | pwsh -Command '[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; & ([scriptblock]::Create((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/microsoft/playwright/main/utils/install.ps1')))' -InstallPlaywrightBrowsers}"

# Or simply:
dotnet build
# This will trigger automatic browser installation on first run
```

### 4. Build the Project
```bash
dotnet build
```

### 5. Run Tests

**All tests:**
```bash
dotnet test Tests/Tests.csproj
```

**Specific category (e.g., Smoke tests):**
```bash
dotnet test Tests/Tests.csproj -- --where="cat==Smoke"
```

**With specific browser:**
```bash
$env:BROWSER="firefox"
dotnet test Tests/Tests.csproj
```

### 6. View Results

After tests complete:
- **HTML Report**: Open `test-output/extent-reports/<timestamp>/index.html`
- **Screenshots**: Check `screenshots/` folder
- **Videos**: Check `videos/` folder
- **Logs**: Check `logs/` folder

## 📝 Running in Different Environments

### QA (Default)
```bash
# Automatic (uses qa.json)
dotnet test Tests/Tests.csproj
```

### Staging
```bash
$env:ENV="staging"
dotnet test Tests/Tests.csproj
```

### With Different Browser
```bash
# Firefox
$env:BROWSER="firefox"

# Safari (WebKit)
$env:BROWSER="webkit"

dotnet test Tests/Tests.csproj
```

### Headless Mode
```bash
$env:HEADLESS="true"
dotnet test Tests/Tests.csproj
```

## 🎯 Common Commands

```bash
# Build only (don't run tests)
dotnet build

# Clean build
dotnet clean && dotnet build

# Run tests with verbose output
dotnet test Tests/Tests.csproj --verbosity=normal

# Run only Visibility tests
dotnet test Tests/Tests.csproj -- --where="cat==Visibility"

# Run tests in Release mode
dotnet build --configuration Release
dotnet test Tests/Tests.csproj --configuration Release
```

## 🧪 Create Your First Test

### 1. Create a Page Object
File: `PlaywrightFramework/Pages/MyPage.cs`
```csharp
using PlaywrightFramework.Base;

namespace PlaywrightFramework.Pages
{
    public class MyPage : BasePage
    {
        private const string MyButton = "button#my-button";

        public async Task<MyPage> ClickMyButtonAsync()
        {
            await ClickAsync(MyButton);
            return this;
        }
    }
}
```

### 2. Create a Test
File: `Tests/MyTest.cs`
```csharp
using NUnit.Framework;
using PlaywrightFramework.Base;
using PlaywrightFramework.Pages;

namespace PlaywrightFramework.Tests
{
    [TestFixture]
    public class MyTest : BaseTest
    {
        [Test]
        [Category("Smoke")]
        public async Task MyFirstTest()
        {
            var myPage = new MyPage();
            await myPage.ClickMyButtonAsync();
        }
    }
}
```

### 3. Run Your Test
```bash
dotnet test Tests/Tests.csproj
```

## 🔧 Configure URLS and Credentials

Edit: `Tests/Resources/Config/qa.json`

```json
{
  "environment": "QA",
  "baseUrl": "https://your-app.com",
  "username": "your_username",
  "password": "your_password",
  "defaultTimeout": 30000,
  "navigationTimeout": 60000
}
```

## 🐛 Troubleshooting

### "Playwright executable not found"
```bash
# Install browsers
dotnet build
```

### "Config file not found"
- Ensure files exist: `Tests/Resources/Config/qa.json` and `staging.json`
- Built files will copy config to `bin/Debug`/`bin/Release`

### "Unable to find element"
- Tests run headless by default (screen not visible)
- Add screenshots to debug:
```csharp
await page.CaptureScreenshotAsync("debug");
```
- Or disable headless:
```bash
$env:HEADLESS="false"
dotnet test
```

## 📚 Next Steps

1. Read the full [README.md](README.md)
2. Explore example tests in `Tests/LoginTest.cs`
3. Check out page objects in `PlaywrightFramework/Pages/`
4. Review base classes in `PlaywrightFramework/Base/`

## 🎓 Key Concepts

- **Page Objects**: All pages extend `BasePage` (see `LoginPage.cs`)
- **Tests**: All tests extend `BaseTest` (see `LoginTest.cs`)
- **Config**: Environment-specific settings in JSON files
- **Reports**: Extent Reports with screenshots/videos
- **Async**: All Playwright calls are async (must use `await`)

## 💡 Tips

- Use `Category` attribute to tag tests for filtering
- Use `Description` attribute for test documentation
- Page objects return `this` for method chaining
- Use `ExtentReportManager.LogStep()` to log test steps
- Check logs in `logs/` folder for detailed execution info

## 🆘 Need Help?

- Check [Playwright .NET Docs](https://playwright.dev/dotnet/)
- Check [NUnit Docs](https://docs.nunit.org/)
- Review example tests in `Tests/LoginTest.cs`
- Check logs in `logs/` folder

Happy testing! 🎉
