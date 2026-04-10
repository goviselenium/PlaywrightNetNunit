# Commands Reference

Quick reference for common testing commands.

## 🏗️ Build Commands

```bash
# Build the entire solution
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Build without running tests
dotnet build --no-restore

# Clean all build artifacts
dotnet clean

# Rebuild from scratch
dotnet clean && dotnet build
```

## 🧪 Test Execution

### Run All Tests
```bash
dotnet test
dotnet test Tests/Tests.csproj
```

### Run Specific Category
```bash
# Smoke tests only
dotnet test -- --where="cat==Smoke"

# Visibility tests only
dotnet test -- --where="cat==Visibility"

# Regression tests only
dotnet test -- --where="cat==Regression"

# Negative tests only
dotnet test -- --where="cat==Negative"

# Multiple categories (OR logic)
dotnet test -- --where="cat==Smoke || cat==Visibility"
```

### Run Specific Test Class
```bash
dotnet test --filter "ClassName=LoginTest"
```

### Run Specific Test Method
```bash
dotnet test --filter "Name=VerifySuccessfulLogin"
```

### Run Tests in Parallel
```bash
# Default is 2 workers, configure in .runsettings
dotnet test Tests/Tests.csproj
```

## 🌐 Environment Configuration

### Windows PowerShell
```powershell
# Set single environment variable
$env:ENV="qa"

# Set multiple variables
$env:ENV="qa"
$env:BROWSER="chromium"
$env:HEADLESS="false"
$env:RETRY_COUNT="2"

# Run tests
dotnet test

# Clear variables
Remove-Item env:ENV
```

### Windows Command Prompt
```batch
# Set environment variables
set ENV=qa
set BROWSER=firefox
set HEADLESS=true

# Run tests
dotnet test

# Clear variables
set ENV=
```

### Linux/Mac (Bash)
```bash
# Set environment variables
export ENV="qa"
export BROWSER="webkit"
export HEADLESS="false"

# Run tests
dotnet test

# Run with variables inline
ENV=staging BROWSER=firefox dotnet test
```

## 🌍 Environment Variations

### QA Environment (Default)
```bash
dotnet test
```

### Staging Environment
```bash
$env:ENV="staging"
dotnet test
```

### With Firefox Browser
```bash
$env:BROWSER="firefox"
dotnet test
```

### With Safari/WebKit Browser
```bash
$env:BROWSER="webkit"
dotnet test
```

### Headless Mode (CI/CD)
```bash
$env:HEADLESS="true"
dotnet test
```

### With Retry (3 retries)
```bash
$env:RETRY_COUNT="3"
dotnet test
```

### Combined (Staging + Firefox + Headless)
```bash
$env:ENV="staging"; $env:BROWSER="firefox"; $env:HEADLESS="true"; dotnet test
```

## 🎯 Test Filtering

### Category Filters
```bash
# Run only Smoke tests
dotnet test -- --where="cat==Smoke"

# Exclude Negative tests
dotnet test -- --where="cat!=Negative"

# Smoke AND Regression
dotnet test -- --where="(cat==Smoke || cat==Regression)"

# All except Negative
dotnet test -- --where="cat!=Negative"
```

### Name Filters
```bash
# Tests containing "Login"
dotnet test --filter "Name~Login"

# Exact test name
dotnet test --filter "Name=VerifySuccessfulLogin"

# Not containing "Visibility"
dotnet test --filter "Name!~Visibility"
```

### Combination Filters
```bash
# Smoke tests in LoginTest class
dotnet test --filter "TestClass=LoginTest&Category=Smoke"

# All except negative and slow
dotnet test -- --where="cat!=Negative && cat!=Slow"
```

## 📊 Test Reporting

### Run with Detailed Output
```bash
dotnet test Tests/Tests.csproj --verbosity normal
```

### Run with Minimal Output
```bash
dotnet test Tests/Tests.csproj --verbosity quiet
```

### Run with NUnit Console (if installed)
```bash
nunit3-console Tests/Tests.csproj
nunit3-console Tests/Tests.csproj --where="cat==Smoke"
```

### View Test Results
```bash
# After running tests, open report
start test-output/extent-reports/
```

## 🎮 Visual Studio Code

### Run All Tests
```
Ctrl+Shift+T
```

### Run Tests in File
```
Right-click on test file > Run Tests
```

### Run Specific Test
```
Click the "Run" icon above test method
```

### Debug Tests
```
Click the "Debug" icon above test method
```

