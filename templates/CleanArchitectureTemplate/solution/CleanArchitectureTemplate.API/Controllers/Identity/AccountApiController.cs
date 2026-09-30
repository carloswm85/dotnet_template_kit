using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using CleanArchitectureTemplate.API.Contracts;
using CleanArchitectureTemplate.API.Contracts.Identity;
using CleanArchitectureTemplate.API.Contracts.Identity.IdentityContractDtos;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Model.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CleanArchitectureTemplate.API.Controllers.Identity;

/// <summary>
/// Authentication and account management endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Tags("Custom Identity API Authentication (with JWT)")]
public class AccountApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly ILogger _logger;
    private readonly ITokenService _tokenService;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AccountApiController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        ISmsSender smsSender,
        ILoggerFactory loggerFactory,
        ITokenService tokenService,
        RoleManager<ApplicationRole> roleManager
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = loggerFactory.CreateLogger<AccountApiController>();
        _tokenService = tokenService;
        _roleManager = roleManager;
    }

    //
    // POST: api/AccountApi/Register
    //
    /// <summary>
    /// ✅ Registers a new user account.
    /// </summary>
    /// <remarks>
    /// Creates a new user using the provided email and password.
    ///
    /// Process:
    /// 1. Creates the user in the Identity store.
    /// 2. Ensures the default role ("User") exists.
    /// 3. Assigns the default role to the new user.
    /// 4. Generates an email confirmation token.
    ///
    /// Email confirmation sending is prepared but requires an email service implementation.
    /// </remarks>
    /// <param name="request">
    /// Registration request containing email and password.
    /// </param>
    /// <response code="200">
    /// User registered successfully.
    /// </response>
    /// <response code="400">
    /// Validation errors, password policy failure, or role creation failure.
    /// </response>
    [AllowAnonymous]
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserRegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            City = request.City,
            UserName = request.Email,
            Email = request.Email,
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "User registration failed for {Email}, with errors: {Errors}",
                request.Email,
                errors
            );
            return BadRequest(new GenericMessageResponse { Message = "User registration failed" });
        }

        const string defaultRole = "User";

        // Ensure role exists
        if (!await _roleManager.RoleExistsAsync(defaultRole))
        {
            _logger.LogError("Default role {RoleName} is required", defaultRole);
            return BadRequest(new GenericMessageResponse { Message = "User registration failed" });
        }

        // Assign default role
        var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError(
                "Failed to assign role {RoleName} to user {Email}, with errors: {Errors}",
                defaultRole,
                request.Email,
                errors
            );
            return BadRequest(new GenericMessageResponse { Message = "Failed to assign role" });
        }

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "AccountApi",
            new { userId = user.Id, token = encodedToken },
            Request.Scheme
        );

        await _emailSender.SendEmailAsync(
            request.Email,
            "Confirm your email",
            $"Please confirm your account by clicking this link: {confirmationLink}"
        );

        return Ok(new UserRegisterResponse() { Message = "User registered successfully" });
    }

    //
    // POST: api/AccountApi/login
    //
    /// <summary>
    /// ⚠️ Partially tested. ✅ Authenticates a user using email and password credentials and issues JWT tokens.
    /// </summary>
    /// <remarks>
    /// Authentication flow:
    ///
    /// 1. Validates email and password.
    /// 2. Verifies that the email is confirmed.
    /// 3. Checks the password using ASP.NET Core Identity.
    /// 4. If two-factor authentication (2FA) is enabled:
    ///    - Returns <c>RequiresTwoFactor = true</c> when no 2FA code is provided.
    ///    - Validates the provided authenticator or recovery code.
    /// 5. Upon successful validation, generates:
    ///    - A signed JWT access token
    ///    - A refresh token stored securely server-side
    ///
    /// Security considerations:
    /// - Password is always validated before 2FA verification.
    /// - The endpoint does not reveal whether a user exists.
    /// - Lockout policies are enforced when configured.
    /// - Designed for stateless JWT authentication (no cookie-based flows).
    /// </remarks>
    /// <param name="request">
    /// Login request containing:
    /// - Email (required)
    /// - Password (required)
    /// - Optional TwoFactorCode
    /// - Optional TwoFactorRecoveryCode
    /// </param>
    /// <returns>
    /// Returns a <see cref="UserLoginResponse"/> containing:
    /// - JWT access token
    /// - Refresh token
    /// - Authenticated user information
    /// - Two-factor requirement flag (when applicable)
    /// </returns>
    /// <response code="200">
    /// Login successful OR two-factor authentication required.
    /// </response>
    /// <response code="400">
    /// Request validation failed (missing email or password).
    /// </response>
    /// <response code="401">
    /// Invalid credentials, invalid two-factor code, or email not confirmed.
    /// </response>
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogIn([FromBody] UserLoginRequest request)
    {
        if (
            request == null
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
        )
        {
            return BadRequest(
                new GenericMessageResponse { Message = "Email and password are required" }
            );
        }

        var user = await _userManager.FindByEmailAsync(request.Email);

        // Do not reveal whether user exists
        if (user == null)
        {
            return Unauthorized(new GenericMessageResponse { Message = "Invalid credentials" });
        }

        if (!user.EmailConfirmed)
        {
            return Unauthorized(new GenericMessageResponse { Message = "Email not confirmed" });
        }

        // Always validate password first
        var passwordResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true
        );

        if (!passwordResult.Succeeded)
        {
            return Unauthorized(new GenericMessageResponse { Message = "Invalid credentials" });
        }

        var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

        if (twoFactorEnabled)
        {
            // If no 2FA code provided → inform client
            if (
                string.IsNullOrWhiteSpace(request.TwoFactorCode)
                && string.IsNullOrWhiteSpace(request.TwoFactorRecoveryCode)
            )
            {
                return Ok(
                    new UserLoginResponse
                    {
                        RequiresTwoFactor = true,
                        Message = "Two-factor authentication required",
                    }
                );
            }

            bool twoFactorValid = false;

            if (!string.IsNullOrWhiteSpace(request.TwoFactorCode))
            {
                twoFactorValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    request.TwoFactorCode
                );
            }
            else if (!string.IsNullOrWhiteSpace(request.TwoFactorRecoveryCode))
            {
                var recoveryResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(
                    user,
                    request.TwoFactorRecoveryCode
                );
                twoFactorValid = recoveryResult.Succeeded;
            }

            if (!twoFactorValid)
            {
                return Unauthorized(
                    new GenericMessageResponse { Message = "Invalid two-factor code" }
                );
            }
        }

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken);

        return Ok(
            new UserLoginResponse
            {
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RequiresTwoFactor = false,
                User = new UserInformationResponse
                {
                    Id = user.Id!,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Roles = roles,
                },
            }
        );
    }

    //
    // POST: api/AccountApi/logout - Device-specific logout, only this device/session (requires refresh token validation)
    //
    /// <summary>
    /// ✅ Logs out the currently authenticated user from the current device/session.
    /// </summary>
    /// <remarks>
    /// This endpoint performs a secure device-scoped logout by validating the provided refresh token
    /// before revoking any tokens.
    ///
    /// Execution Flow:
    /// - A valid refresh token MUST be supplied in the request body.
    /// - The refresh token is validated against the current authenticated user.
    /// - If (and only if) the refresh token is valid:
    ///     - The provided access token (Bearer token) is revoked.
    ///     - The refresh token is revoked.
    ///
    /// If the refresh token is missing, invalid, expired, revoked, or does not belong to
    /// the authenticated user, no tokens are revoked and the request returns 401 Unauthorized.
    ///
    /// Authentication:
    /// - Requires a valid Bearer access token in the Authorization header.
    ///
    /// Scope:
    /// - Only the current device/session (token pair) is invalidated.
    /// - Other active sessions remain unaffected.
    /// </remarks>
    /// <param name="request">
    /// Contains the refresh token associated with the current device/session.
    /// This value is required.
    /// </param>
    /// <response code="200">
    /// Logout completed successfully. The refresh token was validated and both tokens were revoked.
    /// </response>
    /// <response code="401">
    /// The request is unauthorized due to:
    /// - Missing or invalid Bearer token
    /// - Missing refresh token
    /// - Invalid or mismatched refresh token
    /// - Invalid authenticated user context
    /// </response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogOut([FromBody] UserLogoutRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return Unauthorized(
                new GenericMessageResponse { Message = "Refresh token is required" }
            );

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new GenericMessageResponse { Message = "Invalid user context" });

        // Validate refresh token FIRST
        var isRefreshTokenValid = await _tokenService.ValidateRefreshTokenAsync(
            userId,
            request.RefreshToken
        );

        if (!isRefreshTokenValid)
            return Unauthorized(new GenericMessageResponse { Message = "Invalid refresh token" });

        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized(
                new GenericMessageResponse { Message = "Invalid or missing Bearer token" }
            );

        var accessToken = authHeader["Bearer ".Length..].Trim();

        // Only revoke if refresh token is valid
        await _tokenService.RevokeTokenAsync(accessToken);
        await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

        return Ok(new GenericMessageResponse { Message = "Logged out successfully" });
    }

    //
    // POST: api/AccountApi/logout/all - Logout all devices
    //
    /// <summary>
    /// ✅ Logs out the currently authenticated user from all devices.
    /// </summary>
    /// <remarks>
    /// Existing access tokens remain valid until expiration unless separately revoked.
    /// </remarks>
    /// <response code="200">All user sessions were successfully invalidated.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost("logout/all")]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogOutAllDevices()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new GenericMessageResponse { Message = "User not authenticated" });

        // Invalidate all tokens for this user
        await _tokenService.InvalidateAllRefreshTokensAsync(userId);

        _logger.LogInformation("User {UserId} logged out from all devices", userId);
        return Ok(
            new GenericMessageResponse { Message = "Logged out from all devices successfully" }
        );
    }

    //
    // POST: api/AccountApi/refresh
    //
    /// <summary>
    /// ✅ Refreshes an expired JWT access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// This endpoint validates the provided refresh token and, if valid:
    ///
    /// - Generates a new signed JWT access token.
    /// - Optionally rotates and returns a new refresh token.
    ///
    /// Possible outcomes:
    ///
    /// - 200 → Token successfully refreshed.
    /// - 400 → Refresh token missing or malformed.
    /// - 401 → Invalid, expired, or revoked refresh token.
    ///
    /// ⚠️ This endpoint does not require authentication because
    /// the refresh token itself is the authentication mechanism.
    /// </remarks>
    /// <param name="request">
    /// Request containing the refresh token string.
    /// </param>
    /// <response code="200">
    /// Returns a new JWT access token and (optionally) a new refresh token.
    /// </response>
    /// <response code="400">
    /// The refresh token was not provided or is invalid in format.
    /// </response>
    /// <response code="401">
    /// The refresh token is invalid, expired, revoked, or the user no longer exists.
    /// </response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] UserRefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken) || string.IsNullOrEmpty(request.AccessToken))
        {
            return BadRequest(
                new UserLoginResponse
                {
                    AccessToken = string.Empty,
                    Message = "Access token and refresh token are required",
                }
            );
        }

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken)!;
        }
        catch
        {
            return Unauthorized(
                new UserLoginResponse
                {
                    AccessToken = string.Empty,
                    Message = "Invalid access token",
                }
            );
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value; // Unique identifier for a specific json token instance.
        var iat = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value; // "Issued At" - Unix timestamp indicating when the token was created.

        if (userId == null || jti == null)
            return Unauthorized(
                new UserLoginResponse
                {
                    AccessToken = string.Empty,
                    Message = "Invalid token claims",
                }
            );

        if (await _tokenService.IsTokenRevokedAsync(jti))
            return Unauthorized(
                new UserLoginResponse
                {
                    AccessToken = string.Empty,
                    Message = "Token has been revoked",
                }
            );

        if (
            !string.IsNullOrEmpty(iat)
            && long.TryParse(iat, out var issuedAt)
            && await _tokenService.IsUserRevokedAsync(userId, issuedAt)
        )
            return Unauthorized(
                new UserLoginResponse
                {
                    AccessToken = string.Empty,
                    Message = "User tokens have been invalidated",
                }
            );

        if (!await _tokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken))
            return Unauthorized(
                new UserLoginResponse
                {
                    AccessToken = string.Empty,
                    Message = "Invalid refresh token",
                }
            );

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized(
                new UserLoginResponse { AccessToken = string.Empty, Message = "User not found" }
            );

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        // Rotate refresh token
        await _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.StoreRefreshTokenAsync(userId, newRefreshToken);

        return Ok(
            new UserLoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = new UserInformationResponse
                {
                    Id = user.Id!,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Roles = roles,
                },
                Message = "Token refreshed successfully",
            }
        );
    }

    //
    // GET: api/AccountApi/confirmEmail
    //
    /// <summary>
    /// ✅ Confirms a user's email address.
    /// </summary>
    /// <remarks>
    /// Validates the email confirmation token generated during registration.
    ///
    /// This endpoint is typically accessed through a confirmation link
    /// sent via email containing:
    /// - userId
    /// - token
    ///
    /// If validation succeeds, the user's EmailConfirmed flag is set to true.
    /// </remarks>
    /// <param name="userId">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="token">
    /// The email confirmation token generated by the Identity system.
    /// </param>
    /// <response code="200">
    /// Email confirmed successfully.
    /// </response>
    /// <response code="400">
    /// Missing parameters or invalid/expired token.
    /// </response>
    /// <response code="404">
    /// User not found.
    /// </response>
    [HttpGet("confirmEmail")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string token
    )
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Email confirmation attempted with missing parameters");
            return BadRequest(
                new GenericMessageResponse { Message = "User ID and token are required" }
            );
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning(
                "Email confirmation attempted for non-existent user {UserId}",
                userId
            );
            return NotFound(new GenericMessageResponse { Message = "User not found" });
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "Email confirmation attempted with malformed token for {Email}",
                user.Email
            );
            return BadRequest(new GenericMessageResponse { Message = "Invalid token" });
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError(
                "Email confirmation failed for user {UserId}, with errors: {Errors}",
                userId,
                errors
            );
            return BadRequest(new GenericMessageResponse { Message = "Email confirmation failed" });
        }

        _logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
        return Ok(new GenericMessageResponse { Message = "Email confirmed successfully" });
    }

    //
    // POST: api/AccountApi/resendConfirmationEmail
    //
    /// <summary>
    /// ✅ Resends the email confirmation link to a user. Already confirmed email addresses don't receive new email.
    /// </summary>
    /// <remarks>
    /// This endpoint triggers the generation of a new email confirmation token
    /// for the specified email address and sends a confirmation link if the user exists
    /// and their email is not already confirmed.
    ///
    /// For security reasons, the response message is always generic and does not
    /// reveal whether the user exists or whether the email is already confirmed.
    ///
    /// This endpoint allows anonymous access.
    /// </remarks>
    /// <param name="request">
    /// The request containing the email address for which the confirmation email
    /// should be resent.
    /// </param>
    /// <returns>
    /// Returns 200 OK with a generic message if the request is processed.
    /// Returns 400 Bad Request if the email is missing or invalid.
    /// </returns>
    /// <response code="200">
    /// Request processed successfully. A confirmation email may have been sent.
    /// </response>
    /// <response code="400">
    /// The request is invalid (e.g., missing email).
    /// </response>
    [HttpPost("resendConfirmationEmail")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationEmail(
        [FromBody] UserResendConfirmationEmailRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Resend confirmation email attempted with missing email");
            return BadRequest(new GenericMessageResponse { Message = "Email is required" });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning(
                "Resend confirmation email attempted for non-existent user {Email}",
                request.Email
            );
            return Ok(
                new GenericMessageResponse
                {
                    Message = "If the email exists, a confirmation link has been sent",
                }
            );
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogInformation(
                "Resend confirmation email attempted for already confirmed email {Email}",
                request.Email
            );
            return Ok(
                new GenericMessageResponse
                {
                    Message = "If the email exists, a confirmation link has been sent",
                }
            );
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "AccountApi",
            new { userId = user.Id, token = encodedToken },
            Request.Scheme
        );

        await _emailSender.SendEmailAsync(
            request.Email,
            "Confirm your email",
            $"Please confirm your account by clicking this link: {confirmationLink}"
        );

        _logger.LogInformation("Confirmation email resent to {Email}", request.Email);
        return Ok(
            new GenericMessageResponse
            {
                Message = "If the email exists, a confirmation link has been sent",
            }
        );
    }

    //
    // POST: api/AccountApi/forgotPassword
    //
    /// <summary>
    /// ✅ Initiates the forgot password flow for a user account.
    /// </summary>
    /// <remarks>
    /// For security purposes, this endpoint always returns the same response regardless of
    /// whether the email exists or the account is confirmed, preventing user enumeration attacks.
    /// <para>
    /// The reset token is Base64URL-encoded before being included in the reset link.
    /// </para>
    /// </remarks>
    /// <param name="request">The request body containing the email address of the account to reset.</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description><c>200 OK</c> — Always returned when the email field is valid, regardless of account existence.</description></item>
    ///   <item><description><c>400 Bad Request</c> — Returned when the email field is missing or empty.</description></item>
    /// </list>
    /// </returns>
    /// <response code="200">Password reset email sent (or silently skipped if account not found/unconfirmed).</response>
    /// <response code="400">The email field was not provided in the request body.</response>
    [HttpPost("forgotPassword")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] UserForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Forgot password attempted with missing email");
            return BadRequest(new GenericMessageResponse { Message = "Email is required" });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.EmailConfirmed)
        {
            // Don't reveal that the user doesn't exist or is not confirmed
            _logger.LogWarning("Forgot password attempted for non-existent or unconfirmed account");
            return Ok(
                new GenericMessageResponse
                {
                    Message = "If the email exists, a password reset link has been sent",
                }
            );
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = Url.Action(
            "ResetPassword",
            "AccountApi",
            new { token = encodedToken },
            Request.Scheme
        );

        await _emailSender.SendEmailAsync(
            request.Email,
            "Reset Password",
            $"Please reset your password by clicking this link: {resetLink}"
        );

        _logger.LogInformation("Password reset email sent to {Email}", request.Email);
        return Ok(
            new GenericMessageResponse
            {
                Message = "If the email exists, a password reset link has been sent",
            }
        );
    }

    //
    // POST: api/AccountApi/resetPassword
    //
    /// <summary>
    /// ✅ Resets the password of a user using a valid password reset token.
    /// </summary>
    /// <remarks>
    /// This endpoint:
    /// - Validates required parameters (Email, Token, NewPassword).
    /// - Prevents user enumeration by returning a generic error message.
    /// - Protects against timing attacks by introducing a small delay when the user is not found.
    /// - Decodes the Base64Url-encoded token before calling the identity reset method.
    /// - Returns a generic response message for both success and failure cases.
    ///
    /// Security considerations:
    /// - Does not reveal whether the email exists or is confirmed.
    /// - Logs detailed errors internally while exposing only generic messages externally.
    /// </remarks>
    /// <param name="request">
    /// Contains:
    /// - Email: User email address.
    /// - Token: Base64Url-encoded password reset token.
    /// - NewPassword: The new password to be set.
    /// </param>
    /// <returns>
    /// 200 OK:
    ///     Password reset completed successfully.
    /// 400 BadRequest:
    ///     Invalid or malformed request, invalid token, or failed reset attempt.
    /// </returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid password reset attempt or missing required parameters.</response>
    [HttpPost("resetPassword")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrEmpty(request.Token)
            || string.IsNullOrEmpty(request.NewPassword)
        )
        {
            _logger.LogWarning("Password reset attempted with missing parameters");
            return BadRequest(
                new GenericMessageResponse
                {
                    Message = "Email, token, and new password are required",
                }
            );
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.EmailConfirmed)
        {
            await Task.Delay(300); // simulate work against timing attacks
            // Don't reveal that the user doesn't exist
            _logger.LogWarning(
                "Password reset attempted for non-existent user {Email}",
                request.Email
            );
            return BadRequest(
                new GenericMessageResponse { Message = "Invalid password reset attempt" }
            );
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "Password reset attempted with malformed token for {Email}",
                request.Email
            );
            return BadRequest(
                new GenericMessageResponse { Message = "Invalid password reset attempt" }
            );
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Password reset failed for {Email}, with errors: {Errors}",
                request.Email,
                errors
            );
            return BadRequest(
                new GenericMessageResponse { Message = "Invalid password reset attempt" }
            );
        }

        _logger.LogInformation("Password reset successful for {Email}", request.Email);
        return Ok(new GenericMessageResponse { Message = "Password reset successfully" });
    }

    //
    // POST: api/AccountApi/manage/2fa
    //
    /// <summary>
    /// ⚠️ Manages two-factor authentication (2FA) settings for the authenticated user.
    /// </summary>
    /// <remarks>
    /// This endpoint allows an authenticated user to configure or manage
    /// their two-factor authentication settings.
    ///
    /// Supported actions:
    ///
    /// 1. <b>GenerateSetup</b>
    ///    - Generates or resets the authenticator key.
    ///    - Returns the shared key and QR code URI for authenticator app setup.
    ///
    /// 2. <b>VerifyAndEnable</b>
    ///    - Validates the provided authenticator verification code.
    ///    - Enables two-factor authentication upon successful validation.
    ///    - Optionally generates recovery codes for backup access.
    ///
    /// 3. <b>Disable</b>
    ///    - Disables two-factor authentication for the user.
    ///
    /// Security considerations:
    /// - Requires authenticated user context.
    /// - Uses stateless token verification compatible with JWT-based APIs.
    /// - Does not expose sensitive internal details.
    /// </remarks>
    /// <param name="request">
    /// Request containing:
    /// - The <c>TwoFactorAction</c> to perform.
    /// - A <c>VerificationCode</c> when required.
    /// </param>
    /// <returns>
    /// Returns either:
    /// - <see cref="ManageTwoFactorResponse"/> when setup or enable operations succeed.
    /// - <see cref="GenericMessageResponse"/> for simple confirmation messages.
    /// </returns>
    /// <response code="200">
    /// Action completed successfully.
    /// </response>
    /// <response code="400">
    /// Invalid request, invalid verification code, or operation failure.
    /// </response>
    /// <response code="401">
    /// User is not authenticated.
    /// </response>
    [HttpPost("manage/2fa")]
    [ProducesResponseType(typeof(ManageTwoFactorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ManageTwoFactor([FromBody] UserManageTwoFactorRequest request)
    {
        if (request == null)
        {
            return BadRequest(new GenericMessageResponse { Message = "Invalid request" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new GenericMessageResponse { Message = "Unauthorized" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized(new GenericMessageResponse { Message = "Unauthorized" });

        switch (request.TwoFactorAction)
        {
            case TwoFactorAction.GenerateSetup:
                {
                    var key = await _userManager.GetAuthenticatorKeyAsync(user);

                    if (string.IsNullOrEmpty(key))
                    {
                        await _userManager.ResetAuthenticatorKeyAsync(user);
                        key = await _userManager.GetAuthenticatorKeyAsync(user);
                    }

                    if (string.IsNullOrEmpty(key))
                    {
                        return StatusCode(
                            StatusCodes.Status500InternalServerError,
                            new GenericMessageResponse
                            {
                                Message = "Failed to generate authenticator key",
                            }
                        );
                    }

                    var email = await _userManager.GetEmailAsync(user);
                    var uri = GenerateQrCodeUri(email!, key);

                    return Ok(
                        new ManageTwoFactorResponse
                        {
                            SharedKey = FormatKey(key),
                            AuthenticatorUri = uri,
                            Message = "Scan QR code and verify to enable 2FA",
                        }
                    );
                }

            case TwoFactorAction.VerifyAndEnable:
                {
                    if (string.IsNullOrWhiteSpace(request.VerificationToken2fa))
                    {
                        return BadRequest(
                            new GenericMessageResponse { Message = "Verification code is required" }
                        );
                    }

                    var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                        user,
                        _userManager.Options.Tokens.AuthenticatorTokenProvider,
                        request.VerificationToken2fa
                    );

                    if (!isValid)
                    {
                        return BadRequest(
                            new GenericMessageResponse { Message = "Invalid verification code" }
                        );
                    }

                    var enableResult = await _userManager.SetTwoFactorEnabledAsync(user, true);

                    if (!enableResult.Succeeded)
                    {
                        return BadRequest(
                            new GenericMessageResponse
                            {
                                Message = "Failed to enable two-factor authentication",
                            }
                        );
                    }

                    // Optional: generate recovery codes immediately
                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(
                        user,
                        10
                    );

                    return Ok(
                        new ManageTwoFactorResponse
                        {
                            RecoveryCodes = recoveryCodes,
                            Message = "Two-factor authentication enabled successfully",
                        }
                    );
                }

            case TwoFactorAction.Disable:
                {
                    var disableResult = await _userManager.SetTwoFactorEnabledAsync(user, false);

                    if (!disableResult.Succeeded)
                    {
                        return BadRequest(
                            new GenericMessageResponse
                            {
                                Message = "Failed to disable two-factor authentication",
                            }
                        );
                    }

                    return Ok(
                        new GenericMessageResponse
                        {
                            Message = "Two-factor authentication disabled successfully",
                        }
                    );
                }

            default:
                return BadRequest(
                    new GenericMessageResponse { Message = "Invalid two-factor action" }
                );
        }
    }

    //
    // GET: api/AccountApi/manage/info
    //
    /// <summary>
    /// ✅ Retrieves the authenticated user's profile information.
    /// </summary>
    /// <remarks>
    /// This endpoint returns basic account details for the currently authenticated user,
    /// including email status, phone confirmation status, two-factor configuration,
    /// and assigned roles.
    ///
    /// The user is identified using the <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> claim
    /// from the current security principal.
    ///
    /// Possible responses:
    /// - 200 OK: User information successfully retrieved.
    /// - 401 Unauthorized: The request does not contain a valid authenticated user.
    /// - 404 Not Found: The authenticated user no longer exists in the system.
    /// </remarks>
    /// <returns>
    /// An <see cref="UserInformationResponse"/> containing the user's profile data.
    /// </returns>
    /// <response code="200">User information successfully retrieved.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("manage/info")]
    [ProducesResponseType(typeof(UserInformationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Get user info attempted without valid user ID");
            return Unauthorized(new GenericMessageResponse { Message = "User not authenticated" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Get user info attempted for non-existent user {UserId}", userId);
            return NotFound(new GenericMessageResponse { Message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User info retrieved for user {UserId}", userId);
        return Ok(
            new UserInformationResponse
            {
                Id = user.Id!,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = roles,
            }
        );
    }

    //
    // PATCH: api/AccountApi/manage/info
    //
    /// <summary>
    /// ✅ Updates the authenticated user's account information.
    /// </summary>
    /// <remarks>
    /// This endpoint allows the currently authenticated user to update selected
    /// account properties such as phone number and password.
    ///
    /// Password changes require the current password for verification.
    /// If validation or identity operations fail, a 400 response is returned.
    ///
    /// The user is identified using the <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> claim
    /// from the current security principal.
    ///
    /// Possible responses:
    /// - 200 OK: User information successfully updated.
    /// - 400 Bad Request: Validation failure or identity update error.
    /// - 401 Unauthorized: The request does not contain a valid authenticated user.
    /// - 404 Not Found: The authenticated user no longer exists in the system.
    /// </remarks>
    /// <param name="request">
    /// The request payload containing the updated user information,
    /// including optional phone number and password fields.
    /// </param>
    /// <returns>
    /// A <see cref="GenericMessageResponse"/> indicating the result of the operation.
    /// </returns>
    /// <response code="200">User information updated successfully.</response>
    /// <response code="400">Invalid input or update operation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User not found.</response>
    [HttpPatch("manage/info")]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(GenericMessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserInfo([FromBody] UserUpdateInfoRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Update user info attempted without valid user ID");
            return Unauthorized(new GenericMessageResponse { Message = "User not authenticated" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Update user info attempted for non-existent user {UserId}", userId);
            return NotFound(new GenericMessageResponse { Message = "User not found" });
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                var errors = string.Join(", ", setPhoneResult.Errors.Select(e => e.Description));
                _logger.LogError(
                    "Failed to update phone number for user {UserId}, with errors: {Errors}",
                    userId,
                    errors
                );
                return BadRequest(
                    new GenericMessageResponse { Message = "Failed to update phone number" }
                );
            }
        }

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                return BadRequest(
                    new GenericMessageResponse
                    {
                        Message = "Current password is required to set a new password",
                    }
                );
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!changePasswordResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    changePasswordResult.Errors.Select(e => e.Description)
                );
                _logger.LogError(
                    "Failed to change password for user {UserId}, with errors: {Errors}",
                    userId,
                    errors
                );
                return BadRequest(
                    new GenericMessageResponse { Message = "Failed to change password" }
                );
            }
        }

        _logger.LogInformation("User info updated for user {UserId}", userId);
        return Ok(new GenericMessageResponse { Message = "User information updated successfully" });
    }

    //=============================================================================================== Helper methods

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string AuthenticatorUriFormat =
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        return string.Format(
            AuthenticatorUriFormat,
            UrlEncoder.Default.Encode("CleanArchitectureTemplate"),
            UrlEncoder.Default.Encode(email),
            unformattedKey
        );
    }
}
