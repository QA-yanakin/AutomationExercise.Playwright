using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class HomePage : BasePage
{
    protected override string PagePath => TestConstants.Urls.Home;

    public HomePage(IPage page) : base(page) { }
    private ILocator NavLogin     => Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" });
    private ILocator NavProducts  => Page.GetByRole(AriaRole.Link, new() { Name = "Products" });
    private ILocator NavCart      => Page.GetByRole(AriaRole.Link, new() { Name = "Cart" });
    private ILocator NavContactUs => Page.GetByRole(AriaRole.Link, new() { Name = "Contact us" });
    private ILocator NavLogout    => Page.GetByRole(AriaRole.Link, new() { Name = "Logout" });
    private ILocator LoggedInBadge    => Page.Locator("[data-qa='logged-in-as']");
    private ILocator LoggedInTextNode => Page.Locator("li").Filter(new() {
        HasText = "Logged in as" });
    private ILocator HeroCarousel     => Page.Locator("#slider-carousel");
    private ILocator SubscribeInput   => Page.Locator("#susbscribe_email");
    private ILocator SubscribeBtn     => Page.Locator("#subscribe");
    private ILocator SubscribeSuccess => Page.GetByText(
        "You have been successfully subscribed!", new() { Exact = true });
    public async Task OpenAsync()
    {
        await GoAsync();
    }

    public async Task GoToLoginAsync()
    {
        await ClickAsync(NavLogin);
        await WaitForUrlContainsAsync(TestConstants.Urls.Login);
    }

    public async Task GoToProductsAsync()
    {
        await ClickAsync(NavProducts);
        await WaitForUrlContainsAsync(TestConstants.Urls.Products);
    }

    public async Task GoToCartAsync()
    {
        await ClickAsync(NavCart);
        await WaitForUrlContainsAsync(TestConstants.Urls.Cart);
    }

    public async Task GoToContactUsAsync()
    {
        await ClickAsync(NavContactUs);
        await WaitForUrlContainsAsync(TestConstants.Urls.ContactUs);
    }

    public async Task LogoutAsync()
    {
        await ClickAsync(NavLogout);
        await WaitForUrlContainsAsync(TestConstants.Urls.Login);
    }
    public async Task<bool> IsDisplayedAsync()
    {
        var isOnHomeUrl = CurrentUrl.TrimEnd('/').EndsWith(
            TestSettings.BaseUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
            || CurrentUrl.EndsWith("/");

        return isOnHomeUrl && await IsVisibleAsync(HeroCarousel);
    }
    public async Task<bool> IsLoggedInAsync()
    {
        return await IsVisibleAsync(LoggedInBadge)
            || await IsVisibleAsync(LoggedInTextNode);
    }
    public async Task<string?> GetLoggedInUsernameAsync()
    {
        ILocator? activeLocator = null;

        if (await IsVisibleAsync(LoggedInBadge))
            activeLocator = LoggedInBadge;
        else if (await IsVisibleAsync(LoggedInTextNode))
            activeLocator = LoggedInTextNode;

        if (activeLocator is null) return null;

        var fullText = await GetTextAsync(activeLocator);
        const string prefix = "Logged in as ";
        var idx = fullText.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? fullText[(idx + prefix.Length)..].Trim() : null;
    }
    public async Task SubscribeAsync(string email)
    {
        await SubscribeInput.ScrollIntoViewIfNeededAsync();
        await FillAsync(SubscribeInput, email);
        await ClickAsync(SubscribeBtn);

        bool appeared;
        try
        {
            await SubscribeSuccess.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = TestSettings.DefaultTimeout
            });
            appeared = true;
        }
        catch
        {
            appeared = false;
        }

        if (!appeared)
            throw new InvalidOperationException(
                $"Newsletter subscription did not show success message after submitting '{email}'. " +
                $"Current URL: {CurrentUrl}. Verify the selector for SubscribeSuccess.");
    }
    public async Task<bool> IsSubscriptionSuccessfulAsync()
        => await IsVisibleAsync(SubscribeSuccess);
}
