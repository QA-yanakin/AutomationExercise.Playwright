using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("UI")]
[Category("Regression")]
public class CheckoutTests : BaseTest
{
    private static FakeUser _testUser = null!;

    [OneTimeSetUp]
    public async Task CreateTestUserAsync()
    {
        _testUser = TestConstants.GenerateUser();
        await ApiCleanupHelper.CreateUserAsync(_testUser);
    }

    [OneTimeTearDown]
    public async Task DeleteTestUserAsync()
    {
        if (_testUser is not null)
            await ApiCleanupHelper.DeleteUserAsync(_testUser);
    }

    [Test]
    [Description("Checkout flow: authenticated context → add to cart → proceed to checkout → address page visible")]
    public async Task Checkout_WhenLoggedIn_ShowsDeliveryAddressPage()
    {
        await UseAuthenticatedContextAsync(_testUser.Email, _testUser.Password);
        var detailPage = CreatePage<ProductDetailPage>();
        await detailPage.GoToProductAsync(TestConstants.Products.StableProductId);
        await detailPage.AddToCartAndViewCartAsync();
        var cartPage = CreatePage<CartPage>();
        var hasProducts = await cartPage.GetProductCountAsync();
        hasProducts.Should().BeGreaterThan(0,
            because: "cart must contain the product before we can proceed to checkout");

        await cartPage.ProceedToCheckoutAsync();
        var checkoutPage = CreatePage<CheckoutPage>();

        var isDisplayed = await checkoutPage.IsDisplayedAsync();
        isDisplayed.Should().BeTrue(
            because: "an authenticated user with items in cart must reach the checkout page " +
                     "with 'Place Order' visible");

        var hasAddress = await checkoutPage.IsDeliveryAddressVisibleAsync();
        hasAddress.Should().BeTrue(
            because: "the checkout page must display the delivery address section");

        checkoutPage.CurrentUrl.Should().Contain(TestConstants.Urls.Checkout,
            because: "URL must be /checkout after proceeding from cart");
    }
}
