namespace RaceDay.Api.Auth;

// Names of the values stored in the server-side session after login
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Role = "Role";
}

// The two roles in the system (they match the Roles table)
public static class Roles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}
