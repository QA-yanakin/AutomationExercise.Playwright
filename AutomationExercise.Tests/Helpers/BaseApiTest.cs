using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace AutomationExercise.Tests.Helpers;
public record ApiResponse(
    int StatusCode,
    string Body,
    long ElapsedMs = 0)
{
    public string? GetString(string property)
    {
        using var doc = JsonDocument.Parse(Body);
        return doc.RootElement.TryGetProperty(property, out var p) ? p.GetString() : null;
    }
    public int? GetInt(string property)
    {
        using var doc = JsonDocument.Parse(Body);
        return doc.RootElement.TryGetProperty(property, out var p) && p.TryGetInt32(out var v)
            ? v : null;
    }
    public bool HasProperty(string property, JsonValueKind kind = JsonValueKind.Undefined)
    {
        using var doc = JsonDocument.Parse(Body);
        if (!doc.RootElement.TryGetProperty(property, out var p)) return false;
        return kind == JsonValueKind.Undefined || p.ValueKind == kind;
    }
    public T WithJson<T>(Func<JsonElement, T> accessor)
    {
        using var doc = JsonDocument.Parse(Body);
        return accessor(doc.RootElement);
    }
    public T Deserialize<T>()
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result ?? throw new InvalidOperationException(
                $"Deserialization of {typeof(T).Name} returned null.\nBody: {Body}");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize response as {typeof(T).Name}.\nBody: {Body}", ex);
        }
    }
}
[TestFixture]
public abstract class BaseApiTest
{
    private static readonly HttpClient _http = new(new HttpClientHandler
    {
        AllowAutoRedirect = false
    })
    {
        BaseAddress = new Uri(TestSettings.BaseUrl),
        Timeout = TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout)
    };
    private ApiResponse? _lastResponse;
    protected const int ResponseTimeSlaMs = 3000;

    [OneTimeSetUp]
    public async Task VerifyApiAvailableAsync()
    {
        try
        {
            var response = await _http.GetAsync(TestConstants.Api.ProductsList);
            if (!response.IsSuccessStatusCode)
                Assert.Fail(
                    $"API health check failed. " +
                    $"GET {TestConstants.Api.ProductsList} returned {(int)response.StatusCode}. " +
                    $"Aborting test suite.");
        }
        catch (HttpRequestException ex)
        {
            Assert.Fail(
                $"Cannot reach {TestSettings.BaseUrl}. " +
                $"Check your internet connection or BASE_URL setting.\n{ex.Message}");
        }
    }

    [TearDown]
    public void LogLastResponseOnFailure()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
            return;

        if (_lastResponse is null) return;

        TestContext.WriteLine(
            $"\n[API] Last response on failure:\n" +
            $"  Status  : {_lastResponse.StatusCode}\n" +
            $"  Time    : {_lastResponse.ElapsedMs}ms\n" +
            $"  Body    : {_lastResponse.Body}");
    }

    protected async Task<ApiResponse> GetAsync(string endpoint)
    {
        var (response, elapsed) = await TimedRequestAsync(() => _http.GetAsync(endpoint));
        return await ParseAsync(response, endpoint, "GET", elapsed);
    }

    protected async Task<ApiResponse> PostFormAsync(
        string endpoint, Dictionary<string, string> formData)
    {
        var (response, elapsed) = await TimedRequestAsync(
            () => _http.PostAsync(endpoint, new FormUrlEncodedContent(formData)));
        return await ParseAsync(response, endpoint, "POST", elapsed);
    }
    protected async Task<ApiResponse> DeleteFormAsync(
        string endpoint, Dictionary<string, string> formData)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
        {
            Content = new FormUrlEncodedContent(formData)
        };
        var (response, elapsed) = await TimedRequestAsync(() => _http.SendAsync(request));
        return await ParseAsync(response, endpoint, "DELETE", elapsed);
    }
    protected async Task<ApiResponse> PutFormAsync(
        string endpoint, Dictionary<string, string> formData)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = new FormUrlEncodedContent(formData)
        };
        var (response, elapsed) = await TimedRequestAsync(() => _http.SendAsync(request));
        return await ParseAsync(response, endpoint, "PUT", elapsed);
    }

    private static async Task<(HttpResponseMessage Response, long ElapsedMs)> TimedRequestAsync(
        Func<Task<HttpResponseMessage>> request)
    {
        var sw = Stopwatch.StartNew();
        var response = await request();
        sw.Stop();
        return (response, sw.ElapsedMilliseconds);
    }

    private async Task<ApiResponse> ParseAsync(
        HttpResponseMessage response,
        string endpoint = "",
        string method = "",
        long elapsedMs = 0)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (!IsValidJson(body))
            body = $"{{\"raw\":\"{JsonEncodedText.Encode(body)}\"}}";

        var parsed = new ApiResponse((int)response.StatusCode, body, elapsedMs);
        _lastResponse = parsed;

        if (elapsedMs > 0)
            TestContext.WriteLine($"[API] {method} {endpoint} → {(int)response.StatusCode} ({elapsedMs}ms)");

        return parsed;
    }

    private static bool IsValidJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return false;
        try { JsonDocument.Parse(body).Dispose(); return true; }
        catch { return false; }
    }
}
