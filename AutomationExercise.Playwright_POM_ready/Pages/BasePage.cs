using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Base page with common helpers.</summary>
    public abstract class BasePage
    {
        protected readonly IPage Page;
        protected readonly string BaseUrl;

        protected BasePage(IPage page, string baseUrl)
        {
            Page = page;
            BaseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>Navigate to a relative path, waiting for network idle.</summary>
        protected async Task GotoAsync(string relativePath = "/") =>
            await Page.GotoAsync($"{BaseUrl}{relativePath}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }
}
