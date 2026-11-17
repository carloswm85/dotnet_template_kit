using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Services.Identity;

public class TokenService : ITokenService
{
    public const string IssuedAtMillisecondsClaimType = "iat_ms";

    // Configuration instance used to read JWT settings from appsettings.json
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _cache;

    public TokenService(IDistributedCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a signed JWT token containing user identity and role claims.
    /// </summary>
    /// <param name="userId">Unique identifier of the user</param>
    /// <param name="email">User email address</param>
    /// <param name="roles">List of roles assigned to the user</param>
    /// <returns>Serialized JWT token string</returns>
    public string GenerateToken(string userId, string email, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = GetRequiredSetting(jwtSettings, "SecretKey");
        var issuer = GetRequiredSetting(jwtSettings, "Issuer");
        var audience = GetRequiredSetting(jwtSettings, "Audience");
        var expirationMinutes = GetPositiveMinutes(jwtSettings, "ExpirationMinutes");

        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 bytes.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = Guid.NewGuid().ToString();
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var iatMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        // IMPORTANT: Create standard JWT claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId), // optional but good practice
            new Claim(JwtRegisteredClaimNames.Jti, jti), // Unique token identifier (prevents token replay attacks)
            new Claim(JwtRegisteredClaimNames.Iat, iat), // Issued at (helps debugging)
            new Claim(IssuedAtMillisecondsClaimType, iatMilliseconds),
        };

        // Add role claims to the token (used for authorization)
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create the JWT token with issuer, audience, expiration, claims, and signing credentials
        /* Recommended minimal JWT claims:
         *   ✓ sub — subject (user ID)
         *   ✓ jti — JWT ID (unique token identifier)
         *   ✓ role — user role(s) / permissions
         *   ✓ exp — expiration time (Unix timestamp)
         *   ✓ iss — issuer (who created the token)
         *   ✓ aud — audience (who the token is intended for)
         * That's the practical minimum for secure, usable tokens in most applications.
         */
        var token = new JwtSecurityToken(
            issuer: issuer, // Token issuer (who created the token)
            audience: audience, // Intended audience (who can use the token)
            claims: claims, // Claims included in the token
            expires: DateTime.UtcNow.AddMinutes( // Token expiration time
                expirationMinutes
            ),
            signingCredentials: credentials // Cryptographic signing credentials
        );

        // Serialize the JWT token into a compact string format
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Revokes a specific JWT token by adding it to the blacklist.
    /// </summary>
    /// <param name="token">The JWT token string to revoke</param>
    public async Task RevokeTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var jti = jwtToken.Id; // JTI
        var expirationTime = jwtToken.ValidTo;

        await _cache.SetStringAsync(
            $"blacklist:{jti}",
            "revoked",
            new DistributedCacheEntryOptions
            { AbsoluteExpiration = expirationTime },
            cancellationToken
        );
    }

    /// <summary>
    /// Checks if a token has been revoked.
    /// </summary>
    /// <param name="jti">The JWT ID (jti) claim from the token</param>
    /// <returns>True if the token is revoked, false otherwise</returns>
    public async Task<bool> IsTokenRevokedAsync(
        string jti,
        CancellationToken cancellationToken = default
    )
    {
        var value = await _cache.GetStringAsync($"blacklist:{jti}", cancellationToken);
        return value != null;
    }

    /// <summary>
    /// Invalidates all tokens for a specific user by setting a "user revocation timestamp".
    /// Any token issued before this timestamp will be considered invalid.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be invalidated</param>
    /// <remarks>
    /// This is more efficient than blacklisting individual tokens. It creates a single
    /// cache entry per user that marks all tokens issued before the current time as invalid.
    /// The cache entry expires based on the maximum token lifetime to avoid keeping
    /// unnecessary data indefinitely.
    /// </remarks>
    public async Task InvalidateAllRefreshTokensAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = GetPositiveMinutes(jwtSettings, "ExpirationMinutes");

