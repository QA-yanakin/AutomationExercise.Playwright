using Microsoft.Playwright;
using System.Threading.Tasks;
using AutomationExercise.Playwright.Components;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Home page encapsulation.</summary>
    public class HomePage : BasePage
    {
        public NavbarComponent Navbar { get; }

        public HomePage(IPage page, string baseUrl) : base(page, baseUrl) =>
            Navbar = new NavbarComponent(page);

        /// <summary>Open the Home page.</summary>
        public async Task OpenAsync() => await GotoAsync("/");
    }
}
