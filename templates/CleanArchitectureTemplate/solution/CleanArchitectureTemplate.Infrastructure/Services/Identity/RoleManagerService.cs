using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Services.Identity;

public class RoleManagerService : IRoleManagerService
{
    #region From IdentityController.cs

    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public RoleManagerService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public IQueryable<ApplicationRole> Roles => _roleManager.Roles;

    public async Task<ApplicationRole?> FindByIdAsync(string roleId)
    {
        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<IdentityResult> CreateAsync(ApplicationRole role)
    {
        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationRole role)
    {
        return await _roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationRole role)
    {
        return await _roleManager.DeleteAsync(role);
    }

    #endregion

    public async Task<IReadOnlyDictionary<string, int>> GetUserCountsByRoleIdAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .UserRoles.AsNoTracking()
            .GroupBy(userRole => userRole.RoleId)
            .Select(group => new { RoleId = group.Key, UserCount = group.Count() })
            .ToDictionaryAsync(item => item.RoleId, item => item.UserCount, cancellationToken);
    }

    public IQueryable<string> QueryUserIdsInRole(string roleName)
    {
        var roleIds = _context
            .Roles.Where(role => role.Name == roleName)
            .Select(role => role.Id);

        return _context
            .UserRoles.Where(userRole => roleIds.Contains(userRole.RoleId))
            .Select(userRole => userRole.UserId);
    }

    public IQueryable<string> QueryUserIdsWithOnlyRoles(IReadOnlyCollection<string> roleNames)
    {
        var allowedRoleIds = _context
            .Roles.Where(role => role.Name != null && roleNames.Contains(role.Name))
            .Select(role => role.Id);

        return _context
            .Users.Where(user =>
                !_context.UserRoles.Any(userRole =>
                    userRole.UserId == user.Id && !allowedRoleIds.Contains(userRole.RoleId)
                )
            )
            .Select(user => user.Id);
    }
}
