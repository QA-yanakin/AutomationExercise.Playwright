using AutomationExercise.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.API;
[TestFixture]
[Parallelizable(ParallelScope.All)]
[Category("API")]
[Category("Regression")]
public class BrandsApiTests : BaseApiTest
{
    [Test]
    [Category("Smoke")]
    [Description("GET brandsList must return 200, non-empty brands array, and meet 3s SLA")]
    public async Task GetBrandsList_WithNoParams_Returns200WithBrandsArray()
    {
        var response = await GetAsync(TestConstants.Api.BrandsList);

        response.StatusCode.Should().Be(200,
            because: "GET /api/brandsList must return HTTP 200");

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
            because: "JSON responseCode must be 200 for a successful brands list");

        response.HasProperty("brands", System.Text.Json.JsonValueKind.Array)
            .Should().BeTrue(because: "response must contain a 'brands' array");

        var brandCount = response.WithJson(root =>
            root.TryGetProperty("brands", out var arr) ? arr.GetArrayLength() : 0);
        brandCount.Should().BeGreaterThan(0,
            because: "the brands list must not be empty");
        var (firstHasRequiredFields, containsPolo) = response.WithJson(root =>
        {
            if (!root.TryGetProperty("brands", out var arr) || arr.GetArrayLength() == 0)
                return (false, false);
            var first = arr[0];
            var hasFields = first.TryGetProperty("id", out _) && first.TryGetProperty("brand", out _);
            var hasPolo = Enumerable.Range(0, arr.GetArrayLength())
                .Any(i => arr[i].TryGetProperty("brand", out var b)
                          && b.GetString() == TestConstants.Brands.Polo);
            return (hasFields, hasPolo);
        });

        firstHasRequiredFields.Should().BeTrue(
            because: "each brand must have 'id' and 'brand' fields");
        containsPolo.Should().BeTrue(
            because: $"'{TestConstants.Brands.Polo}' is a known stable brand on the site");

        response.ElapsedMs.Should().BeLessThan(ResponseTimeSlaMs,
            because: $"brands API must respond within {ResponseTimeSlaMs}ms SLA");
    }

    [Test]
    [Category("Negative")]
    [Description("PUT to brandsList (read-only endpoint) must return 405 Method Not Allowed")]
    public async Task PutBrandsList_OnReadOnlyEndpoint_Returns405()
    {
        var response = await PutFormAsync(TestConstants.Api.BrandsList,
            new Dictionary<string, string>());

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.MethodNotAllowed,
            because: "PUT is not supported on /api/brandsList — must return responseCode 405");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.MethodNotAllowed,
            because: "the error message must explicitly state the method is not supported");
    }
}
