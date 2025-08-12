using Microsoft.Playwright;
using System.Threading.Tasks;
using FluentAssertions;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Cart page methods and checks.</summary>
    public class CartPage : BasePage
    {
        private ILocator CartTable => Page.Locator("#cart_info");
        public CartPage(IPage page, string baseUrl) : base(page, baseUrl) { }
        /// <summary>Assert cart table is visible.</summary>
        public async Task AssertCartVisibleAsync()
        {
            await CartTable.WaitForAsync();
            (await CartTable.IsVisibleAsync()).Should().BeTrue();
        }
    }
}
