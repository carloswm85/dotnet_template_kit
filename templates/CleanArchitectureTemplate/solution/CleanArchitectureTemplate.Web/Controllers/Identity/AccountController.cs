// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Account;
using CleanArchitectureTemplate.Web.Services.Interfaces;

namespace CleanArchitectureTemplate.Web.Controllers.Identity;

[Authorize]
public class AccountController(
    IUserManagerService userManagerService,
    ISignInManagerService signInManagerService,
    IEmailSender emailSender,
    ISmsSender smsSender,
    ILoggerFactory loggerFactory,
    IRoleManagerService roleManagerService,
    IImageStorageService imageStorage
) : Controller
{
    private readonly IUserManagerService _userManagerService = userManagerService;
    private readonly ISignInManagerService _signInManagerService = signInManagerService;
    private readonly IRoleManagerService _roleManagerService = roleManagerService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ISmsSender _smsSender = smsSender;
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly IImageStorageService _imageStorage = imageStorage;

    #region Listing/Index

    //
    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, string? message = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Message"] = message;
        return View();
    }

    //
    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    //
    // GET: /Account/ExternalLoginCallback
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(
        string? returnUrl = null,
        string? remoteError = null
    )
    {
        if (remoteError != null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The external provider returned an error. Please try again."
            );
            return View(nameof(Login));
        }

        var info = await _signInManagerService.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        // Sign in the vm with this external login provider if the vm already has a login.
        var result = await _signInManagerService.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false
        );
        if (result.Succeeded)
        {
            // Update any authentication tokens if login succeeded
            await _signInManagerService.UpdateExternalAuthenticationTokensAsync(info);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    5,
                    "User logged in with {Name} provider.",
                    info.LoginProvider
                );
            }

            return RedirectToLocal(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
        }

        if (result.IsLockedOut)
        {
            return View("Lockout");
        }

        // If the vm does not have an account, then ask the vm to create an account.
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        return string.IsNullOrWhiteSpace(email)
            ? View("Error")
            : View(
            "ExternalLoginConfirmation",
            new ExternalLoginConfirmationViewModel { Email = email }
        );
    }

    // GET: /Account/ConfirmEmail
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }
        var user = await _userManagerService.FindByIdAsync(userId);
        if (user == null)
        {
            return View("Error");
        }
        var result = await _userManagerService.ConfirmEmailAsync(user, code);
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    //
    // GET: /Account/ForgotPassword
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    //
    // GET: /Account/ForgotPasswordConfirmation
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    //
    // GET: /Account/ResetPassword
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? code = null)
    {
        return code == null ? View("Error") : View();
    }

    //
    // GET: /Account/ResetPasswordConfirmation
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    // GET: /Account/UserProfile
    [HttpGet("account/profile")]
    public async Task<IActionResult> UserProfileDetails()
    {
        // Implementation for vm profile view
        var currentUserId = _userManagerService.GetUserId(HttpContext.User);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        ApplicationUser? user = await _userManagerService.FindByIdAsync(currentUserId);

        if (user == null)
        {
            return NotFound();
        }

        // TODO add mapping
        var vm = new UserProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ImagePath = user.ImagePath,
            RegistrationDate = user.RegistrationDate
        };

        return View("UserProfileDetails", vm);
    }

    //
    // GET: /Account/SendCode
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> SendCode(string? returnUrl = null, bool rememberMe = false)
    {
        var user = await _signInManagerService.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return View("Error");
        }
        var userFactors = await _userManagerService.GetValidTwoFactorProvidersAsync(user);
        var factorOptions = userFactors
            .Select(purpose => new SelectListItem { Text = purpose, Value = purpose })
            .ToList();
        return View(
            new SendCodeViewModel
            {
                Providers = factorOptions,
                ReturnUrl = returnUrl,
                RememberMe = rememberMe,
            }
        );
    }

    //
    // GET: /Account/VerifyCode
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCode(
        string provider,
        bool rememberMe,
        string? returnUrl = null
    )
    {
        // Require that the vm has already logged in via username/password or external login
        var user = await _signInManagerService.GetTwoFactorAuthenticationUserAsync();
        return user == null
            ? View("Error")
            : View(
            new VerifyCodeViewModel
            {
                Provider = provider,
                ReturnUrl = returnUrl,
                RememberMe = rememberMe,
            }
        );
    }

    //
    // GET: /Account/VerifyAuthenticatorCode
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyAuthenticatorCode(
        bool rememberMe,
        string? returnUrl = null
    )
    {
        // Require that the vm has already logged in via username/password or external login
        var user = await _signInManagerService.GetTwoFactorAuthenticationUserAsync();
        return user == null
            ? View("Error")
            : View(
            new VerifyAuthenticatorCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe }
        );
    }

    //
    // GET: /Account/UseRecoveryCode
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> UseRecoveryCode(string? returnUrl = null)
    {
        // Require that the vm has already logged in via username/password or external login
        var user = await _signInManagerService.GetTwoFactorAuthenticationUserAsync();
        return user == null ? View("Error") : View(new UseRecoveryCodeViewModel { ReturnUrl = returnUrl });
    }

    #endregion

    #region Edit

    // GET: /cuenta/editar
    [HttpGet("account/edit")]
    public async Task<IActionResult> UserProfileEdit()
    {
        var currentUserId = _userManagerService.GetUserId(HttpContext.User);
        if (string.IsNullOrWhiteSpace(currentUserId))
            return Unauthorized();

        var user = await _userManagerService.FindByIdAsync(currentUserId);
        if (user == null)
            return NotFound();

        var vm = new UserProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ImagePath = user.ImagePath,
            RegistrationDate = user.RegistrationDate,
        };

        return View("UserProfileEdit", vm);
    }

    // POST: /cuenta/editar/{id}
    [HttpPost("account/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserProfileEdit(string id, UserProfileViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        if (string.IsNullOrWhiteSpace(id) || id != vm.Id)
            return BadRequest();

        var currentUserId = _userManagerService.GetUserId(HttpContext.User);
        if (string.IsNullOrWhiteSpace(currentUserId))
            return Unauthorized();

        if (!string.Equals(currentUserId, id, StringComparison.Ordinal))
            return Forbid();

        try
        {
            var user = await _userManagerService.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManagerService.SetEmailAsync(user, vm.Email!);
                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                        ModelState.AddModelError(nameof(vm.Email), error.Description);

                    return View(vm);
                }
            }

            if (vm.Image != null)
                vm.ImagePath = await _imageStorage.SaveAsync(vm.Image, "users", HttpContext.RequestAborted);

            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.PhoneNumber = vm.PhoneNumber;
            user.City = vm.City;
            user.ImagePath = vm.ImagePath;

            var updateResult = await _userManagerService.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );

                return View("UserProfileEdit", vm);
            }

            return RedirectToAction(nameof(UserProfileDetails));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var user = await _userManagerService.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            _logger.LogWarning(ex, "Concurrency conflict while updating user ID {UserId}.", id);
            ModelState.AddModelError(
                string.Empty,
                "The profile was changed by another process. Reload the page and try again."
            );
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating user profile {UserId}.", id);
            ModelState.AddModelError(
                string.Empty,
                "The changes could not be saved. Try again and, if the problem persists, contact the system administrator."
            );
            return View(vm);
        }
    }

    #endregion

    #region Save

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = await _userManagerService.FindByEmailAsync(vm.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(vm);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been deactivated. Contact support."
                );
                return View(vm);
            }

            var result = await _signInManagerService.PasswordSignInAsync(
                user,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                _logger.LogInformation(1, "User logged in.");
                return RedirectToAction("Index", "Home");
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(
                    nameof(SendCode),
                    new { ReturnUrl = returnUrl, vm.RememberMe }
                );
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(2, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(vm);
            }
        }

        // If we got this far, something failed, redisplay form
        return View(vm);
    }

    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                UserName = vm.Email,
                Email = vm.Email,
            };

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError(nameof(vm.Password), "Password is required.");
                return View(vm);
            }

            var result = await _userManagerService.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                // TODO implement email confirmation
                //var code = await _userManagerService.GenerateEmailConfirmationTokenAsync(vm);
                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { currentUserId = vm.Id, code = code }, protocol: HttpContext.Request.Scheme);
                //await _emailSender.SendEmailAsync(vm.Email, "Confirm your account",
                //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                _logger.LogInformation(3, "User created a new account with password.");

                return RedirectToLocal(returnUrl, "The user has been created.");
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form
        return View(vm);
    }

    //
    // POST: /Account/LogOut
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOut()
    {
        await _signInManagerService.SignOutAsync();
        _logger.LogInformation(4, "User logged out.");
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    //
    // POST: /Account/ExternalLogin
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action(
            "ExternalLoginCallback",
            "Account",
            new { ReturnUrl = returnUrl }
        );
        var properties = _signInManagerService.ConfigureExternalAuthenticationProperties(
            provider,
            redirectUrl
        );
        return Challenge(properties, provider);
    }

    //
    // POST: /Account/ExternalLoginConfirmation
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(
        ExternalLoginConfirmationViewModel vm,
        string? returnUrl = null
    )
    {
        if (ModelState.IsValid)
        {
            // Get the information about the vm from the external login provider
            var info = await _signInManagerService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return View("ExternalLoginFailure");
            }
            var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
            var result = await _userManagerService.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManagerService.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManagerService.SignInAsync(user, isPersistent: false);
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation(
                            6,
                            "User created an account using {Name} provider.",
                            info.LoginProvider
                        );
                    }

                    // Update any authentication tokens as well
                    await _signInManagerService.UpdateExternalAuthenticationTokensAsync(info);

                    return RedirectToLocal(returnUrl);
                }
            }
            AddErrors(result);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(vm);
    }

    //
    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManagerService.FindByEmailAsync(vm.Email);
            if (user == null || !await _userManagerService.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the vm does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            //var code = await _userManagerService.GeneratePasswordResetTokenAsync(vm);
            //var callbackUrl = Url.Action("ResetPassword", "Account", new { currentUserId = vm.Id, code = code }, protocol: HttpContext.Request.Scheme);
            //await _emailSender.SendEmailAsync(vm.Email, "Reset Password",
            //   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
            //return View("ForgotPasswordConfirmation");
        }

        // If we got this far, something failed, redisplay form
        return View(vm);
    }

    //
    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }
        var user = await _userManagerService.FindByEmailAsync(vm.Email);
        if (user == null)
        {
            // Don't reveal that the vm does not exist
            return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
        }
        var result = await _userManagerService.ResetPasswordAsync(
            user,
            vm.Code,
            vm.Password
        );
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
        }
        AddErrors(result);
        return View();
    }

    //
    // POST: /Account/SendCode
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendCode(SendCodeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _signInManagerService.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return View("Error");
        }

        if (vm.SelectedProvider == "Authenticator")
        {
            return RedirectToAction(
                nameof(VerifyAuthenticatorCode),
                new { vm.ReturnUrl, vm.RememberMe }
            );
        }

        // Generate the token and send it
        var code = await _userManagerService.GenerateTwoFactorTokenAsync(
            user,
            vm.SelectedProvider!
        );
        if (string.IsNullOrWhiteSpace(code))
        {
            return View("Error");
        }

        var message = "Your security code is: " + code;
        if (vm.SelectedProvider == "Email")
        {
            var email = await _userManagerService.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
            {
                return View("Error");
            }

            await _emailSender.SendEmailAsync(email, "Security code", message);
        }
        else if (vm.SelectedProvider == "Phone")
        {
            var phoneNumber = await _userManagerService.GetPhoneNumberAsync(user);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return View("Error");
            }

            await _smsSender.SendSmsAsync(phoneNumber, message);
        }

        return RedirectToAction(
            nameof(VerifyCode),
            new
            {
                Provider = vm.SelectedProvider,
                vm.ReturnUrl,
                vm.RememberMe,
            }
        );
    }

    //
    // POST: /Account/VerifyCode
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(VerifyCodeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        // The following code protects for brute force attacks against the two factor codes.
        // If a vm enters incorrect codes for a specified amount of time then the vm account
        // will be locked out for a specified amount of time.
        var result = await _signInManagerService.TwoFactorSignInAsync(
            vm.Provider,
            vm.Code,
            vm.RememberMe,
            vm.RememberBrowser
        );
        if (result.Succeeded)
        {
            return RedirectToLocal(vm.ReturnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning(7, "User account locked out.");
            return View("Lockout");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid code.");
            return View(vm);
        }
    }

    //
    // POST: /Account/VerifyAuthenticatorCode
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyAuthenticatorCode(
        VerifyAuthenticatorCodeViewModel vm
    )
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        // The following code protects for brute force attacks against the two factor codes.
        // If a vm enters incorrect codes for a specified amount of time then the vm account
        // will be locked out for a specified amount of time.
        var result = await _signInManagerService.TwoFactorAuthenticatorSignInAsync(
            vm.Code,
            vm.RememberMe,
            vm.RememberBrowser
        );
        if (result.Succeeded)
        {
            return RedirectToLocal(vm.ReturnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning(7, "User account locked out.");
            return View("Lockout");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid code.");
            return View(vm);
        }
    }

    //
    // POST: /Account/UseRecoveryCode
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Code))
        {
            ModelState.AddModelError(nameof(vm.Code), "The recovery code is required.");
            return View(vm);
        }

        var result = await _signInManagerService.TwoFactorRecoveryCodeSignInAsync(vm.Code);
        if (result.Succeeded)
        {
            return RedirectToLocal(vm.ReturnUrl);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid code.");
            return View(vm);
        }
    }

    #endregion

    #region Delete

    // No methods

    #endregion

    #region Private methods

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description
            );
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var user = await _userManagerService.GetUserAsync(HttpContext.User);
        return user ?? throw new InvalidOperationException("No authenticated user found.");
    }

    private IActionResult RedirectToLocal(string? returnUrl, string? message = null)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction(nameof(Login), new { message });
    }

    // ~/cuenta/editar

    #endregion
}
