using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

// Requirement: user registration and login (success and failure)
public class AuthTests : ApiTestBase
{
    private static object NewUser(string role, string email = "new.runner@example.com") => new
    {
        firstName = "New",
        lastName = "Runner",
        email,
        password = "Secret123!",
        role
    };

    [Fact]
    public async Task Register_NewParticipant_Returns201WithProfile()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register", NewUser("Participant"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal("new.runner@example.com", profile!.Email);
        Assert.Equal("Participant", profile.Role);
    }

    [Fact]
    public async Task Register_NewOrganiser_Returns201WithOrganiserRole()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register", NewUser("organiser"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal("Organiser", profile!.Role);
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotThePlainPassword()
    {
        await AnonymousClient().PostAsJsonAsync("/api/auth/register", NewUser("Participant"));

        var storedHash = InDatabase(db => db.Users.Single(u => u.Email == "new.runner@example.com").PasswordHash);
        Assert.NotEqual("Secret123!", storedHash);
        Assert.StartsWith("$2", storedHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Secret123!", storedHash));
    }

    [Fact]
    public async Task Register_EmailAlreadyUsed_Returns409()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register", NewUser("Participant", Participant));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidRole_Returns400()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register", NewUser("Admin"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Short",
            lastName = "Password",
            email = "short@example.com",
            password = "abc",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_SeededOrganiser_Returns200AndStartsSession()
    {
        var client = AnonymousClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = Organiser, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.Equal("Organiser", body!.Role);

        // The session cookie now identifies the user on the next request
        var me = await client.GetFromJsonAsync<UserProfileResponse>("/api/users/me");
        Assert.Equal(Organiser, me!.Email);
        Assert.Equal("Organiser", me.Role);
    }

    [Fact]
    public async Task RegisterThenLogin_NewParticipant_SessionHasParticipantRole()
    {
        var client = AnonymousClient();
        await client.PostAsJsonAsync("/api/auth/register", NewUser("Participant"));

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "new.runner@example.com", password = "Secret123!" });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.Equal("Participant", body!.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/login", new { email = Organiser, password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/auth/login", new { email = "nobody@example.com", password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WithoutLogin_Returns401()
    {
        var response = await AnonymousClient().GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_LoggedIn_Returns200AndSavesChanges()
    {
        var client = await LoginAsync(Participant);

        var response = await client.PutAsJsonAsync("/api/users/me", new
        {
            firstName = "Sipho",
            lastName = "Dlamini",
            phoneNumber = "+27710000000",
            dateOfBirth = "1990-04-12",
            city = "Cape Town",
            emergencyContactName = "Nomsa Dlamini",
            emergencyContactPhone = "+27715550203"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Cape Town", InDatabase(db => db.Users.Single(u => u.UserId == 3).City));
    }
}
