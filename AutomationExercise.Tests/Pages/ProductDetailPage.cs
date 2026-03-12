using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class ProductDetailPage : BasePage
{
    protected override string PagePath => "/products";

    public ProductDetailPage(IPage page) : base(page) { }
    private ILocator ProductName     => Page.Locator(".product-information h2");
    private ILocator ProductPrice    => Page.Locator(".product-information span span");
    private ILocator ProductCategory => Page.GetByText("Category:", new() { Exact = false });
    private ILocator InStockBadge    => Page.GetByText("In Stock", new() { Exact = false });
    private ILocator QuantityInput   => Page.Locator("#quantity");
    private ILocator AddToCartButton =>
        Page.Locator(".product-information").GetByText("Add to cart", new() { Exact = true });
    private ILocator CartSuccessModal    => Page.Locator(".modal-content");
    private ILocator ViewCartLink        => Page.GetByRole(AriaRole.Link, new() { Name = "View Cart" });
    private ILocator ContinueShoppingBtn => Page.GetByRole(AriaRole.Button,
        new() { Name = "Continue Shopping" });
    public async Task GoToProductAsync(int productId)
    {
        await Page.GotoAsync(
            $"{TestSettings.BaseUrl}/product_details/{productId}",
            new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await HandleKnownOverlaysAsync();
        await WaitForVisibleAsync(AddToCartButton);
    }
    public async Task AddToCartAsync(int quantity = 1)
    {
        await FillAsync(QuantityInput, quantity.ToString());
        await ClickAsync(AddToCartButton);
        await WaitForVisibleAsync(CartSuccessModal);
    }
    public async Task AddToCartAndViewCartAsync(int quantity = 1)
    {
        await AddToCartAsync(quantity);
        await ClickAsync(ViewCartLink);
        await WaitForUrlContainsAsync(TestConstants.Urls.Cart);
    }
    public async Task AddToCartAndContinueShoppingAsync(int quantity = 1)
    {
        await AddToCartAsync(quantity);
        await ClickAsync(ContinueShoppingBtn);
        await WaitForHiddenAsync(CartSuccessModal);
    }
    public async Task<string> GetProductNameAsync() =>
        await GetTextAsync(ProductName);
    public async Task<string> GetProductPriceAsync() =>
        await GetTextAsync(ProductPrice);
    public async Task<bool> IsInStockAsync() =>
        await IsVisibleAsync(InStockBadge);
    public async Task<bool> IsDisplayedAsync() =>
        CurrentUrl.Contains("/product_details/") && await IsVisibleAsync(AddToCartButton);
}
