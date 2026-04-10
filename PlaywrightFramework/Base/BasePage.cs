using Microsoft.Playwright;
using Serilog;
using PlaywrightFramework.Config;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Base
{
    /// <summary>
    /// Base class for all Page Objects.
    ///
    /// Provides:
    ///  - Smart wait wrappers (WaitForVisibleAsync, WaitForClickableAsync, WaitForHiddenAsync)
    ///  - Safe action methods with logging
    ///  - Visibility assertions
    ///  - Screenshot on demand
    /// </summary>
    public abstract class BasePage
    {
        protected readonly ILogger Log;
        protected readonly IPage Page;
        protected readonly int Timeout;

        protected BasePage()
        {
            Log = Serilog.Log.ForContext(GetType());
            var page = PlaywrightManager.GetPage();
            if (page == null)
                throw new InvalidOperationException("Page is not initialized. Call PlaywrightManager.InitAsync() first.");
            
            Page = page;
            Timeout = ConfigManager.Instance.DefaultTimeout;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Navigation
        // ════════════════════════════════════════════════════════════════════════

        public async Task NavigateToAsync(string url)
        {
            Log.Information("Navigating to: {url}", url);
            await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await WaitForPageLoadAsync();
        }

        public async Task WaitForPageLoadAsync()
        {
            Log.Debug("Waiting for page LOAD state");
            await Page.WaitForLoadStateAsync(LoadState.Load);
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            Log.Debug("Page fully loaded: {url}", Page.Url);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Wait Strategies
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Waits until the element is VISIBLE on the page
        /// </summary>
        public async Task<ILocator> WaitForVisibleAsync(string selector)
        {
            Log.Debug("WaitForVisible → {selector}", selector);
            var locator = Page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = Timeout 
            });
            return locator;
        }

        public async Task<ILocator> WaitForVisibleAsync(string selector, int customTimeout)
        {
            Log.Debug("WaitForVisible [{timeout}ms] → {selector}", customTimeout, selector);
            var locator = Page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = customTimeout 
            });
            return locator;
        }

        /// <summary>
        /// Waits until the element is HIDDEN or detached
        /// </summary>
        public async Task WaitForHiddenAsync(string selector)
        {
            Log.Debug("WaitForHidden → {selector}", selector);
            await Page.Locator(selector).WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Hidden,
                Timeout = Timeout 
            });
        }

        /// <summary>
        /// Waits until the element is ATTACHED to DOM
        /// </summary>
        public async Task<ILocator> WaitForAttachedAsync(string selector)
        {
            Log.Debug("WaitForAttached → {selector}", selector);
            var locator = Page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Attached,
                Timeout = Timeout 
            });
            return locator;
        }

        /// <summary>
        /// Waits for network to be idle
        /// </summary>
        public async Task WaitForNetworkIdleAsync()
        {
            Log.Debug("Waiting for network idle");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Actions
        // ════════════════════════════════════════════════════════════════════════

        public async Task ClickAsync(string selector)
        {
            Log.Information("Clicking element: {selector}", selector);
            var locator = await WaitForVisibleAsync(selector);
            await locator.ClickAsync();
        }

        public async Task ClickWithForceAsync(string selector)
        {
            Log.Information("Force-clicking element: {selector}", selector);
            await Page.Locator(selector).ClickAsync(new LocatorClickOptions { Force = true });
        }

        public async Task TypeAsync(string selector, string text)
        {
            Log.Information("Typing '{text}' into: {selector}", text, selector);
            var el = await WaitForVisibleAsync(selector);
            await el.ClearAsync();
            await el.FillAsync(text);
        }

        public async Task SelectDropdownAsync(string selector, string value)
        {
            Log.Information("Selecting '{value}' in dropdown: {selector}", value, selector);
            var locator = await WaitForVisibleAsync(selector);
            await locator.SelectOptionAsync(value);
        }

        public async Task PressKeyAsync(string selector, string key)
        {
            Log.Information("Pressing key '{key}' on: {selector}", key, selector);
            var locator = await WaitForVisibleAsync(selector);
            await locator.PressAsync(key);
        }

        public async Task ScrollIntoViewAsync(string selector)
        {
            Log.Debug("Scrolling into view: {selector}", selector);
            await Page.Locator(selector).ScrollIntoViewIfNeededAsync();
        }

        public async Task HoverAsync(string selector)
        {
            Log.Debug("Hovering over: {selector}", selector);
            var locator = await WaitForVisibleAsync(selector);
            await locator.HoverAsync();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Getters
        // ════════════════════════════════════════════════════════════════════════

        public async Task<string> GetTextAsync(string selector)
        {
            var text = await (await WaitForVisibleAsync(selector)).TextContentAsync();
            var result = text?.Trim() ?? string.Empty;
            Log.Debug("GetText({selector}) → '{text}'", selector, result);
            return result;
        }

        public async Task<string?> GetAttributeAsync(string selector, string attribute)
        {
            var val = await (await WaitForVisibleAsync(selector)).GetAttributeAsync(attribute);
            Log.Debug("GetAttribute({selector}, {attr}) → '{val}'", selector, attribute, val);
            return val;
        }

        public async Task<string> GetInputValueAsync(string selector)
        {
            return await (await WaitForVisibleAsync(selector)).InputValueAsync();
        }

        public string GetCurrentUrl() => Page.Url;

        public string GetTitle() => Page.Title;

        // ════════════════════════════════════════════════════════════════════════
        //  Visibility Checks
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns true if element is visible right now (no wait)
        /// </summary>
        public async Task<bool> IsVisibleAsync(string selector)
        {
            var visible = await Page.Locator(selector).IsVisibleAsync();
            Log.Debug("IsVisible({selector}) → {visible}", selector, visible);
            return visible;
        }

        /// <summary>
        /// Asserts element is visible; throws if not
        /// </summary>
        public async Task AssertVisibleAsync(string selector, string elementDescription)
        {
            Log.Information("Asserting visibility of '{desc}' ({selector})", elementDescription, selector);
            try
            {
                await WaitForVisibleAsync(selector);
                Log.Information("✅ Element VISIBLE: {desc}", elementDescription);
            }
            catch (Exception ex)
            {
                var msg = $"❌ Element NOT VISIBLE: {elementDescription} | Selector: {selector}";
                Log.Error(msg);
                await ScreenshotUtil.TakeScreenshotAsync(Page, $"visibility_fail_{elementDescription.Replace(" ", "_")}");
                throw new AssertionException(msg, ex);
            }
        }

        /// <summary>
        /// Asserts element is NOT visible; throws if it is
        /// </summary>
        public async Task AssertNotVisibleAsync(string selector, string elementDescription)
        {
            Log.Information("Asserting NOT visible: '{desc}' ({selector})", elementDescription, selector);
            var visible = await IsVisibleAsync(selector);
            if (visible)
            {
                var msg = $"❌ Element SHOULD NOT be visible: {elementDescription}";
                Log.Error(msg);
                throw new AssertionException(msg);
            }
            Log.Information("✅ Element correctly not visible: {desc}", elementDescription);
        }

        /// <summary>
        /// Returns all matching visible elements
        /// </summary>
        public async Task<List<ILocator>> GetAllVisibleAsync(string selector)
        {
            var locators = await Page.Locator(selector).AllAsync();
            var visibleLocators = new List<ILocator>();
            
            foreach (var locator in locators)
            {
                if (await locator.IsVisibleAsync())
                    visibleLocators.Add(locator);
            }

            return visibleLocators;
        }

        public async Task<int> CountVisibleAsync(string selector)
        {
            var visible = await GetAllVisibleAsync(selector);
            return visible.Count;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  Screenshot
        // ════════════════════════════════════════════════════════════════════════

        public async Task<byte[]> CaptureScreenshotAsync(string name)
        {
            return await ScreenshotUtil.TakeScreenshotAsync(Page, name);
        }
    }
}
