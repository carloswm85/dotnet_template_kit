using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureTemplate.Web.Services.Examples;

/// <summary>
/// EXAMPLE ONLY - not registered. Shows how to localize ASP.NET Core Identity
/// error messages (here: Spanish).
///
/// To use it, register it in StartUp.ConfigureServices:
/// <code>
/// services
///     .AddIdentity&lt;ApplicationUser, ApplicationRole&gt;()
///     .AddErrorDescriber&lt;SpanishIdentityErrorDescriber&gt;()
///     ...
/// </code>
/// For several languages, prefer an IdentityErrorDescriber that reads from
/// IStringLocalizer resources instead of hard-coded strings.
/// </summary>
public sealed class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Error(nameof(DefaultError), "Se produjo un error desconocido.");

    public override IdentityError ConcurrencyFailure() =>
        Error(nameof(ConcurrencyFailure), "Error de concurrencia: el objeto fue modificado por otro proceso.");

    public override IdentityError PasswordMismatch() =>
        Error(nameof(PasswordMismatch), "La contraseña es incorrecta.");

    public override IdentityError InvalidToken() =>
        Error(nameof(InvalidToken), "El token no es válido.");

    public override IdentityError LoginAlreadyAssociated() =>
        Error(nameof(LoginAlreadyAssociated), "Ya existe un usuario asociado con este inicio de sesión.");

    public override IdentityError InvalidUserName(string? userName) =>
        Error(nameof(InvalidUserName), $"El nombre de usuario '{userName}' no es válido.");

    public override IdentityError InvalidEmail(string? email) =>
        Error(nameof(InvalidEmail), $"La dirección de correo electrónico '{email}' no es válida.");

    public override IdentityError DuplicateUserName(string userName) =>
        Error(nameof(DuplicateUserName), $"El nombre de usuario '{userName}' ya está en uso.");

    public override IdentityError DuplicateEmail(string email) =>
        Error(nameof(DuplicateEmail), $"La dirección de correo electrónico '{email}' ya está en uso.");

    public override IdentityError InvalidRoleName(string? role) =>
        Error(nameof(InvalidRoleName), $"El nombre de rol '{role}' no es válido.");

    public override IdentityError DuplicateRoleName(string role) =>
        Error(nameof(DuplicateRoleName), $"El nombre de rol '{role}' ya está en uso.");

    public override IdentityError UserAlreadyHasPassword() =>
        Error(nameof(UserAlreadyHasPassword), "El usuario ya tiene una contraseña establecida.");

    public override IdentityError UserLockoutNotEnabled() =>
        Error(nameof(UserLockoutNotEnabled), "El bloqueo no está habilitado para este usuario.");

    public override IdentityError UserAlreadyInRole(string role) =>
        Error(nameof(UserAlreadyInRole), $"El usuario ya tiene asignado el rol '{role}'.");

    public override IdentityError UserNotInRole(string role) =>
        Error(nameof(UserNotInRole), $"El usuario no tiene asignado el rol '{role}'.");

    public override IdentityError PasswordTooShort(int length) =>
        Error(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe contener al menos un carácter no alfanumérico.");

    public override IdentityError PasswordRequiresDigit() =>
        Error(nameof(PasswordRequiresDigit), "La contraseña debe contener al menos un dígito del 0 al 9.");

    public override IdentityError PasswordRequiresLower() =>
        Error(nameof(PasswordRequiresLower), "La contraseña debe contener al menos una letra minúscula.");

    public override IdentityError PasswordRequiresUpper() =>
        Error(nameof(PasswordRequiresUpper), "La contraseña debe contener al menos una letra mayúscula.");

    public override IdentityError RecoveryCodeRedemptionFailed() =>
        Error(nameof(RecoveryCodeRedemptionFailed), "No se pudo canjear el código de recuperación.");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Error(nameof(PasswordRequiresUniqueChars), $"La contraseña debe contener al menos {uniqueChars} caracteres diferentes.");

    private static IdentityError Error(string code, string description) =>
        new() { Code = code, Description = description };
}
