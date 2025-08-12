using NUnit.Framework;
using System.Threading.Tasks;
using FluentAssertions;
using AutomationExercise.Playwright.Support;
using AutomationExercise.Playwright.Pages;

namespace AutomationExercise.Playwright.Tests
{
    /// <summary>Auth scenarios (from your sheet): register/login variants.</summary>
    [TestFixture]
    public class AuthTests : TestBase
    {
        [Test]
        public async Task TC01_Register_User_With_Valid_Credentials()
        {
            var home = new HomePage(Page, Settings.BaseUrl);
            await home.OpenAsync();
            await home.Navbar.OpenSignupLoginAsync();

            var login = new LoginPage(Page, Settings.BaseUrl);
            var uniqueEmail = $"autouser_{System.DateTime.UtcNow.Ticks}@example.com";
            var signupError = await login.BeginSignupAsync("Auto QA", uniqueEmail);
            signupError.Should().BeNull("unique email should proceed to account info page");

            var register = new RegisterAccountPage(Page, Settings.BaseUrl);
            await register.CompleteRequiredFieldsAndSubmitAsync(
                password: "P@ssword1234",
                firstName: "Auto",
                lastName: "QA",
                address: "Automation Street 1",
                country: "United States",
                state: "CA",
                city: "Los Angeles",
                zipcode: "90001",
                mobile: "+15555550123"
            );

            var created = new AccountCreatedPage(Page, Settings.BaseUrl);
            await created.ContinueAsync();
            var loggedInAs = Page.GetByText("Logged in as", new() { Exact = false });
            (await loggedInAs.IsVisibleAsync()).Should().BeTrue();
        }

        [Test]
        public async Task TC02_Login_With_Valid_Credentials()
        {
            var home = new HomePage(Page, Settings.BaseUrl);
            await home.OpenAsync();
            await home.Navbar.OpenSignupLoginAsync();

            var login = new LoginPage(Page, Settings.BaseUrl);
            var error = await login.LoginAsync(Settings.DefaultUser.Email, Settings.DefaultUser.Password);

            error.Should().BeNull();
            var loggedInAs = Page.GetByText("Logged in as", new() { Exact = false });
            (await loggedInAs.IsVisibleAsync()).Should().BeTrue();
        }

        [Test]
        public async Task TC03_Login_With_Invalid_Password_Shows_Error()
        {
            var home = new HomePage(Page, Settings.BaseUrl);
            await home.OpenAsync();
            await home.Navbar.OpenSignupLoginAsync();

            var login = new LoginPage(Page, Settings.BaseUrl);
            var error = await login.LoginAsync(Settings.DefaultUser.Email, "wrong-password-!");

            error.Should().NotBeNull();
            error!.ToLowerInvariant().Should().Contain("incorrect");
        }
    }
}
