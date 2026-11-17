// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Manage;

namespace CleanArchitectureTemplate.Web.Controllers.Identity;

[Authorize]
public class ManageController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender emailSender,
    ISmsSender smsSender,
    ILoggerFactory loggerFactory
) : Controller
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ISmsSender _smsSender = smsSender;
    private readonly ILogger _logger = loggerFactory.CreateLogger<ManageController>();

    #region Listing

    //
    // GET: /Manage/Index
    [HttpGet]
    [Authorize(Roles = RoleNames.User)]
    public async Task<IActionResult> Index(ManageMessageId? message = null)
    {
        ViewData["StatusMessage"] =
            message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.SetTwoFactorSuccess
                ? "Your two-factor authentication provider has been set."
            : message == ManageMessageId.Error ? "An error has occurred."
            : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
            : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
            : "";

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Challenge();
        }

        var viewModel = new IndexViewModel
        {
            HasPassword = await _userManager.HasPasswordAsync(user),
            PhoneNumber = await _userManager.GetPhoneNumberAsync(user) ?? string.Empty,
            TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
            Logins = await _userManager.GetLoginsAsync(user),
            BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
            AuthenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty,
        };
        return View(viewModel);
    }

    //
    // GET: /Manage/AddPhoneNumber
    public IActionResult AddPhoneNumber()
    {
        return View();
    }

    //
    // GET: /Manage/VerifyPhoneNumber
    [HttpGet]
    public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Challenge();

        await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
        // Send an SMS to verify the phone number
        return phoneNumber == null
            ? View("Error")
            : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
    }

    //
    // GET: /Manage/ChangePassword
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    //
    // GET: /Manage/SetPassword
    [HttpGet]
    public IActionResult SetPassword()
    {
        return View();
    }

    //GET: /Manage/ManageLogins
    [HttpGet]
    public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
    {
        ViewData["StatusMessage"] =
            message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
            : message == ManageMessageId.Error ? "An error has occurred."
            : "";
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return View("Error");
        }
        var userLogins = await _userManager.GetLoginsAsync(user);
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var otherLogins = schemes
            .Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();
        ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
        return View(
            new ManageLoginsViewModel { CurrentLogins = userLogins, OtherLogins = otherLogins }
        );
    }

    //
    // GET: /Manage/LinkLoginCallback
    [HttpGet]
    public async Task<ActionResult> LinkLoginCallback()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return View("Error");
        }
        var info = await _signInManager.GetExternalLoginInfoAsync(
            await _userManager.GetUserIdAsync(user)
        );
        if (info == null)
        {
            return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
        }
        var result = await _userManager.AddLoginAsync(user, info);
        var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
        return RedirectToAction(nameof(ManageLogins), new { Message = message });
    }

    #endregion

    #region Edit

    // No methods

    #endregion

    #region Save

    //
    // POST: /Manage/RemoveLogin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
    {
        ManageMessageId? message = ManageMessageId.Error;
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await _userManager.RemoveLoginAsync(
                user,
                account.LoginProvider,
                account.ProviderKey
            );
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                message = ManageMessageId.RemoveLoginSuccess;
            }
        }
        return RedirectToAction(nameof(ManageLogins), new { Message = message });
    }

    //
    // POST: /Manage/AddPhoneNumber
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);
        // Generate the token and send it
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Challenge();

        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(
            user,
            viewModel.PhoneNumber
        );
        await _smsSender.SendSmsAsync(viewModel.PhoneNumber, "Your security code is: " + code);
        return RedirectToAction(nameof(VerifyPhoneNumber), new { viewModel.PhoneNumber });
    }

    //
    // POST: /Manage/ResetAuthenticatorKey
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetAuthenticatorKey()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation(1, "User reset authenticator key.");
        }
        return RedirectToAction(nameof(Index), "Manage");
    }

    //
    // POST: /Manage/GenerateRecoveryCode
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateRecoveryCode()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5) ?? [];
            _logger.LogInformation(1, "User generated new recovery code.");
            return View(
                "DisplayRecoveryCodes",
                new DisplayRecoveryCodesViewModel { Codes = codes }
            );
        }
        return View("Error");
    }

    //
    // POST: /Manage/EnableTwoFactorAuthentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableTwoFactorAuthentication()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(1, "User enabled two-factor authentication.");
        }
        return RedirectToAction(nameof(Index), "Manage");
    }

    //
    // POST: /Manage/DisableTwoFactorAuthentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(2, "User disabled two-factor authentication.");
        }
        return RedirectToAction(nameof(Index), "Manage");
    }

    //
    // POST: /Manage/VerifyPhoneNumber
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await _userManager.ChangePhoneNumberAsync(
                user,
                viewModel.PhoneNumber,
                viewModel.Code
            );
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(
                    nameof(Index),
                    new { Message = ManageMessageId.AddPhoneSuccess }
                );
            }
        }
        // If we got this far, something failed, redisplay the form
        ModelState.AddModelError(string.Empty, "Failed to verify phone number");
        return View(viewModel);
    }

    //
    // GET: /Manage/RemovePhoneNumber
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoneNumber()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await _userManager.SetPhoneNumberAsync(user, null);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(
                    nameof(Index),
                    new { Message = ManageMessageId.RemovePhoneSuccess }
                );
            }
        }
        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
    }

    //
    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await _userManager.ChangePasswordAsync(
                user,
                viewModel.OldPassword,
                viewModel.NewPassword
            );
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(3, "User changed their password successfully.");
                return RedirectToAction(
                    nameof(Index),
                    new { Message = ManageMessageId.ChangePasswordSuccess }
                );
            }
            AddErrors(result);
            return View(viewModel);
        }
        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
    }

    //
    // POST: /Manage/SetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await _userManager.AddPasswordAsync(user, viewModel.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(
                    nameof(Index),
                    new { Message = ManageMessageId.SetPasswordSuccess }
                );
            }
            AddErrors(result);
            return View(viewModel);
        }
        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
    }

    //
    // POST: /Manage/LinkLogin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LinkLogin(string provider)
    {
        // Request a redirect to the external login provider to link a login for the current user
        var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            provider,
            redirectUrl,
            _userManager.GetUserId(User)
        );
        return Challenge(properties, provider);
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
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public enum ManageMessageId
    {
        AddPhoneSuccess,
        AddLoginSuccess,
        ChangePasswordSuccess,
        SetTwoFactorSuccess,
        SetPasswordSuccess,
        RemoveLoginSuccess,
        RemovePhoneSuccess,
        Error,
    }

    private Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return _userManager.GetUserAsync(HttpContext.User);
    }

    #endregion
}
