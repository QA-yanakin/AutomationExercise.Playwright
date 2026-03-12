using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace AutomationExercise.Tests.Helpers;
[TestFixture]
public abstract class BaseTest
{
    private IPlaywright _playwright = null!;
    private IBrowser    _browser    = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage           Page    { get; private set; } = null!;

    private static readonly string[] BlockedUrlPatterns =
    [
        "**/*doubleclick*/**",
        "**/*googlesyndication*/**",
        "**/*google-analytics*/**",
        "**/*googletagmanager*/**",
        "**/*googleadservices*/**",
        "**/*hotjar*/**",
        "**/*clarity.ms*/**",
        "**/*facebook**/tr/**",
        "**/*amazon-adsystem*/**",
    ];

    [OneTimeSetUp]
    public async Task BrowserSetUpAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestSettings.Headless,
            SlowMo   = TestSettings.SlowMo,
            Args     = ["--no-sandbox", "--disable-dev-shm-usage"]
        });
    }

    [SetUp]
    public async Task ContextSetUpAsync()
    {
        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL          = TestSettings.BaseUrl,
            ViewportSize     = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true
        });

        Context.SetDefaultTimeout(TestSettings.DefaultTimeout);
        Context.SetDefaultNavigationTimeout(TestSettings.NavigationTimeout);

        Page = await Context.NewPageAsync();
        await ApplyRouteBlockingAsync();
    }

    [TearDown]
    public async Task ContextTearDownAsync()
    {
        await CaptureScreenshotOnFailureAsync();
        await Context.CloseAsync();
    }

    [OneTimeTearDown]
    public async Task BrowserTearDownAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
    private Task ApplyRouteBlockingAsync() => ApplyRouteBlockingOnPageAsync(Page);
    protected async Task UseAuthenticatedContextAsync(string email, string password)
    {
        await Context.CloseAsync();

        var authContext = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL           = TestSettings.BaseUrl,
            ViewportSize      = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true
        });
        var setupPage = await authContext.NewPageAsync();
        await ApplyRouteBlockingOnPageAsync(setupPage);
        await setupPage.GotoAsync($"{TestSettings.BaseUrl}/login",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await setupPage.Locator("[data-qa='login-email']").FillAsync(email);
        await setupPage.Locator("[data-qa='login-password']").FillAsync(password);
        await setupPage.Locator("[data-qa='login-button']").ClickAsync();
        await setupPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var currentUrl = setupPage.Url;
        if (currentUrl.Contains("/login"))
        {
            await setupPage.CloseAsync();
            await authContext.CloseAsync();
            throw new InvalidOperationException(
                $"UseAuthenticatedContextAsync: Login failed for '{email}'. " +
                $"Still on login page after submitting credentials. " +
                $"Verify the test user exists (created via ApiCleanupHelper.CreateUserAsync).");
        }

        await setupPage.CloseAsync();
        Context = authContext;
        Context.SetDefaultTimeout(TestSettings.DefaultTimeout);
        Context.SetDefaultNavigationTimeout(TestSettings.NavigationTimeout);
        Page = await Context.NewPageAsync();
        await ApplyRouteBlockingAsync();
    }
    private Task ApplyRouteBlockingOnPageAsync(IPage page) =>
        Task.WhenAll(BlockedUrlPatterns.Select(pattern =>
            page.RouteAsync(pattern, route => route.AbortAsync())));

    private async Task CaptureScreenshotOnFailureAsync()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
            return;

        var screenshotDir = Path.Combine(
            AppContext.BaseDirectory, "TestResults", "Screenshots");

        Directory.CreateDirectory(screenshotDir);
        var invalidChars = Path.GetInvalidFileNameChars()
            .Concat(new[] { '(', ')', '"', '\'' })
            .ToHashSet();

        var testName = string.Concat(
            TestContext.CurrentContext.Test.FullName
                .Select(c => invalidChars.Contains(c) ? '_' : c))
            .Replace("__", "_")
            .TrimEnd('_');

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filePath  = Path.Combine(screenshotDir, $"{testName}_{timestamp}.png");

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path     = filePath,
            FullPage = true
        });

        TestContext.WriteLine($"Screenshot saved: {filePath}");
    }
    protected T CreatePage<T>() where T : BasePage
    {
        return (T)Activator.CreateInstance(typeof(T), Page)!;
    }
}
