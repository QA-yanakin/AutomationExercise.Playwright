using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("UI")]
[Category("Regression")]
public class RegisterTests : BaseTest
{
    private FakeUser? _createdUser;

    [TearDown]
    public async Task DeleteCreatedUserIfAnyAsync()
    {
        if (_createdUser is null) return;
        await ApiCleanupHelper.DeleteUserAsync(_createdUser);
        _createdUser = null;
    }

    [Test]
    [Description("Full registration flow: /login signup → /signup form → 'Account Created!' shown")]
    public async Task Register_WithValidData_ShowsAccountCreatedAndLogsIn()
    {
        var user = TestConstants.GenerateUser();
        _createdUser = user;
        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();
        await loginPage.StartSignupAsync(user.Name, user.Email);
        var registerPage = CreatePage<RegisterPage>();
        await registerPage.WaitForPageAsync();

        var isFormDisplayed = await registerPage.IsDisplayedAsync();
        isFormDisplayed.Should().BeTrue(
            because: "the full registration form must appear after submitting name + email on /login");
        await registerPage.FillAndSubmitAsync(user);
        var isCreated = await registerPage.IsAccountCreatedAsync();
        isCreated.Should().BeTrue(
            because: "completing the registration form must show the 'Account Created!' confirmation");
        await registerPage.ContinueAfterRegistrationAsync();
        var homePage = CreatePage<HomePage>();
        var isLoggedIn = await homePage.IsLoggedInAsync();
        isLoggedIn.Should().BeTrue(
            because: "after registration the user must be automatically logged in");

        var username = await homePage.GetLoggedInUsernameAsync();
        username.Should().NotBeNullOrWhiteSpace(
            because: "the nav bar must show the registered user's name after account creation");
    }

    [Test]
    [Category("Negative")]
    [Description("Attempting to start signup with an already-registered email must show a duplicate email error")]
    public async Task Register_WithDuplicateEmail_ShowsDuplicateEmailError()
    {
        var existingUser = TestConstants.GenerateUser();
        _createdUser = existingUser;
        await ApiCleanupHelper.CreateUserAsync(existingUser);
        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();
        await loginPage.StartSignupAsync("Any Name", existingUser.Email);
        var registerPage = CreatePage<RegisterPage>();
        await registerPage.WaitForUrlContainsAsync("/signup");
        var isDuplicateErrorVisible = await registerPage.IsDuplicateEmailErrorVisibleAsync();
        isDuplicateErrorVisible.Should().BeTrue(
            because: "submitting a duplicate email must show a duplicate-email error on /signup");
        var isRegisterFormVisible = await registerPage.IsDisplayedAsync();
        isRegisterFormVisible.Should().BeFalse(
            because: "a duplicate email must block 'Enter Account Information' — only the error must be shown");
    }

    [Test]
    [Category("Negative")]
    [Description("Starting signup with an empty name uses HTML5 browser validation — form must not submit")]
    public async Task Register_WithEmptyName_FormDoesNotSubmitDueToHtmlValidation()
    {
        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();

        await loginPage.StartSignupAsync("", $"unique_{Guid.NewGuid():N}@example.com");
        loginPage.CurrentUrl.Should().Contain(TestConstants.Urls.Login,
            because: "HTML5 required-field validation must prevent signup form submission with empty name");
        var registerPage = CreatePage<RegisterPage>();
        var isRegisterVisible = await registerPage.IsDisplayedAsync();
        isRegisterVisible.Should().BeFalse(
            because: "an empty name must not allow navigation to the /signup registration form");
    }
}
