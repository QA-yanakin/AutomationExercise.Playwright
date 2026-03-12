using AutomationExercise.Tests.Helpers;
using Microsoft.Playwright;

namespace AutomationExercise.Tests.Pages;
public class ContactUsPage : BasePage
{
    protected override string PagePath => TestConstants.Urls.ContactUs;

    public ContactUsPage(IPage page) : base(page) { }

    private ILocator ContactForm    => Page.Locator("#contact-us-form");
    private ILocator NameInput      => Page.Locator("[name='name']");
    private ILocator EmailInput     => Page.Locator("[name='email']");
    private ILocator SubjectInput   => Page.Locator("[name='subject']");
    private ILocator MessageArea    => Page.Locator("[name='message']");
    private ILocator SubmitButton   => Page.Locator("#contact-us-form [type='submit']");
    private ILocator SuccessMessage => Page.Locator("#contact-page .alert-success");
    private ILocator PageHeading    => Page.GetByText("Get In Touch", new() { Exact = true });

    public async Task OpenAsync()
    {
        await GoAsync();
        await WaitForVisibleAsync(ContactForm);
    }
    public async Task SubmitContactFormAsync(
        string name,
        string email,
        string subject,
        string message)
    {
        await FillAsync(NameInput,    name);
        await FillAsync(EmailInput,   email);
        await FillAsync(SubjectInput, subject);
        await FillAsync(MessageArea,  message);
        Page.Dialog += async (_, dialog) =>
        {
            if (dialog.Message.Contains("Press OK to proceed"))
                await dialog.AcceptAsync();
            else
                await dialog.DismissAsync();
        };

        await ClickAsync(SubmitButton);
        await WaitForVisibleAsync(SuccessMessage);
    }
    public async Task<bool> IsSubmissionSuccessfulAsync() =>
        await IsVisibleAsync(SuccessMessage);
    public async Task<string> GetSuccessMessageAsync() =>
        await GetTextAsync(SuccessMessage);
    public async Task<bool> IsDisplayedAsync() =>
        await IsVisibleAsync(ContactForm);
}
