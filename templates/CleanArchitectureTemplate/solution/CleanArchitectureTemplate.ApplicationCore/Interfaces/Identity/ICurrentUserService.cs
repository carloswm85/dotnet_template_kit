namespace CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;

/// <summary>
/// Gives application code the signed-in user's ID without depending on
/// HttpContext.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }

    string GetRequiredUserId();
}