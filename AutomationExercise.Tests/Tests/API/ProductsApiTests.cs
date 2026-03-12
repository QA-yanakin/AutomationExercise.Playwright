using AutomationExercise.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System.Text.Json;

namespace AutomationExercise.Tests.Tests.API;
[TestFixture]
[Parallelizable(ParallelScope.All)]
[Category("API")]
[Category("Regression")]
public class ProductsApiTests : BaseApiTest
{

    [Test]
    [Category("Smoke")]
    [Description("GET productsList must return 200, non-empty products array, and meet 3s SLA")]
    public async Task GetProductsList_WithNoParams_Returns200WithProductsArray()
    {
        var response = await GetAsync(TestConstants.Api.ProductsList);
        response.StatusCode.Should().Be(200,
            because: "GET /api/productsList must return HTTP 200");
        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
            because: "JSON responseCode must be 200 for a successful products list request");
        response.HasProperty("products", System.Text.Json.JsonValueKind.Array)
            .Should().BeTrue(because: "response must contain a 'products' array");

        var productCount = response.WithJson(root =>
            root.TryGetProperty("products", out var arr) ? arr.GetArrayLength() : 0);
        productCount.Should().BeGreaterThan(0,
            because: "the products catalogue must not be empty");
        var firstHasRequiredFields = response.WithJson(root =>
        {
            if (!root.TryGetProperty("products", out var arr) || arr.GetArrayLength() == 0) return false;
            var first = arr[0];
            return first.TryGetProperty("id", out _)
                && first.TryGetProperty("name", out _)
                && first.TryGetProperty("price", out _)
                && first.TryGetProperty("category", out _);
        });
        firstHasRequiredFields.Should().BeTrue(
            because: "each product must have 'id', 'name', 'price', and 'category' fields");
        response.ElapsedMs.Should().BeLessThan(ResponseTimeSlaMs,
            because: $"API response time must be under {ResponseTimeSlaMs}ms SLA");
    }

    [Test]
    [Category("Negative")]
    [Description("POST to productsList (read-only endpoint) must return 405 Method Not Allowed")]
    public async Task PostProductsList_OnReadOnlyEndpoint_Returns405()
    {
        var response = await PostFormAsync(TestConstants.Api.ProductsList,
            new Dictionary<string, string>());

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.MethodNotAllowed,
            because: "POST is not supported on /api/productsList — must return responseCode 405");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.MethodNotAllowed,
            because: "the error message must explicitly state the method is not supported");
    }

    [Test]
    [Description("POST searchProduct with a valid term must return matching products")]
    public async Task SearchProduct_WithValidTerm_ReturnsMatchingProducts()
    {
        var response = await PostFormAsync(TestConstants.Api.SearchProduct,
            new Dictionary<string, string> { ["search_product"] = TestConstants.Products.SearchTerm });

        response.StatusCode.Should().Be(200,
            because: "POST /api/searchProduct with valid term must return HTTP 200");

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
            because: "JSON responseCode must be 200 for a successful search");

        response.HasProperty("products", System.Text.Json.JsonValueKind.Array)
            .Should().BeTrue(because: "search response must contain a 'products' array");

        var resultCount = response.WithJson(root =>
            root.TryGetProperty("products", out var arr) ? arr.GetArrayLength() : 0);
        resultCount.Should().BeGreaterThan(0,
            because: $"search for '{TestConstants.Products.SearchTerm}' must return at least one result");

        response.ElapsedMs.Should().BeLessThan(ResponseTimeSlaMs,
            because: $"search API must respond within {ResponseTimeSlaMs}ms SLA");
    }

    [Test]
    [Category("Negative")]
    [Description("POST searchProduct without required parameter must return 400 Bad Request")]
    public async Task SearchProduct_WithMissingParameter_Returns400()
    {
        var response = await PostFormAsync(TestConstants.Api.SearchProduct,
            new Dictionary<string, string>());

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.BadRequest,
            because: "missing search_product parameter must return responseCode 400");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.MissingSearchParam,
            because: "the error message must match the documented bad request message exactly");
    }

    [Test]
    [Category("Negative")]
    [Description("POST searchProduct with a term that matches nothing must return empty products array")]
    public async Task SearchProduct_WithNoMatchingTerm_ReturnsEmptyProductsArray()
    {
        var response = await PostFormAsync(TestConstants.Api.SearchProduct,
            new Dictionary<string, string>
            {
                ["search_product"] = TestConstants.Products.NoResultsTerm
            });

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
            because: "no-results search must still return 200 — not a 404");

        response.HasProperty("products", System.Text.Json.JsonValueKind.Array)
            .Should().BeTrue(because: "response must always contain a 'products' key");

        var emptyCount = response.WithJson(root =>
            root.TryGetProperty("products", out var arr) ? arr.GetArrayLength() : -1);
        emptyCount.Should().Be(0,
            because: $"'{TestConstants.Products.NoResultsTerm}' must match no products");
    }
}
