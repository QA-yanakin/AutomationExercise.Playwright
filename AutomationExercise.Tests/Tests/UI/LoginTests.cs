using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("UI")]
[Category("Regression")]
public class LoginTests : BaseTest
{
    private static FakeUser _testUser = null!;

    [OneTimeSetUp]
    public async Task CreateTestUserViaApiAsync()
    {
        _testUser = TestConstants.GenerateUser();
        await ApiCleanupHelper.CreateUserAsync(_testUser);
    }

    [OneTimeTearDown]
    public async Task DeleteTestUserViaApiAsync()
    {
        if (_testUser is not null)
            await ApiCleanupHelper.DeleteUserAsync(_testUser);
    }

    [Test]
    [Category("Smoke")]
    [Description("Login with valid credentials must redirect to home page and show 'Logged in as' badge")]
    public async Task Login_WithValidCredentials_RedirectsToHomeAndShowsLoggedInBadge()
    {
        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();

        loginPage.CurrentUrl.Should().Contain(TestConstants.Urls.Login,
            because: "navigation to /login must land on the login page");

        var isDisplayed = await loginPage.IsDisplayedAsync();
        isDisplayed.Should().BeTrue(
            because: "both Login and Signup headings must be visible on the login page");
        await loginPage.LoginAndWaitForHomeAsync(_testUser.Email, _testUser.Password);
        loginPage.CurrentUrl.Should().Contain(TestSettings.BaseUrl,
            because: "successful login must redirect away from /login");

        loginPage.CurrentUrl.Should().NotContain(TestConstants.Urls.Login,
            because: "after login the user must not remain on the login page");
        var homePage = CreatePage<HomePage>();
        var isLoggedIn = await homePage.IsLoggedInAsync();
        isLoggedIn.Should().BeTrue(
            because: "the nav bar must show 'Logged in as' after a successful login");

        var username = await homePage.GetLoggedInUsernameAsync();
        username.Should().NotBeNullOrWhiteSpace(
            because: "a username must appear in the 'Logged in as' nav badge after login");
    }

    [Test]
    [Description("After login, clicking Logout must redirect to /login and remove the session")]
    public async Task Logout_WhenLoggedIn_RedirectsToLoginAndClearsSession()
    {
        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();
        await loginPage.LoginAndWaitForHomeAsync(_testUser.Email, _testUser.Password);
        var homePage = CreatePage<HomePage>();
        await homePage.LogoutAsync();
        homePage.CurrentUrl.Should().Contain(TestConstants.Urls.Login,
            because: "logout must redirect to /login");
        var isLoggedIn = await homePage.IsLoggedInAsync();
        isLoggedIn.Should().BeFalse(
            because: "the 'Logged in as' badge must not appear after logout");
    }
    private const string _testUserEmailPlaceholder = "RUNTIME_TEST_USER_EMAIL";
    private static readonly object[] InvalidCredentialsCases =
    [
        new object[] { "wrong@example.com",       "wrongpassword",    "non-existent email" },
        new object[] { _testUserEmailPlaceholder,  "WrongPassword99!", "wrong password for valid email" },
    ];

    [Test]
    [Category("Negative")]
    [TestCaseSource(nameof(InvalidCredentialsCases))]
    [Description("Login with wrong credentials must stay on /login and show the app error message")]
    public async Task Login_WithWrongCredentials_ShowsErrorAndStaysOnLoginPage(
        string email, string password, string scenario)
    {
        var resolvedEmail = email == _testUserEmailPlaceholder ? _testUser.Email : email;

        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();

        await loginPage.LoginAsync(resolvedEmail, password);

        loginPage.CurrentUrl.Should().Contain(TestConstants.Urls.Login,
            because: $"wrong credentials ({scenario}) must not redirect away from /login");

        var isErrorVisible = await loginPage.IsLoginErrorVisibleAsync();
        isErrorVisible.Should().BeTrue(
            because: $"app error message must appear for invalid credentials ({scenario})");

        var errorText = await loginPage.GetLoginErrorTextAsync();
        errorText.Should().NotBeNullOrWhiteSpace(
            because: $"error text must not be empty ({scenario})");
    }

    [Test]
    [Category("Negative")]
    [Description("Login with empty fields uses HTML5 browser validation — form must not submit")]
    public async Task Login_WithEmptyFields_FormDoesNotSubmit()
    {
        var loginPage = CreatePage<LoginPage>();
        await loginPage.OpenAsync();
        await loginPage.LoginAsync("", "");
        loginPage.CurrentUrl.Should().Contain(TestConstants.Urls.Login,
            because: "HTML5 required-field validation must prevent form submission with empty fields");
        var isAppErrorVisible = await loginPage.IsLoginErrorVisibleAsync();
        isAppErrorVisible.Should().BeFalse(
            because: "empty fields are caught by browser validation — the server error message must not appear");
    }
}
