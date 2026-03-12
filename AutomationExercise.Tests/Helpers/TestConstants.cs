using Bogus;

namespace AutomationExercise.Tests.Helpers;
public static class TestConstants
{

    public static class Api
    {
        public const string ProductsList   = "/api/productsList";
        public const string BrandsList     = "/api/brandsList";
        public const string SearchProduct  = "/api/searchProduct";
        public const string CreateAccount  = "/api/createAccount";
        public const string DeleteAccount  = "/api/deleteAccount";
        public const string UpdateAccount  = "/api/updateAccount";
        public const string VerifyLogin    = "/api/verifyLogin";
        public const string GetUserByEmail = "/api/getUserDetailByEmail";
    }

    public static class Urls
    {
        public const string Home           = "/";
        public const string Login          = "/login";
        public const string Products       = "/products";
        public const string Cart           = "/view_cart";
        public const string Checkout       = "/checkout";
        public const string Payment        = "/payment";
        public const string ContactUs      = "/contact_us";
        public const string AccountCreated = "/account_created";
        public const string AccountDeleted = "/delete_account";
    }

    public static class Products
    {
        public const int    StableProductId   = 1;
        public const string StableProductName = "Blue Top";
        public const string SearchTerm        = "top";
        public const string NoResultsTerm     = "xyzzy_no_results_expected";
    }

    public static class Categories
    {
        public const string WomenParent = "Women";
        public const string MenParent   = "Men";
        public const string KidsParent  = "Kids";

        public const string WomenDress   = "Dress";
        public const string WomenTops    = "Tops";
        public const string MenTshirts   = "Tshirts";
        public const int    WomenDressId = 1;
        public const int    WomenTopsId  = 2;
        public const int    MenTshirtsId = 3;
    }

    public static class Brands
    {
        public const string Polo   = "Polo";
        public const string HAndM  = "H&M";
        public const string Madame = "Madame";
    }

    public static class ApiResponseCodes
    {
        public const int Success         = 200;
        public const int Created         = 201;
        public const int BadRequest      = 400;
        public const int NotFound        = 404;
        public const int MethodNotAllowed = 405;
    }

    public static class ApiMessages
    {
        public const string UserCreated         = "User created!";
        public const string UserDeleted         = "Account deleted!";
        public const string UserExists          = "User exists!";
        public const string UserNotFound            = "User not found!";
        public const string AccountNotFound         = "Account not found with this email, try another email!";
        public const string EmailAlreadyExists  = "Email already exists!";
        public const string MethodNotAllowed    = "This request method is not supported.";
        public const string MissingSearchParam  =
            "Bad request, search_product parameter is missing in POST request.";
    }
    public const string DefaultTestPassword = "Test@1234!";
    public static string UniqueEmail() =>
        $"user_{Guid.NewGuid():N}@example.com";
    public static FakeUser GenerateUser()
    {
        var faker = new Faker("en");
        return new FakeUser(
            Name:      faker.Name.FullName(),
            Email:     $"user_{Guid.NewGuid():N}@example.com",
            Password:  DefaultTestPassword,
            Title:     faker.PickRandom("Mr", "Mrs"),
            BirthDay:  faker.Random.Int(1, 28).ToString(),
            BirthMonth: faker.Date.Month().ToString(),
            BirthYear: faker.Random.Int(1970, 2000).ToString(),
            FirstName: faker.Name.FirstName(),
            LastName:  faker.Name.LastName(),
            Company:   faker.Company.CompanyName(),
            Address1:  faker.Address.StreetAddress(),
            Address2:  faker.Address.SecondaryAddress(),
            Country:   "United States",
            State:     faker.Address.State(),
            City:      faker.Address.City(),
            Zipcode:   faker.Address.ZipCode(),
            Mobile:    faker.Phone.PhoneNumber("##########")
        );
    }
}
public record FakeUser(
    string Name,
    string Email,
    string Password,
    string Title,
    string BirthDay,
    string BirthMonth,
    string BirthYear,
    string FirstName,
    string LastName,
    string Company,
    string Address1,
    string Address2,
    string Country,
    string State,
    string City,
    string Zipcode,
    string Mobile
)
{
    public Dictionary<string, string> ToFormData() => new()
    {
        ["name"]       = Name,
        ["email"]      = Email,
        ["password"]   = Password,
        ["title"]      = Title,
        ["birth_date"] = BirthDay,
        ["birth_month"]= BirthMonth,
        ["birth_year"] = BirthYear,
        ["firstname"]  = FirstName,
        ["lastname"]   = LastName,
        ["company"]    = Company,
        ["address1"]   = Address1,
        ["address2"]   = Address2,
        ["country"]    = Country,
        ["state"]      = State,
        ["city"]       = City,
        ["zipcode"]    = Zipcode,
        ["mobile_number"] = Mobile
    };
}
