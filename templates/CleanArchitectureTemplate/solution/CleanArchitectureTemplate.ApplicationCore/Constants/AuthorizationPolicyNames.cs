namespace CleanArchitectureTemplate.ApplicationCore.Constants;

/// <summary>
/// Named authorization policies, registered by
/// <c>AuthorizationPolicyExtensions.AddApplicationPolicies</c>.
/// Prefer policies over role lists in <c>[Authorize]</c> so the roles behind
/// a permission change in one place.
/// </summary>
public static class AuthorizationPolicyNames
{
    /// <summary>Administration pages (SuperAdmin, Admin).</summary>
    public const string AdministrationAccess = "AdministrationAccess";

    /// <summary>User and role management (SuperAdmin).</summary>
    public const string IdentityAdministration = "IdentityAdministration";

    /// <summary>Changing rows of the AppSetting table (SuperAdmin).</summary>
    public const string ManageAppSettings = "ManageAppSettings";
}
