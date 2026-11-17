using CleanArchitectureTemplate.ApplicationCore.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitectureTemplate.ApplicationCore.Extensions;

public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Registers the policies in <see cref="AuthorizationPolicyNames"/>.
    /// Shared by the Web and API hosts so both enforce the same rules.
    /// </summary>
    public static AuthorizationOptions AddApplicationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(
            AuthorizationPolicyNames.AdministrationAccess,
            policy => policy.RequireRole(RoleNames.SuperAdmin, RoleNames.Admin)
        );

        options.AddPolicy(
            AuthorizationPolicyNames.IdentityAdministration,
            policy => policy.RequireRole(RoleNames.SuperAdmin)
        );

        options.AddPolicy(
            AuthorizationPolicyNames.ManageAppSettings,
            policy => policy.RequireRole(RoleNames.SuperAdmin)
        );

        return options;
    }
}
