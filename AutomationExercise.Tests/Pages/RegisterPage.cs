using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class RegisterPage : BasePage
{
    protected override string PagePath => "/signup";

    public RegisterPage(IPage page) : base(page) { }
    private ILocator TitleMrRadio      => Page.Locator("#id_gender1");
    private ILocator TitleMrsRadio     => Page.Locator("#id_gender2");
    private ILocator NameInput         => Page.Locator("[data-qa='name']");
    private ILocator EmailInput        => Page.Locator("[data-qa='email']");
    private ILocator PasswordInput     => Page.Locator("[data-qa='password']");
    private ILocator DayDropdown       => Page.Locator("[data-qa='days']");
    private ILocator MonthDropdown     => Page.Locator("[data-qa='months']");
    private ILocator YearDropdown      => Page.Locator("[data-qa='years']");
    private ILocator NewsletterCheck    => Page.Locator("#newsletter");
    private ILocator SpecialOffersCheck => Page.Locator("#optin");
    private ILocator FirstNameInput    => Page.Locator("[data-qa='first_name']");
    private ILocator LastNameInput     => Page.Locator("[data-qa='last_name']");
    private ILocator CompanyInput      => Page.Locator("[data-qa='company']");
    private ILocator Address1Input     => Page.Locator("[data-qa='address']");
    private ILocator Address2Input     => Page.Locator("[data-qa='address2']");
    private ILocator CountryDropdown   => Page.Locator("[data-qa='country']");
    private ILocator StateInput        => Page.Locator("[data-qa='state']");
    private ILocator CityInput         => Page.Locator("[data-qa='city']");
    private ILocator ZipcodeInput      => Page.Locator("[data-qa='zipcode']");
    private ILocator MobileInput       => Page.Locator("[data-qa='mobile_number']");
    private ILocator CreateAccountButton => Page.Locator("[data-qa='create-account']");
    private ILocator AccountCreatedHeading => Page.GetByText(
        "Account Created!", new() { Exact = true });
    private ILocator ContinueButton    => Page.Locator("[data-qa='continue-button']");
    private ILocator PageHeading => Page.GetByText(
        "Enter Account Information", new() { Exact = true });
    private ILocator DuplicateEmailError => Page.GetByText(
        "Email Address already exist!", new() { Exact = false });
    public async Task WaitForPageAsync()
    {
        await WaitForUrlContainsAsync("/signup");
        await WaitForVisibleAsync(PageHeading);
    }
    public async Task FillAndSubmitAsync(FakeUser user)
    {
        var titleLocator = user.Title == "Mrs" ? TitleMrsRadio : TitleMrRadio;
        await ClickAsync(titleLocator);
        await FillAsync(NameInput, user.Name);
        await FillAsync(PasswordInput, user.Password);
        await SelectByTextAsync(DayDropdown, user.BirthDay);
        await SelectByTextAsync(MonthDropdown, user.BirthMonth);
        await SelectByTextAsync(YearDropdown, user.BirthYear);
        if (!await NewsletterCheck.IsCheckedAsync())
            await ClickAsync(NewsletterCheck);
        if (!await SpecialOffersCheck.IsCheckedAsync())
            await ClickAsync(SpecialOffersCheck);
        await FillAsync(FirstNameInput,  user.FirstName);
        await FillAsync(LastNameInput,   user.LastName);
        await FillAsync(CompanyInput,    user.Company);
        await FillAsync(Address1Input,   user.Address1);
        await FillAsync(Address2Input,   user.Address2);
        await SelectByTextAsync(CountryDropdown, user.Country);
        await FillAsync(StateInput,      user.State);
        await FillAsync(CityInput,       user.City);
        await FillAsync(ZipcodeInput,    user.Zipcode);
        await FillAsync(MobileInput,     user.Mobile);
        await ClickAsync(CreateAccountButton);
        await WaitForLoadAsync();
    }
    public async Task<bool> IsAccountCreatedAsync() =>
        await IsVisibleAsync(AccountCreatedHeading);
    public async Task<bool> IsDuplicateEmailErrorVisibleAsync() =>
        await IsVisibleAsync(DuplicateEmailError);
    public async Task<bool> IsDisplayedAsync() =>
        await IsVisibleAsync(PageHeading);
    public async Task ContinueAfterRegistrationAsync()
    {
        await ClickAsync(ContinueButton);
        await WaitForUrlContainsAsync(TestConstants.Urls.Home);
    }
}
