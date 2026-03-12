namespace AutomationExercise.Tests.Helpers;
public static class ApiCleanupHelper
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(TestSettings.BaseUrl),
        Timeout = TimeSpan.FromSeconds(15)
    };
    public static async Task CreateUserAsync(FakeUser user)
    {
        var response = await _http.PostAsync(
            TestConstants.Api.CreateAccount,
            new FormUrlEncodedContent(user.ToFormData()));

        var body = await response.Content.ReadAsStringAsync();

        if (!body.Contains("201"))
            throw new InvalidOperationException(
                $"Failed to create test user '{user.Email}' via API. Response: {body}");
    }
    public static async Task DeleteUserAsync(string email, string password)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, TestConstants.Api.DeleteAccount)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["email"]    = email,
                    ["password"] = password
                })
            };
            await _http.SendAsync(request);
        }
        catch
        {
        }
    }
    public static async Task DeleteUserAsync(FakeUser user) =>
        await DeleteUserAsync(user.Email, user.Password);
}
