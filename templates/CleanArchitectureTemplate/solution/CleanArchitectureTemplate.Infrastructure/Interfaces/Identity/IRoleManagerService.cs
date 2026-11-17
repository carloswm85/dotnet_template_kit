using Microsoft.AspNetCore.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;

public interface IRoleManagerService
{
    #region From IdentityController.cs

    IQueryable<ApplicationRole> Roles { get; }

    Task<ApplicationRole?> FindByIdAsync(string roleId);

    Task<IdentityResult> CreateAsync(ApplicationRole role);

    Task<IdentityResult> UpdateAsync(ApplicationRole role);

    Task<IdentityResult> DeleteAsync(ApplicationRole role);

    #endregion

    /// <summary>
    /// Returns the number of users assigned to each role, keyed by role id.
    /// Roles without users are not included.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> GetUserCountsByRoleIdAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a composable query of user ids assigned to the named role.
    /// </summary>
    IQueryable<string> QueryUserIdsInRole(string roleName);

    /// <summary>
    /// Composable query of the ids of users whose roles are all within <paramref name="roleNames"/>
    /// (users without any role are included). Meant to be combined with other user queries
    /// on the same request scope, e.g. to find users that only hold the base `User` role.
    /// </summary>
    IQueryable<string> QueryUserIdsWithOnlyRoles(IReadOnlyCollection<string> roleNames);
}
