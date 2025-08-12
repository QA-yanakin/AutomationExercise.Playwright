using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Signup/Login page object.</summary>
    public class LoginPage : BasePage
    {
        // Login section
        private ILocator LoginEmail => Page.GetByPlaceholder("Email Address");
        private ILocator LoginPassword => Page.GetByPlaceholder("Password");
        private ILocator LoginButton => Page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        private ILocator LoginError => Page.Locator("#form .login-form p");

        // Signup section
        private ILocator SignupName => Page.GetByPlaceholder("Name");
        private ILocator SignupEmail => Page.Locator("#form input[name='email']");
        private ILocator SignupButton => Page.GetByRole(AriaRole.Button, new() { Name = "Signup" });
        private ILocator SignupError => Page.Locator("#form .signup-form p");

        public LoginPage(IPage page, string baseUrl) : base(page, baseUrl) { }

        /// <summary>Open the Signup/Login page directly.</summary>
        public async Task OpenAsync() => await GotoAsync("/login");

        /// <summary>Attempt login; returns inline error text if present, else null.</summary>
        public async Task<string?> LoginAsync(string email, string password)
        {
            await LoginEmail.FillAsync(email);
            await LoginPassword.FillAsync(password);
            await LoginButton.ClickAsync();
            if (await LoginError.IsVisibleAsync()) return await LoginError.InnerTextAsync();
            return null;
        }

        /// <summary>Start signup; returns error text if email already exists.</summary>
        public async Task<string?> BeginSignupAsync(string name, string email)
        {
            await SignupName.FillAsync(name);
            await SignupEmail.FillAsync(email);
            await SignupButton.ClickAsync();
            if (await SignupError.IsVisibleAsync()) return await SignupError.InnerTextAsync();
            return null;
        }
    }
}
