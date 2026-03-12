using AutomationExercise.Tests.Helpers;
using AutomationExercise.Tests.Pages;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.UI;
[TestFixture]
[Category("Smoke")]
public class HomePageTests : BaseTest
{

    [Test]
    [Description("Verifies the home page loads correctly — URL, hero section visible, page title present.")]
    public async Task HomePage_WhenNavigated_DisplaysCorrectlyWithAllKeyElements()
    {
        var homePage = CreatePage<HomePage>();
        await homePage.OpenAsync();
        homePage.CurrentUrl.Should().Contain(
            TestSettings.BaseUrl,
            because: "navigation to home page should land on the base URL");
        var isDisplayed = await homePage.IsDisplayedAsync();
        isDisplayed.Should().BeTrue(
            because: "the home page hero carousel must be visible after successful load");
        var title = await homePage.GetPageTitleAsync();
        title.Should().NotBeNullOrWhiteSpace(
            because: "the browser tab title must be set — empty title indicates a load failure");

        title.Should().ContainEquivalentOf(
            "automation",
            because: "page title should identify the site as AutomationExercise");
        var isLoggedIn = await homePage.IsLoggedInAsync();
        isLoggedIn.Should().BeFalse(
            because: "a fresh browser context has no session — user must not appear as logged in");
    }

    [Test]
    [Description("Verifies that clicking 'Signup / Login' in the nav navigates to the login page.")]
    public async Task HomePage_WhenLoginNavClicked_NavigatesToLoginPage()
    {
        var homePage = CreatePage<HomePage>();
        await homePage.OpenAsync();
        await homePage.GoToLoginAsync();
        homePage.CurrentUrl.Should().Contain(
            TestConstants.Urls.Login,
            because: "clicking 'Signup / Login' must navigate to the /login route");
        homePage.CurrentUrl.Should().NotBe(
            TestSettings.BaseUrl + TestConstants.Urls.Home,
            because: "successful navigation must move away from the home page");
    }
}
