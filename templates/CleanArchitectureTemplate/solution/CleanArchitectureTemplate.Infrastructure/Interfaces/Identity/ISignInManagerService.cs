using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;

public interface ISignInManagerService
{
    #region From AccountController.cs

    Task<SignInResult> PasswordSignInAsync(
        ApplicationUser user,
        string password,
        bool isPersistent,
        bool lockoutOnFailure
    );

    Task SignInAsync(ApplicationUser user, bool isPersistent);

    Task SignOutAsync();

    AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string provider,
        string? redirectUrl
    );

    Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();

    Task<SignInResult> ExternalLoginSignInAsync(
        string loginProvider,
        string providerKey,
        bool isPersistent
    );

    Task UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo externalLogin);

    Task<ApplicationUser?> GetTwoFactorAuthenticationUserAsync();

    Task<SignInResult> TwoFactorSignInAsync(
        string provider,
        string code,
        bool isPersistent,
        bool rememberClient
    );

    Task<SignInResult> TwoFactorAuthenticatorSignInAsync(
        string code,
        bool isPersistent,
        bool rememberClient
    );
    Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode);

    #endregion

    #region From ManageController.cs

    Task<bool> IsTwoFactorClientRememberedAsync(ApplicationUser user);

    Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();

    AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string provider,
        string? redirectUrl,
        string? userId = null
    );

    Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null);

    #endregion
}
