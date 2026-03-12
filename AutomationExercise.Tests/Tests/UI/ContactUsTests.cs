using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("UI")]
[Category("Regression")]
public class ContactUsTests : BaseTest
{
    [Test]
    [Description("Submitting the Contact Us form with valid data must show a success message")]
    public async Task ContactUs_WithValidData_ShowsSuccessMessage()
    {
        var contactPage = CreatePage<ContactUsPage>();
        await contactPage.OpenAsync();

        var isDisplayed = await contactPage.IsDisplayedAsync();
        isDisplayed.Should().BeTrue(
            because: "the Contact Us page must load with the contact form visible");
        await contactPage.SubmitContactFormAsync(
            name:    "Test Automation",
            email:   TestConstants.UniqueEmail(),
            subject: "Automated test enquiry",
            message: "This is an automated test message. Please ignore.");
        var isSuccess = await contactPage.IsSubmissionSuccessfulAsync();
        isSuccess.Should().BeTrue(
            because: "a valid form submission must show the success confirmation message");

        var successText = await contactPage.GetSuccessMessageAsync();
        successText.Should().Contain("Success",
            because: "the success banner must contain 'Success' to confirm the submission was received");
    }
}
