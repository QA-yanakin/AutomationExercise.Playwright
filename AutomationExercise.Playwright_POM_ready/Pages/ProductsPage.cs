using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Products listing interactions.</summary>
    public class ProductsPage : BasePage
    {
        public ProductsPage(IPage page, string baseUrl) : base(page, baseUrl) { }

        /// <summary>Open products page.</summary>
        public async Task OpenAsync() => await GotoAsync("/products");

        /// <summary>Add product to cart by visible product name.</summary>
        public async Task AddProductToCartByNameAsync(string productName)
        {
            var card = Page.Locator(".productinfo").Filter(new() { HasTextString = productName });
            await card.HoverAsync();
            await card.GetByRole(AriaRole.Link, new() { Name = "Add to cart" }).ClickAsync();
            var continueBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
            if (await continueBtn.IsVisibleAsync()) await continueBtn.ClickAsync();
        }
    }
}
