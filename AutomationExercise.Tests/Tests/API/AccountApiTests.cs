using AutomationExercise.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests.API;
[TestFixture]
[Parallelizable(ParallelScope.All)]
[Category("API")]
[Category("Regression")]
public class AccountApiTests : BaseApiTest
{

    [Test]
    [Description("POST createAccount with valid data must return 201 and confirm creation")]
    public async Task CreateAccount_WithValidData_Returns201UserCreated()
    {
        var user = TestConstants.GenerateUser();
        try
        {
            var response = await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

            response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Created,
                because: "a new account must return responseCode 201");

            response.GetString("message").Should().Be(TestConstants.ApiMessages.UserCreated,
                because: "the confirmation message must match the documented API response");

            response.ElapsedMs.Should().BeLessThan(ResponseTimeSlaMs,
                because: $"account creation must complete within {ResponseTimeSlaMs}ms SLA");
        }
        finally
        {
            await CleanupUserAsync(user.Email, user.Password);
        }
    }

    [Test]
    [Category("Negative")]
    [Description("POST createAccount with an already-registered email must return duplicate error")]
    public async Task CreateAccount_WithDuplicateEmail_ReturnsEmailAlreadyExistsError()
    {
        var user = TestConstants.GenerateUser();
        await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

        try
        {
            var response = await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

            response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.BadRequest,
                because: "duplicate email registration must return responseCode 400");

            response.GetString("message").Should().Be(TestConstants.ApiMessages.EmailAlreadyExists,
                because: "the error message must identify that the email is already registered");
        }
        finally
        {
            await CleanupUserAsync(user.Email, user.Password);
        }
    }

    [Test]
    [Description("DELETE deleteAccount with valid credentials must return 200 and confirm deletion")]
    public async Task DeleteAccount_WithValidCredentials_Returns200AccountDeleted()
    {
        var user = TestConstants.GenerateUser();
        await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

        var response = await DeleteFormAsync(TestConstants.Api.DeleteAccount, new()
        {
            ["email"]    = user.Email,
            ["password"] = user.Password
        });

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
            because: "successful account deletion must return responseCode 200");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.UserDeleted,
            because: "the confirmation message must match the documented API response");
    }

    [Test]
    [Category("Negative")]
    [Description("DELETE deleteAccount with wrong password must return an error — not silently succeed")]
    public async Task DeleteAccount_WithWrongPassword_ReturnsError()
    {
        var user = TestConstants.GenerateUser();
        await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

        try
        {
            var response = await DeleteFormAsync(TestConstants.Api.DeleteAccount, new()
            {
                ["email"]    = user.Email,
                ["password"] = "WrongPassword999!"
            });
            response.GetInt("responseCode").Should().NotBe(TestConstants.ApiResponseCodes.Success,
                because: "deleting with wrong password must not return a success responseCode");
        }
        finally
        {
            await CleanupUserAsync(user.Email, user.Password);
        }
    }

    [Test]
    [Description("GET getUserDetailByEmail with a valid email must return user profile data")]
    public async Task GetUserByEmail_WithValidEmail_ReturnsUserProfile()
    {
        var user = TestConstants.GenerateUser();
        await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

        try
        {
            var response = await GetAsync(
                $"{TestConstants.Api.GetUserByEmail}?email={Uri.EscapeDataString(user.Email)}");

            response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
                because: "an existing user lookup must return responseCode 200");

            response.HasProperty("user", System.Text.Json.JsonValueKind.Object)
                .Should().BeTrue(because: "response must contain a 'user' object");

            var returnedEmail = response.WithJson(root =>
                root.TryGetProperty("user", out var u)
                && u.TryGetProperty("email", out var e) ? e.GetString() : null);
            returnedEmail.Should().Be(user.Email,
                because: "returned email must match the queried email exactly");

            response.ElapsedMs.Should().BeLessThan(ResponseTimeSlaMs,
                because: $"user lookup must complete within {ResponseTimeSlaMs}ms SLA");
        }
        finally
        {
            await CleanupUserAsync(user.Email, user.Password);
        }
    }

    [Test]
    [Category("Negative")]
    [Description("GET getUserDetailByEmail with a non-existent email must return 404 User Not Found")]
    public async Task GetUserByEmail_WithNonExistentEmail_Returns404()
    {
        var fakeEmail = $"nonexistent_{Guid.NewGuid():N}@example.com";

        var response = await GetAsync(
            $"{TestConstants.Api.GetUserByEmail}?email={Uri.EscapeDataString(fakeEmail)}");

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.NotFound,
            because: "querying a non-existent user must return responseCode 404");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.AccountNotFound,
            because: "getUserDetailByEmail uses its own not-found message — different from verifyLogin");
    }

    [Test]
    [Category("Negative")]
    [Description("GET getUserDetailByEmail with a malformed email (no @ sign, unique) " +
                 "must not return a user object. " +
                 "NOTE: API does not validate email format — any string can be stored as email on this practice site. " +
                 "Using a Guid-based string to ensure it cannot match any existing user.")]
    public async Task GetUserByEmail_WithMalformedEmail_DoesNotReturnUserData()
    {
        var malformedEmail = $"notanemail-{Guid.NewGuid():N}";

        var response = await GetAsync(
            $"{TestConstants.Api.GetUserByEmail}?email={Uri.EscapeDataString(malformedEmail)}");
        var hasUser = response.HasProperty("user", System.Text.Json.JsonValueKind.Object);

        hasUser.Should().BeFalse(
            because: $"'{malformedEmail}' is not registered — no user object must be returned");
    }

    [Test]
    [Description("POST verifyLogin with valid credentials must confirm user exists")]
    public async Task VerifyLogin_WithValidCredentials_ReturnsUserExists()
    {
        var user = TestConstants.GenerateUser();
        await PostFormAsync(TestConstants.Api.CreateAccount, user.ToFormData());

        try
        {
            var response = await PostFormAsync(TestConstants.Api.VerifyLogin, new()
            {
                ["email"]    = user.Email,
                ["password"] = user.Password
            });

            response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.Success,
                because: "valid credentials must return responseCode 200");

            response.GetString("message").Should().Be(TestConstants.ApiMessages.UserExists,
                because: "successful login verification must confirm the user exists");
        }
        finally
        {
            await CleanupUserAsync(user.Email, user.Password);
        }
    }

    [Test]
    [Category("Negative")]
    [Description("POST verifyLogin with invalid credentials must return 404 User Not Found")]
    public async Task VerifyLogin_WithInvalidCredentials_ReturnsUserNotFound()
    {
        var response = await PostFormAsync(TestConstants.Api.VerifyLogin, new()
        {
            ["email"]    = $"nobody_{Guid.NewGuid():N}@example.com",
            ["password"] = "WrongPassword999!"
        });

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.NotFound,
            because: "invalid credentials must return responseCode 404");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.UserNotFound,
            because: "verifyLogin with non-existent credentials must return 'User not found!' — " +
                     "note: different from getUserDetailByEmail which returns 'Account not found...'");
    }

    [Test]
    [Category("Negative")]
    [Description("POST verifyLogin with missing email must return 400 Bad Request")]
    public async Task VerifyLogin_WithMissingEmail_Returns400()
    {
        var response = await PostFormAsync(TestConstants.Api.VerifyLogin, new()
        {
            ["password"] = TestConstants.DefaultTestPassword
        });

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.BadRequest,
            because: "missing email parameter must return responseCode 400");
    }

    [Test]
    [Category("Negative")]
    [Description("DELETE to verifyLogin (POST-only endpoint) must return 405 Method Not Allowed")]
    public async Task VerifyLogin_WithDeleteMethod_Returns405()
    {
        var response = await DeleteFormAsync(TestConstants.Api.VerifyLogin, new()
        {
            ["email"]    = "any@example.com",
            ["password"] = "anyPassword"
        });

        response.GetInt("responseCode").Should().Be(TestConstants.ApiResponseCodes.MethodNotAllowed,
            because: "DELETE is not supported on /api/verifyLogin — must return responseCode 405");

        response.GetString("message").Should().Be(TestConstants.ApiMessages.MethodNotAllowed,
            because: "the error message must explicitly state the method is not supported");
    }

    private async Task CleanupUserAsync(string email, string password)
    {
        await DeleteFormAsync(TestConstants.Api.DeleteAccount, new()
        {
            ["email"]    = email,
            ["password"] = password
        });
    }
}
