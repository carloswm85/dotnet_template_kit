namespace CleanArchitectureTemplate.ApplicationCore.Constants;

/// <summary>
/// Rate-limiting policies. Limits are read from the "RateLimiting" section of
/// appsettings.json.
/// </summary>
public static class RateLimitPolicyNames
{
    /// <summary>Login, register, password reset and token refresh.</summary>
    public const string Authentication = "Authentication";
}