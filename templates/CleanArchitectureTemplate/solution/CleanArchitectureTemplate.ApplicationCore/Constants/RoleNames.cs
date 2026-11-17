namespace CleanArchitectureTemplate.ApplicationCore.Constants;

/// <summary>
/// Application role names. Declared as <c>const</c> so they can be used in
/// attributes, for example <c>[Authorize(Roles = RoleNames.Admin)]</c>.
/// </summary>
public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
}
