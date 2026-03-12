using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class CheckoutPage : BasePage
{
    protected override string PagePath => TestConstants.Urls.Checkout;

    public CheckoutPage(IPage page) : base(page) { }
    private ILocator PlaceOrderLink =>
        Page.GetByRole(AriaRole.Link, new() { Name = "Place Order" });
    private ILocator DeliveryAddressSection =>
        Page.GetByText("Your delivery address", new() { Exact = false });
    private ILocator OrderCommentArea =>
        Page.Locator("textarea[name='message']");
    private ILocator PageIdentity => DeliveryAddressSection;

    public async Task OpenAsync()
    {
        await GoAsync();
        await WaitForVisibleAsync(PlaceOrderLink);
    }
    public async Task PlaceOrderAsync()
    {
        await ClickAsync(PlaceOrderLink);
        await WaitForUrlContainsAsync(TestConstants.Urls.Payment);
    }
    public async Task AddCommentAsync(string comment)
    {
        if (await IsVisibleAsync(OrderCommentArea))
            await FillAsync(OrderCommentArea, comment);
    }
    public async Task<bool> IsDisplayedAsync() =>
        CurrentUrl.Contains(TestConstants.Urls.Checkout) &&
        await IsVisibleAsync(PlaceOrderLink);
    public async Task<bool> IsDeliveryAddressVisibleAsync() =>
        await IsVisibleAsync(DeliveryAddressSection);
}
