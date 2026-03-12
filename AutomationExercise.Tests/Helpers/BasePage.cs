using Microsoft.Playwright;

namespace AutomationExercise.Tests.Helpers;
public abstract class BasePage
{
    protected readonly IPage Page;
    protected abstract string PagePath { get; }
    private static readonly (string Overlay, string CloseButton)[] KnownOverlays =
    [
        (".fc-consent-root",          ".fc-cta-consent"),
        ("#gdpr-banner-wrapper",      "#gdpr-banner-accept"),
        ("#gdpr-consent-tool-wrapper","[data-qa='gdpr-close']")
    ];

    protected BasePage(IPage page)
    {
        Page = page;
        Page.SetDefaultTimeout(TestSettings.DefaultTimeout);
        Page.SetDefaultNavigationTimeout(TestSettings.NavigationTimeout);
    }
    public async Task GoAsync()
    {
        await Page.GotoAsync(
            $"{TestSettings.BaseUrl}{PagePath}",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await HandleKnownOverlaysAsync();
    }
    public async Task WaitForUrlContainsAsync(string urlFragment)
    {
        await Page.WaitForURLAsync(
            url => url.Contains(urlFragment),
            new PageWaitForURLOptions
            {
                Timeout   = TestSettings.NavigationTimeout,
                WaitUntil = WaitUntilState.DOMContentLoaded
            }
        );
    }
    public string CurrentUrl => Page.Url;
    public async Task<string> GetPageTitleAsync() => await Page.TitleAsync();
    public async Task WaitForLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }
    protected async Task<ILocator> WaitForVisibleAsync(
        ILocator locator,
        int? timeout = null,
        string? description = null)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State   = WaitForSelectorState.Visible,
                Timeout = timeout ?? TestSettings.ElementTimeout
            });
        }
        catch (Microsoft.Playwright.PlaywrightException ex) when (description is not null)
        {
            throw new InvalidOperationException(
                $"[{GetType().Name}] Timed out waiting for: {description}. " +
                $"URL: {CurrentUrl}\n" +
                $"Playwright detail: {ex.Message}", ex);
        }

        return locator;
    }
    protected async Task WaitForHiddenAsync(ILocator locator)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = TestSettings.DefaultTimeout
        });
    }
    protected async Task<bool> IsVisibleAsync(ILocator locator)
    {
        try
        {
            return await locator.CountAsync() > 0 && await locator.IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }
    protected async Task<bool> ExistsAsync(ILocator locator)
    {
        return await locator.CountAsync() > 0;
    }
    protected async Task ClickAsync(ILocator locator)
    {
        await locator.ScrollIntoViewIfNeededAsync();
        await WaitForVisibleAsync(locator);
        await locator.ClickAsync();
    }
    protected async Task FillAsync(ILocator locator, string value)
    {
        await WaitForVisibleAsync(locator);
        await locator.ClearAsync();
        await locator.FillAsync(value);
    }
    protected async Task SelectByTextAsync(ILocator locator, string optionText)
    {
        await WaitForVisibleAsync(locator);
        await locator.SelectOptionAsync(new SelectOptionValue { Label = optionText });
    }
    protected async Task<string> GetTextAsync(ILocator locator)
    {
        await WaitForVisibleAsync(locator);
        return (await locator.InnerTextAsync()).Trim();
    }
    public async Task HandleKnownOverlaysAsync()
    {
        foreach (var (overlaySelector, closeSelector) in KnownOverlays)
        {
            var overlay = Page.Locator(overlaySelector);
            if (!await IsVisibleAsync(overlay)) continue;

            var closeButton = Page.Locator(closeSelector);
            if (await IsVisibleAsync(closeButton))
            {
                await closeButton.ClickAsync();
                await WaitForHiddenAsync(overlay);
            }
        }
    }
    protected async Task DismissPopupAsync(string overlaySelector, string closeButtonSelector)
    {
        var overlay = Page.Locator(overlaySelector);
        if (!await IsVisibleAsync(overlay)) return;

        var closeButton = Page.Locator(closeButtonSelector);
        await ClickAsync(closeButton);
        await WaitForHiddenAsync(overlay);
    }
}
