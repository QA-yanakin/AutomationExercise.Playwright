using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("UI")]
[Category("Regression")]
public class CartTests : BaseTest
{

    [Test]
    [Description("Adding a product from its detail page must show it in /view_cart")]
    public async Task AddProduct_FromDetailPage_AppearsInCart()
    {
        var detailPage = CreatePage<ProductDetailPage>();
        await detailPage.GoToProductAsync(TestConstants.Products.StableProductId);

        var isLoaded = await detailPage.IsDisplayedAsync();
        isLoaded.Should().BeTrue(
            because: $"product {TestConstants.Products.StableProductId} detail page must load before adding to cart");
        await detailPage.AddToCartAndViewCartAsync();
        var cartPage = CreatePage<CartPage>();
        var cartLoaded = await cartPage.IsDisplayedAsync();
        cartLoaded.Should().BeTrue(
            because: "after clicking 'View Cart' we must land on /view_cart");
        var isEmpty = await cartPage.IsEmptyAsync();
        isEmpty.Should().BeFalse(
            because: "cart must contain the product we just added");

        var count = await cartPage.GetProductCountAsync();
        count.Should().BeGreaterThan(0,
            because: "at least one product row must appear in the cart table");
    }

    [Test]
    [Description("Removing the only product from cart must result in an empty cart")]
    public async Task RemoveProduct_FromCart_CartBecomesEmpty()
    {
        var detailPage = CreatePage<ProductDetailPage>();
        await detailPage.GoToProductAsync(TestConstants.Products.StableProductId);
        await detailPage.AddToCartAndViewCartAsync();

        var cartPage = CreatePage<CartPage>();
        var countBefore = await cartPage.GetProductCountAsync();
        countBefore.Should().BeGreaterThan(0,
            because: "cart must have at least one product before we can test removal");
        await cartPage.RemoveProductAsync(TestConstants.Products.StableProductId);
        await cartPage.WaitForLoadAsync();
        var isEmpty = await cartPage.IsEmptyAsync();
        isEmpty.Should().BeTrue(
            because: "removing the only product must result in an empty cart with the empty-cart message");
    }

    [Test]
    [Category("Negative")]
    [Description("Clicking checkout without being logged in must show a modal with Register/Login link")]
    public async Task Checkout_WhenNotLoggedIn_ShowsModalWithLoginLink()
    {
        var detailPage = CreatePage<ProductDetailPage>();
        await detailPage.GoToProductAsync(TestConstants.Products.StableProductId);
        await detailPage.AddToCartAndViewCartAsync();

        var cartPage = CreatePage<CartPage>();
        await cartPage.ProceedToCheckoutAsGuestAsync();
        cartPage.CurrentUrl.Should().Contain(TestConstants.Urls.Login,
            because: "a guest attempting checkout must be redirected to /login to authenticate first");
    }
}