        // Set a revocation timestamp for this user
        // All tokens issued before this timestamp will be considered invalid
        var revocationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        await _cache.SetStringAsync(
            $"user_revoked:{userId}",
            revocationTimestamp,
            new DistributedCacheEntryOptions
            {
                // Keep this entry until all tokens could have expired
                // Add some buffer time to ensure all tokens are covered
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes + 10),
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Checks if a user has had all their tokens invalidated.
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <param name="tokenIssuedAtUnixMilliseconds">
    /// The Unix timestamp in milliseconds when the token was issued.
    /// </param>
    /// <returns>True if the user's tokens were invalidated after this token was issued</returns>
    public async Task<bool> IsUserRevokedAsync(
        string userId,
        long tokenIssuedAtUnixMilliseconds,
        CancellationToken cancellationToken = default
    )
    {
        var revocationTimestamp = await _cache.GetStringAsync(
            $"user_revoked:{userId}",
            cancellationToken
        );

        if (string.IsNullOrEmpty(revocationTimestamp))
            return false;

        // A token issued before the revocation timestamp is invalid.
        return tokenIssuedAtUnixMilliseconds < long.Parse(revocationTimestamp);
    }

    #region Refresh Token

    /// <summary>
    /// Extracts a <see cref="ClaimsPrincipal"/> from an expired JWT access token.
    /// </summary>
    /// <remarks>
    /// This method intentionally disables lifetime validation in order to allow
    /// expired access tokens to be validated during the refresh process.
    ///
    /// ⚠️ IMPORTANT:
    /// - Signature, issuer, and audience are still validated.
    /// - Lifetime is NOT validated because the token is expected to be expired.
    /// - This method should ONLY be used in refresh-token scenarios.
    /// </remarks>
    /// <param name="token">The expired JWT access token.</param>
    /// <returns>
    /// A <see cref="ClaimsPrincipal"/> if the token is valid (ignoring expiration);
    /// otherwise throws <see cref="SecurityTokenException"/>.
    /// </returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,

            // IMPORTANT: We disable lifetime validation because
            // the access token is expected to be expired during refresh.
            ValidateLifetime = false,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            // Validate signature using the configured symmetric key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(
            token,
            tokenValidationParameters,
            out SecurityToken securityToken
        );

        // Ensure the token is a valid JWT and was signed using HMAC SHA256
        return securityToken is not JwtSecurityToken jwtToken
            || !jwtToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase
            )
            ? throw new SecurityTokenException("Invalid token")
            : principal;
    }

    /// <summary>
    /// Generates a new refresh token value.
    /// </summary>
    /// <remarks>
    /// This implementation uses a GUID encoded as Base64.
    ///
    /// ⚠️ NOTE:
    /// You may want to include a secure refresh token workflow. This would
    /// require the inclusion of a table schema and additional code.
    /// </remarks>
    /// <returns>A new refresh token string.</returns>
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Stores a refresh token in the distributed cache with an absolute expiration.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="refreshToken">The refresh token to store.</param>
    /// <remarks>
    /// The token is stored using a composite key format:
    ///     refresh:{userId}:{refreshToken}
    ///
    /// The expiration is controlled by configuration:
    ///     JwtSettings:RefreshTokenLifetimeMinutes
    ///
    /// </remarks>
    public async Task StoreRefreshTokenAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var lifetimeMinutes = GetPositiveMinutes(jwtSettings, "RefreshTokenLifetimeMinutes");

        await _cache.SetStringAsync(
            $"refresh:{userId}:{refreshToken}",
            "valid", // marker value (could be replaced with metadata if needed)
            new DistributedCacheEntryOptions
            {
                // Absolute expiration ensures the token becomes invalid after the configured duration
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(lifetimeMinutes),
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Validates whether a refresh token exists in the distributed cache.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <returns>
    /// True if the refresh token exists and is valid; otherwise false.
    /// </returns>
    /// <remarks>
    /// This method checks for the existence of the cache entry.
    /// If the entry is missing, the token is considered invalid or expired.
    /// </remarks>
    public async Task<bool> ValidateRefreshTokenAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var value = await _cache.GetStringAsync(
            $"refresh:{userId}:{refreshToken}",
            cancellationToken
        );

        return value != null;
    }

    /// <summary>
    /// Revokes (removes) a refresh token from the distributed cache.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <remarks>
    /// This operation invalidates the refresh token immediately,
    /// preventing further use (e.g., during logout or token rotation).
    /// </remarks>
    public async Task RevokeRefreshTokenAsync(
        string userId,
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        await _cache.RemoveAsync($"refresh:{userId}:{refreshToken}", cancellationToken);
    }

    #endregion

    private static string GetRequiredSetting(IConfigurationSection settings, string key)
    {
        return settings[key] is { } value && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"JWT {key} is not configured.");
    }

    private static double GetPositiveMinutes(IConfigurationSection settings, string key)
    {
        return !double.TryParse(settings[key], out var value) || value <= 0
            ? throw new InvalidOperationException($"JWT {key} must be greater than zero.")
            : value;
    }
}
