using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Services.Identity;

public class UserManagerService(UserManager<ApplicationUser> userManager) : IUserManagerService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    public IQueryable<ApplicationUser> Users => _userManager.Users;

    #region From AccountController.cs

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }

    // New users always receive the base User role; if that fails the user is
    // deleted so no account is left without a role.
    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        return await CreateWithBaseRoleAsync(user, () => _userManager.CreateAsync(user, password));
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        return await CreateWithBaseRoleAsync(user, () => _userManager.CreateAsync(user));
    }

    public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code)
    {
        return await _userManager.ConfirmEmailAsync(user, code);
    }

    public async Task<IdentityResult> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    )
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IList<string>> GetValidTwoFactorProvidersAsync(ApplicationUser user)
    {
        return await _userManager.GetValidTwoFactorProvidersAsync(user);
    }

    public async Task<string> GenerateTwoFactorTokenAsync(
        ApplicationUser user,
        string tokenProvider
    )
    {
        return await _userManager.GenerateTwoFactorTokenAsync(user, tokenProvider);
    }

    public async Task<string?> GetEmailAsync(ApplicationUser user)
    {
        return await _userManager.GetEmailAsync(user);
    }

    public async Task<string?> GetPhoneNumberAsync(ApplicationUser user)
    {
        return await _userManager.GetPhoneNumberAsync(user);
    }

    public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login)
    {
        return await _userManager.AddLoginAsync(user, login);
    }

    #endregion

    #region From IdentityController.cs

    public async Task<ApplicationUser?> FindByNameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        return await _userManager.GetClaimsAsync(user);
    }

    public async Task<IdentityResult> UpdateSecurityStampAsync(ApplicationUser user)
    {
        return await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<IdentityResult> SetEmailAsync(ApplicationUser user, string email)
    {
        return await _userManager.SetEmailAsync(user, email);
    }

    public async Task<IdentityResult> SetUserNameAsync(ApplicationUser user, string username)
    {
        return await _userManager.SetUserNameAsync(user, username);
    }

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> AddToRolesAsync(
        ApplicationUser user,
        IEnumerable<string> roles
    )
    {
        return await _userManager.AddToRolesAsync(user, roles);
    }

    public async Task<IdentityResult> RemoveFromRolesAsync(
        ApplicationUser user,
        IEnumerable<string> roles
    )
    {
        var removableRoles = roles
            .Where(role =>
                !string.Equals(role, RoleNames.User, StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        if (removableRoles.Length == 0)
            return IdentityResult.Success;

        return await _userManager.RemoveFromRolesAsync(user, removableRoles);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    #endregion

    #region From ManageController.cs

    public async Task<string?> GetUserIdAsync(ApplicationUser user)
    {
        return await _userManager.GetUserIdAsync(user);
    }

    public string? GetUserId(ClaimsPrincipal principal)
    {
        return _userManager.GetUserId(principal);
    }

    public async Task<bool> HasPasswordAsync(ApplicationUser user)
    {
        return await _userManager.HasPasswordAsync(user);
    }

    public async Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
    {
        return await _userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
    {
        return await _userManager.GetLoginsAsync(user);
    }

    public async Task<string?> GetAuthenticatorKeyAsync(ApplicationUser user)
    {
        return await _userManager.GetAuthenticatorKeyAsync(user);
    }

    public async Task<IdentityResult> RemoveLoginAsync(
        ApplicationUser user,
        string loginProvider,
        string providerKey
    )
    {
        return await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
    }

    public async Task<string> GenerateChangePhoneNumberTokenAsync(
        ApplicationUser user,
        string phoneNumber
    )
    {
        return await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
    }

    public async Task ResetAuthenticatorKeyAsync(ApplicationUser user)
    {
        await _userManager.ResetAuthenticatorKeyAsync(user);
    }

    public async Task<IEnumerable<string>?> GenerateNewTwoFactorRecoveryCodesAsync(
        ApplicationUser user,
        int number
    )
    {
        return await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, number);
    }

    public async Task<IdentityResult> SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
    {
        return await _userManager.SetTwoFactorEnabledAsync(user, enabled);
    }

    public async Task<IdentityResult> ChangePhoneNumberAsync(
        ApplicationUser user,
        string phoneNumber,
        string token
    )
    {
        return await _userManager.ChangePhoneNumberAsync(user, phoneNumber, token);
    }

    public async Task<IdentityResult> SetPhoneNumberAsync(ApplicationUser user, string? phoneNumber)
    {
        return await _userManager.SetPhoneNumberAsync(user, phoneNumber);
    }

    public async Task<IdentityResult> ChangePasswordAsync(
        ApplicationUser user,
        string currentPassword,
        string newPassword
    )
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.AddPasswordAsync(user, password);
    }

    #endregion

    private async Task<IdentityResult> CreateWithBaseRoleAsync(
        ApplicationUser user,
        Func<Task<IdentityResult>> createUser
    )
    {
        var createResult = await createUser();
        if (!createResult.Succeeded)
            return createResult;

        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (roleResult.Succeeded)
            return createResult;

        var deleteResult = await _userManager.DeleteAsync(user);
        if (deleteResult.Succeeded)
            return roleResult;

        return IdentityResult.Failed([.. roleResult.Errors, .. deleteResult.Errors]);
    }
}
