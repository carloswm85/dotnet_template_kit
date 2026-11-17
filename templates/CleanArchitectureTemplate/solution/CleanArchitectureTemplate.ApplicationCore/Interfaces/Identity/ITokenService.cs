using System.Security.Claims;

namespace CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;

public interface ITokenService
{
    string GenerateToken(string userId, string email, IList<string> roles);
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default);
    Task InvalidateAllRefreshTokensAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsUserRevokedAsync(
        string userId,
        long tokenIssuedAtUnixMilliseconds,
        CancellationToken cancellationToken = default
    );

    #region Refresh Token

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string GenerateRefreshToken();
    Task StoreRefreshTokenAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    );
    Task<bool> ValidateRefreshTokenAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    );
    Task RevokeRefreshTokenAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    );

    #endregion
}
