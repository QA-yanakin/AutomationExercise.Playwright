using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class ProductsPage : BasePage
{
    protected override string PagePath => TestConstants.Urls.Products;

    public ProductsPage(IPage page) : base(page) { }
    private ILocator SearchInput   => Page.Locator("#search_product");
    private ILocator SearchButton  => Page.Locator("#submit_search");
    private ILocator SearchHeading => Page.GetByText("Searched Products", new() { Exact = true });
    private ILocator AllProductsHeading => Page.GetByText("All Products", new() { Exact = true });
    private ILocator AllViewProductLinks => Page.Locator("a[href*='/product_details/']");
    private ILocator CategoryParentLink(string parentName) =>
        Page.Locator($"a[href='#{parentName}']");

    private ILocator CategoryChildLink(int categoryId) =>
        Page.Locator($"a[href='/category_products/{categoryId}']");
    private ILocator BrandLink(string brandName) =>
        Page.Locator($"a[href='/brand_products/{brandName}']");

    public async Task OpenAsync()
    {
        await GoAsync();
        await WaitForVisibleAsync(AllProductsHeading);
    }
    public async Task<int> SearchForAsync(string term)
    {
        await SearchInput.ScrollIntoViewIfNeededAsync();
        await FillAsync(SearchInput, term);
        await ClickAsync(SearchButton);
        await WaitForVisibleAsync(SearchHeading);
        return await AllViewProductLinks.CountAsync();
    }
    public async Task<bool> IsSearchResultsPageAsync() =>
        await IsVisibleAsync(SearchHeading);
    public async Task FilterByCategoryAsync(int categoryId, string parentName = TestConstants.Categories.WomenParent)
    {
        var parentLink = CategoryParentLink(parentName);
        await parentLink.ScrollIntoViewIfNeededAsync();
        await ClickAsync(parentLink);
        var childLink = CategoryChildLink(categoryId);
        await WaitForVisibleAsync(childLink);
        await ClickAsync(childLink);
        await WaitForLoadAsync();
    }
    public async Task FilterByBrandAsync(string brandName)
    {
        var link = BrandLink(brandName);
        await link.ScrollIntoViewIfNeededAsync();
        await ClickAsync(link);
        await WaitForLoadAsync();
    }
    public async Task<bool> IsDisplayedAsync() =>
        await IsVisibleAsync(AllProductsHeading) && await AllViewProductLinks.CountAsync() > 0;
    public async Task<int> GetProductCountAsync() =>
        await AllViewProductLinks.CountAsync();
    public async Task<bool> IsFilteredPageDisplayedAsync() =>
        await AllViewProductLinks.CountAsync() > 0;
}