## 🔧 Development Commands

### Format Code
```bash
dotnet format
```

### Analyze Code Quality
```bash
dotnet build /p:EnforceCodeStyleInBuild=true
```

### Restore Dependencies
```bash
dotnet restore
```

### List Installed Packages
```bash
dotnet list package

# Include pre-release versions
dotnet list package --include-prerelease
```

### Update Packages
```bash
dotnet package update
```

### Install Playwright Browsers
```bash
# Automatic (on first build or test run)
dotnet build

# Explicit install
playwright install
```

## 📁 File & Folder Commands

### Create New Page Object
```bash
New-Item PlaywrightFramework/Pages/MyPage.cs
```

### Create New Test
```bash
New-Item Tests/MyTest.cs
```

### Clean Output Directories
```bash
Remove-Item -Recurse screenshots/
Remove-Item -Recurse videos/
Remove-Item -Recurse logs/
Remove-Item -Recurse test-output/
```

## 🐛 Debugging Commands

### Run Tests in Debug Mode
```bash
dotnet test --configuration Debug
```

### Set Breakpoint and Run
1. Add breakpoint in code
2. Visual Studio: Debug > Debug All Tests
3. VS Code: Run > Start Debugging

### Run Single Test for Debugging
```bash
dotnet test --filter "Name=VerifySuccessfulLogin" --configuration Debug
```

### Enable Verbose Logging
```bash
$env:SERILOG__MINIMUMLEVEL="Debug"
dotnet test
```

## 🚀 CI/CD Commands

### Azure Pipelines
```bash
dotnet build
dotnet test Tests/Tests.csproj --logger trx --results-directory $(Agent.TempDirectory)
```

### GitHub Actions
```bash
dotnet build
dotnet test Tests/Tests.csproj --verbosity normal
```

### Jenkins
```bash
#!/bin/bash
dotnet build
dotnet test Tests/Tests.csproj
```

## 📦 NuGet Commands

### Add Package
```bash
dotnet package add <PackageName>
dotnet package add Newtonsoft.Json
```

### Remove Package
```bash
dotnet package remove <PackageName>
```

### Update Package
```bash
dotnet package update <PackageName>
dotnet package update Microsoft.Playwright
```

## 🎯 Common Commands Combinations

### Full CI/CD Build & Test
```bash
dotnet clean
dotnet build --configuration Release
dotnet test Tests/Tests.csproj --configuration Release --verbosity normal
```

### Quick Development Test
```bash
$env:HEADLESS="false"
dotnet test -- --where="cat==Smoke"
```

### Full Regression Suite
```bash
$env:ENV="qa"
$env:RETRY_COUNT="2"
dotnet test Tests/Tests.csproj
```

### Staging Verification
```bash
$env:ENV="staging"
$env:HEADLESS="true"
dotnet test -- --where="cat==Smoke"
```

### Performance Testing
```bash
dotnet test Tests/Tests.csproj --configuration Release --verbosity detailed
```

## 💾 Save & Re-run Failed Tests

### Run Last Failed Tests (PowerShell)
```powershell
$env:ENV="qa"; dotnet test
# Fix code...
dotnet test --filter "Name~FailedTestName"
```

## 🎓 Learning

### View Help
```bash
dotnet test --help
dotnet build --help
dotnet clean --help
```

### Check .NET Version
```bash
dotnet --version
```

### List Global Tools
```bash
dotnet tool list -g
```

## ✨ Useful Aliases

### PowerShell Profile
```powershell
# Add to profile ($PROFILE)
function dbuild { dotnet build }
function dtest { dotnet test }
function drun { dotnet run }
function dqa { $env:ENV="qa"; dotnet test }
function dstaging { $env:ENV="staging"; dotnet test }
function dsmoke { dotnet test -- --where="cat==Smoke" }
function dvis { dotnet test -- --where="cat==Visibility" }
```

### Bash/Zsh
```bash
# Add to ~/.bashrc or ~/.zshrc
alias dbuild='dotnet build'
alias dtest='dotnet test'
alias dqa='ENV=qa dotnet test'
alias dstaging='ENV=staging dotnet test'
alias dsmoke='dotnet test -- --where="cat==Smoke"'
```

### Git Hooks (pre-commit)
```bash
#!/bin/bash
# .git/hooks/pre-commit
dotnet build
if [ $? -ne 0 ]; then exit 1; fi
dotnet test -- --where="cat==Smoke"
if [ $? -ne 0 ]; then exit 1; fi
```
