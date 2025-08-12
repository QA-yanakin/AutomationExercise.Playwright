using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExercise.Playwright.Pages
{
    /// <summary>Account Information page after starting signup.</summary>
    public class RegisterAccountPage : BasePage
    {
        private ILocator TitleMr => Page.Locator("#id_gender1");
        private ILocator Password => Page.Locator("#password");
        private ILocator FirstName => Page.Locator("#first_name");
        private ILocator LastName => Page.Locator("#last_name");
        private ILocator Address1 => Page.Locator("#address1");
        private ILocator Country => Page.Locator("#country");
        private ILocator State => Page.Locator("#state");
        private ILocator City => Page.Locator("#city");
        private ILocator Zipcode => Page.Locator("#zipcode");
        private ILocator MobileNumber => Page.Locator("#mobile_number");
        private ILocator CreateAccountButton => Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" });

        public RegisterAccountPage(IPage page, string baseUrl) : base(page, baseUrl) { }

        /// <summary>Fill required fields and submit account creation.</summary>
        public async Task CompleteRequiredFieldsAndSubmitAsync(string password, string firstName, string lastName, string address, string country, string state, string city, string zipcode, string mobile)
        {
            await TitleMr.CheckAsync();
            await Password.FillAsync(password);
            await FirstName.FillAsync(firstName);
            await LastName.FillAsync(lastName);
            await Address1.FillAsync(address);
            await Country.SelectOptionAsync(new() { Label = country });
            await State.FillAsync(state);
            await City.FillAsync(city);
            await Zipcode.FillAsync(zipcode);
            await MobileNumber.FillAsync(mobile);
            await CreateAccountButton.ClickAsync();
        }
    }
}
