using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class LoginPage : BasePage
{
    protected override string PagePath => TestConstants.Urls.Login;

    public LoginPage(IPage page) : base(page) { }
    private ILocator LoginEmailInput    => Page.Locator("[data-qa='login-email']");
    private ILocator LoginPasswordInput => Page.Locator("[data-qa='login-password']");
    private ILocator LoginButton        => Page.Locator("[data-qa='login-button']");
    private ILocator LoginErrorMessage  => Page.GetByText(
        "Your email or password is incorrect!", new() { Exact = true });
    private ILocator SignupNameInput    => Page.Locator("[data-qa='signup-name']");
    private ILocator SignupEmailInput   => Page.Locator("[data-qa='signup-email']");
    private ILocator SignupButton       => Page.Locator("[data-qa='signup-button']");
    private ILocator LoginFormHeading   => Page.GetByText("Login to your account",
        new() { Exact = true });
    private ILocator SignupFormHeading  => Page.GetByText("New User Signup!",
        new() { Exact = true });

    public async Task OpenAsync()
    {
        await GoAsync();
        await WaitForVisibleAsync(LoginFormHeading);
    }
    public async Task<string> LoginAsync(string email, string password)
    {
        await FillAsync(LoginEmailInput, email);
        await FillAsync(LoginPasswordInput, password);
        await ClickAsync(LoginButton);
        await WaitForLoadAsync();
        return CurrentUrl;
    }
    public async Task LoginAndWaitForHomeAsync(string email, string password)
    {
        await LoginAsync(email, password);
        await WaitForUrlContainsAsync(TestConstants.Urls.Home);
    }
    public async Task<string> LoginAndGetErrorMessageAsync(string email, string password)
    {
        await LoginAsync(email, password);
        await WaitForVisibleAsync(LoginErrorMessage);
        return await GetTextAsync(LoginErrorMessage);
    }
    public async Task StartSignupAsync(string name, string email)
    {
        await FillAsync(SignupNameInput, name);
        await FillAsync(SignupEmailInput, email);
        await ClickAsync(SignupButton);
        await WaitForLoadAsync();
    }
    public async Task<bool> IsDisplayedAsync() =>
        await IsVisibleAsync(LoginFormHeading) && await IsVisibleAsync(SignupFormHeading);
    public async Task<bool> IsLoginErrorVisibleAsync() =>
        await IsVisibleAsync(LoginErrorMessage);
    public async Task<string?> GetLoginErrorTextAsync() =>
        await IsLoginErrorVisibleAsync()
            ? await GetTextAsync(LoginErrorMessage)
            : null;
}
