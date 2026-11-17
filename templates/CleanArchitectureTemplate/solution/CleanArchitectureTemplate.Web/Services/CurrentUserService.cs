using System.Security.Claims;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;

namespace CleanArchitectureTemplate.Web.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string GetRequiredUserId() =>
        UserId
        ?? throw new UnauthorizedAccessException(
            "An authenticated user is required for this operation."
        );
}