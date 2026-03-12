using Microsoft.Extensions.Configuration;

namespace AutomationExercise.Tests.Helpers;

public class TestSettings
{
    private static readonly IConfiguration _config;

    static TestSettings()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL")
        ?? _config["BaseUrl"]
        ?? "https://automationexercise.com";
    public static int DefaultTimeout =>
        int.Parse(_config["Timeouts:DefaultTimeout"] ?? "30000");
    public static int NavigationTimeout =>
        int.Parse(_config["Timeouts:NavigationTimeout"] ?? "15000");
    public static int ElementTimeout =>
        int.Parse(_config["Timeouts:ElementTimeout"] ?? "10000");

    public static bool Headless =>
        bool.Parse(_config["Browser:Headless"] ?? "true");

    public static int SlowMo =>
        int.Parse(_config["Browser:SlowMo"] ?? "0");

    public static string TestUserEmail =>
        _config["TestUser:Email"] ?? string.Empty;

    public static string TestUserPassword =>
        _config["TestUser:Password"] ?? string.Empty;
}
