using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Confirmation page after account creation.</summary>
    public class AccountCreatedPage : BasePage
    {
        private ILocator AccountCreatedHeader => Page.GetByRole(AriaRole.Heading, new() { Name = "Account Created!" });
        private ILocator ContinueButton => Page.GetByRole(AriaRole.Link, new() { Name = "Continue" });

        public AccountCreatedPage(IPage page, string baseUrl) : base(page, baseUrl) { }

        /// <summary>Wait for success header and continue into logged-in home.</summary>
        public async Task ContinueAsync()
        {
            await AccountCreatedHeader.WaitForAsync();
            await ContinueButton.ClickAsync();
        }
    }
}
