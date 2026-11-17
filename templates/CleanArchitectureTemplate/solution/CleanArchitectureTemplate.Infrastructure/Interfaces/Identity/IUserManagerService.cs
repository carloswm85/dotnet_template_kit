using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;

/// <summary>
/// The following methods were extracte from controllers in the following file implementations:
/// - AccountController.cs
/// - IdentityController.cs
/// - ManageController.cs
/// Available at:
/// TODO Include correct file reference from GitHub
/// </summary>
public interface IUserManagerService
{
    #region From AccountController.cs

    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> CreateAsync(ApplicationUser user);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<IList<string>> GetValidTwoFactorProvidersAsync(ApplicationUser user);
    Task<string> GenerateTwoFactorTokenAsync(ApplicationUser user, string tokenProvider);
    Task<string?> GetEmailAsync(ApplicationUser user);
    Task<string?> GetPhoneNumberAsync(ApplicationUser user);
    Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login);

    #endregion

    #region From IdentityController.cs

    IQueryable<ApplicationUser> Users { get; }
    Task<ApplicationUser?> FindByNameAsync(string username);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IList<Claim>> GetClaimsAsync(ApplicationUser user);
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<IdentityResult> UpdateSecurityStampAsync(ApplicationUser user);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<IdentityResult> SetEmailAsync(ApplicationUser user, string email);
    Task<IdentityResult> SetUserNameAsync(ApplicationUser user, string username);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

    #endregion

    #region From ManageController.cs

    Task<string?> GetUserIdAsync(ApplicationUser user);

    string? GetUserId(ClaimsPrincipal principal);

    Task<bool> HasPasswordAsync(ApplicationUser user);

    Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user);

    Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);

    Task<string?> GetAuthenticatorKeyAsync(ApplicationUser user);

    Task<IdentityResult> RemoveLoginAsync(
        ApplicationUser user,
        string loginProvider,
        string providerKey
    );

    Task<string> GenerateChangePhoneNumberTokenAsync(ApplicationUser user, string phoneNumber);

    Task ResetAuthenticatorKeyAsync(ApplicationUser user);

    Task<IEnumerable<string>?> GenerateNewTwoFactorRecoveryCodesAsync(
        ApplicationUser user,
        int number
    );

    Task<IdentityResult> SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled);

    Task<IdentityResult> ChangePhoneNumberAsync(
        ApplicationUser user,
        string phoneNumber,
        string token
    );

    Task<IdentityResult> SetPhoneNumberAsync(ApplicationUser user, string? phoneNumber);

    Task<IdentityResult> ChangePasswordAsync(
        ApplicationUser user,
        string currentPassword,
        string newPassword
    );

    Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password);

    #endregion
}
