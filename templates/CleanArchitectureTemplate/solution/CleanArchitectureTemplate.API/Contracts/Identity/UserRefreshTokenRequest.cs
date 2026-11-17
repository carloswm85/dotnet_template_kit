namespace CleanArchitectureTemplate.API.Contracts.Identity;

public sealed class UserRefreshTokenRequest
{
    // TODO
    // Should I include this field for device-bound tokens?
    // How do I really implement it?
    public string? DeviceId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
