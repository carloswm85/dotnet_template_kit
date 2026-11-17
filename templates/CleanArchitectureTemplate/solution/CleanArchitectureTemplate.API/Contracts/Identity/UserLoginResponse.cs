using CleanArchitectureTemplate.API.Contracts.Identity.IdentityContractDtos;

namespace CleanArchitectureTemplate.API.Contracts.Identity;

public class UserLoginResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserInformationResponse? User { get; set; }
    public required string Message { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string? TokenType { get; set; }
}
