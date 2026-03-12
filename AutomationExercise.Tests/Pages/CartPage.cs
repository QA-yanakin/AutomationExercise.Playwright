using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class CartPage : BasePage
{
    protected override string PagePath => TestConstants.Urls.Cart;

    public CartPage(IPage page) : base(page) { }
    private ILocator EmptyCartMessage =>
        Page.GetByText("Cart is empty!", new() { Exact = false });
    private ILocator CartProductRows =>
        Page.Locator("#cart_info_table tbody tr");
    private ILocator DeleteButtonForProduct(int productId) =>
        Page.Locator($"#product-{productId} .cart_quantity_delete");
    private ILocator CheckoutButton => Page.Locator(".check_out");
    private ILocator CheckoutModal => Page.Locator("#checkoutModal");
    private ILocator CheckoutModalLoginLink =>
        CheckoutModal.GetByRole(AriaRole.Link, new() { Name = "Register / Login" });

    public async Task OpenAsync()
    {
        await GoAsync();
        await WaitForLoadAsync();
    }
    public async Task RemoveProductAsync(int productId)
    {
        var deleteBtn = DeleteButtonForProduct(productId);
        await ClickAsync(deleteBtn);
        var row = Page.Locator($"#product-{productId}");
        try
        {
            await row.WaitForAsync(new LocatorWaitForOptions
            {
                State   = WaitForSelectorState.Detached,
                Timeout = TestSettings.DefaultTimeout
            });
        }
        catch (Microsoft.Playwright.PlaywrightException)
        {
        }
    }
    public async Task ProceedToCheckoutAsync()
    {
        await ClickAsync(CheckoutButton);
        await WaitForUrlContainsAsync(TestConstants.Urls.Checkout);
    }
    public async Task ProceedToCheckoutAsGuestAsync()
    {
        await ClickAsync(CheckoutButton);
        await WaitForVisibleAsync(CheckoutModal);
        await ClickAsync(CheckoutModalLoginLink);
        await WaitForUrlContainsAsync(TestConstants.Urls.Login);
    }
    public async Task<bool> IsEmptyAsync() =>
        await IsVisibleAsync(EmptyCartMessage);
    public async Task<int> GetProductCountAsync() =>
        await CartProductRows.CountAsync();
    public async Task<bool> IsDisplayedAsync() =>
        CurrentUrl.Contains(TestConstants.Urls.Cart) &&
        (await IsEmptyAsync() || await GetProductCountAsync() > 0);
}
