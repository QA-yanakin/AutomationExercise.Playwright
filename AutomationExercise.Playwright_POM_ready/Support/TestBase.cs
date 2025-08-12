using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Support
{
    /// <summary>
    /// NUnit base: single browser per run, fresh context/page per test for isolation.
    /// </summary>
    public abstract class TestBase
    {
        protected static IPlaywright _playwright = null!;
        protected static IBrowser _browser = null!;
        protected IPage Page = null!;
        protected IBrowserContext Context = null!;
        protected static TestSettings Settings = null!;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            Settings = TestSettings.Load();
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Settings.Headless
            });
        }

        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            if (_browser != null) await _browser.CloseAsync();
            _playwright?.Dispose();
        }

        [SetUp]
        public async Task SetUp()
        {
            Context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1440, Height = 900 }
            });
            Page = await Context.NewPageAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (Context != null) await Context.CloseAsync();
        }
    }
}
