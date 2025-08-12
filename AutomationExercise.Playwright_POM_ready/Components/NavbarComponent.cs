using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Components
{
    /// <summary>Top navigation bar actions.</summary>
    public class NavbarComponent
    {
        private readonly IPage _page;

        private ILocator SignupLoginLink => _page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" });
        private ILocator LogoutLink => _page.GetByRole(AriaRole.Link, new() { Name = "Logout" });
        private ILocator CartLink => _page.GetByRole(AriaRole.Link, new() { Name = "Cart" });
        private ILocator HomeLink => _page.GetByRole(AriaRole.Link, new() { Name = "Home" });

        public NavbarComponent(IPage page) => _page = page;

        /// <summary>Opens Signup/Login page.</summary>
        public async Task OpenSignupLoginAsync() => await SignupLoginLink.ClickAsync();
        /// <summary>Opens Cart page.</summary>
        public async Task OpenCartAsync() => await CartLink.ClickAsync();
        /// <summary>Navigates Home.</summary>
        public async Task GoHomeAsync() => await HomeLink.ClickAsync();
        /// <summary>Logs out if 'Logout' is visible.</summary>
        public async Task LogoutIfLoggedInAsync()
        {
            if (await LogoutLink.IsVisibleAsync()) await LogoutLink.ClickAsync();
        }
    }
}
