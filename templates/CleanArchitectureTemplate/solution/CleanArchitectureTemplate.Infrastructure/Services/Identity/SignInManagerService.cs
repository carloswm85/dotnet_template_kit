using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Services.Identity;

public class SignInManagerService(SignInManager<ApplicationUser> signInManager)
    : ISignInManagerService
{
    #region For AccountController.cs

    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

    public async Task<SignInResult> PasswordSignInAsync(
        ApplicationUser user,
        string password,
        bool isPersistent,
        bool lockoutOnFailure
    )
    {
        return await _signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent,
            lockoutOnFailure
        );
    }

    public async Task SignInAsync(ApplicationUser user, bool isPersistent)
    {
        await _signInManager.SignInAsync(user, isPersistent);
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string provider,
        string? redirectUrl
    )
    {
        return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
    }

    public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
    {
        return await _signInManager.GetExternalLoginInfoAsync();
    }

    public async Task<SignInResult> ExternalLoginSignInAsync(
        string loginProvider,
        string providerKey,
        bool isPersistent
    )
    {
        return await _signInManager.ExternalLoginSignInAsync(
            loginProvider,
            providerKey,
            isPersistent
        );
    }

    public async Task UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo externalLogin)
    {
        await _signInManager.UpdateExternalAuthenticationTokensAsync(externalLogin);
    }

    public async Task<ApplicationUser?> GetTwoFactorAuthenticationUserAsync()
    {
        return await _signInManager.GetTwoFactorAuthenticationUserAsync();
    }

    public async Task<SignInResult> TwoFactorSignInAsync(
        string provider,
        string code,
        bool isPersistent,
        bool rememberClient
    )
    {
        return await _signInManager.TwoFactorSignInAsync(
            provider,
            code,
            isPersistent,
            rememberClient
        );
    }

    public async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(
        string code,
        bool isPersistent,
        bool rememberClient
    )
    {
        return await _signInManager.TwoFactorAuthenticatorSignInAsync(
            code,
            isPersistent,
            rememberClient
        );
    }

    public async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        return await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
    }

    #endregion

    #region For ManageController.cs

    public async Task<bool> IsTwoFactorClientRememberedAsync(ApplicationUser user)
    {
        return await _signInManager.IsTwoFactorClientRememberedAsync(user);
    }

    public async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
    {
        return await _signInManager.GetExternalAuthenticationSchemesAsync();
    }

    public AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string provider,
        string? redirectUrl,
        string? userId = null
    )
    {
        return _signInManager.ConfigureExternalAuthenticationProperties(
            provider,
            redirectUrl,
            userId
        );
    }

    public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
    {
        return await _signInManager.GetExternalLoginInfoAsync(expectedXsrf);
    }

    #endregion
}
