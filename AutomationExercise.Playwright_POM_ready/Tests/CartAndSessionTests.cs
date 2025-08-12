using NUnit.Framework;
using System.Threading.Tasks;
using FluentAssertions;
using AutomationExercise.Playwright.Support;
using AutomationExercise.Playwright.Pages;

namespace AutomationExercise.Playwright.Tests
{
    /// <summary>Cart and session-related scenarios.</summary>
    [TestFixture]
    public class CartAndSessionTests : TestBase
    {
        [Test]
        public async Task TC09_Logout_Functionality()
        {
            var home = new HomePage(Page, Settings.BaseUrl);
            await home.OpenAsync();
            await home.Navbar.OpenSignupLoginAsync();

            var login = new LoginPage(Page, Settings.BaseUrl);
            var error = await login.LoginAsync(Settings.DefaultUser.Email, Settings.DefaultUser.Password);
            error.Should().BeNull();

            await home.Navbar.LogoutIfLoggedInAsync();
            var signupLoginLink = Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" });
            (await signupLoginLink.IsVisibleAsync()).Should().BeTrue();
        }
    }
}
