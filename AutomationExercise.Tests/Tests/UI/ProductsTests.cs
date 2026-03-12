using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("UI")]
[Category("Regression")]
public class ProductsTests : BaseTest
{

    [Test]
    [Category("Smoke")]
    [Description("Products page must load with a heading and at least one product visible")]
    public async Task ProductsPage_WhenNavigated_DisplaysProductsListing()
    {
        var productsPage = CreatePage<ProductsPage>();
        await productsPage.OpenAsync();

        var isDisplayed = await productsPage.IsDisplayedAsync();
        isDisplayed.Should().BeTrue(
            because: "the products page must show 'All Products' heading and at least one product");

        var count = await productsPage.GetProductCountAsync();
        count.Should().BeGreaterThan(0,
            because: "the product catalogue must not be empty");
    }

    [Test]
    [Description("Product detail page must display name, price, and 'In Stock' for a known product")]
    public async Task ProductDetail_WhenNavigatedById_DisplaysCorrectProductInfo()
    {
        var detailPage = CreatePage<ProductDetailPage>();
        await detailPage.GoToProductAsync(TestConstants.Products.StableProductId);

        var isDisplayed = await detailPage.IsDisplayedAsync();
        isDisplayed.Should().BeTrue(
            because: $"product detail page for ID {TestConstants.Products.StableProductId} must load and show Add to Cart button");

        var name = await detailPage.GetProductNameAsync();
        name.Should().Be(TestConstants.Products.StableProductName,
            because: $"product ID {TestConstants.Products.StableProductId} must be '{TestConstants.Products.StableProductName}'");

        var price = await detailPage.GetProductPriceAsync();
        price.Should().StartWith("Rs.",
            because: "all product prices on this site use the Rs. currency prefix");

        var inStock = await detailPage.IsInStockAsync();
        inStock.Should().BeTrue(
            because: $"'{TestConstants.Products.StableProductName}' must be in stock");
    }

    [Test]
    [Description("Searching with a known term must show 'Searched Products!' heading and at least one result")]
    public async Task Search_WithValidTerm_ReturnsMatchingProducts()
    {
        var productsPage = CreatePage<ProductsPage>();
        await productsPage.OpenAsync();

        var resultCount = await productsPage.SearchForAsync(TestConstants.Products.SearchTerm);

        var isSearchPage = await productsPage.IsSearchResultsPageAsync();
        isSearchPage.Should().BeTrue(
            because: "after search the 'Searched Products!' heading must appear");

        resultCount.Should().BeGreaterThan(0,
            because: $"search for '{TestConstants.Products.SearchTerm}' must return at least one product");

        productsPage.CurrentUrl.Should().Contain("search",
            because: "the URL must reflect the search query");
    }

    [Test]
    [Description("Clicking a category filter must navigate to the filtered URL and show products")]
    public async Task FilterByCategory_WhenClicked_NavigatesToCategoryProductsUrl()
    {
        var productsPage = CreatePage<ProductsPage>();
        await productsPage.OpenAsync();
        await productsPage.FilterByCategoryAsync(TestConstants.Categories.WomenDressId, TestConstants.Categories.WomenParent);

        productsPage.CurrentUrl.Should().Contain(
            $"/category_products/{TestConstants.Categories.WomenDressId}",
            because: $"filtering by '{TestConstants.Categories.WomenDress}' must navigate to /category_products/{TestConstants.Categories.WomenDressId}");

        var hasProducts = await productsPage.IsFilteredPageDisplayedAsync();
        hasProducts.Should().BeTrue(
            because: $"the '{TestConstants.Categories.WomenDress}' category must contain at least one product");
    }

    [Test]
    [Description("Clicking a brand filter must navigate to the filtered URL and show products")]
    public async Task FilterByBrand_WhenClicked_NavigatesToBrandProductsUrl()
    {
        var productsPage = CreatePage<ProductsPage>();
        await productsPage.OpenAsync();

        await productsPage.FilterByBrandAsync(TestConstants.Brands.Polo);

        productsPage.CurrentUrl.Should().Contain(
            $"/brand_products/{TestConstants.Brands.Polo}",
            because: $"filtering by brand '{TestConstants.Brands.Polo}' must navigate to /brand_products/{TestConstants.Brands.Polo}");

        var hasProducts = await productsPage.IsFilteredPageDisplayedAsync();
        hasProducts.Should().BeTrue(
            because: $"the '{TestConstants.Brands.Polo}' brand must have at least one product");
    }
}
