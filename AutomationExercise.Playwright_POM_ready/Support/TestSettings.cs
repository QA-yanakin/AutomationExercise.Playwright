using System.Text.Json;

namespace AutomationExercise.Playwright.Support
{
    /// <summary>Strongly-typed settings loaded from appsettings.json.</summary>
    public class TestSettings
    {
        public string BaseUrl { get; init; } = "https://automationexercise.com";
        public int TimeoutMs { get; init; } = 10000;
        public bool Headless { get; init; } = true;
        public Credential DefaultUser { get; init; } = new();

        public static TestSettings Load(string path = "appsettings.json")
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<TestSettings>(json)!;
        }
    }

    /// <summary>Simple credential record for reusable login data.</summary>
    public record Credential
    {
        public string Email { get; init; } = "";
        public string Password { get; init; } = "";
    }
}
