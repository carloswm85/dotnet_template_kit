using System.IdentityModel.Tokens.Jwt;
using CleanArchitectureTemplate.Infrastructure.Services.Identity;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Services.Identity;

public class TokenServiceTests
{
    private const string UserId = "4b7f1c2e-0000-4000-8000-000000000001";

    [Fact]
    public void GenerateToken_IncludesMillisecondIssuedAtClaim()
    {
        var service = CreateService();

        var token = service.GenerateToken(UserId, "user@example.com", ["User"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should()
            .Contain(claim => claim.Type == TokenService.IssuedAtMillisecondsClaimType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("too-short-secret")]
    public void GenerateToken_MissingOrShortSecret_Throws(string? secretKey)
    {
        var service = CreateService(secretKey);

        var action = () => service.GenerateToken(UserId, "user@example.com", []);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task RevokeTokenAsync_MarksTokenAsRevoked()
    {
        var service = CreateService();
        var token = service.GenerateToken(UserId, "user@example.com", []);
        var jti = new JwtSecurityTokenHandler().ReadJwtToken(token).Id;

        await service.RevokeTokenAsync(token, TestContext.Current.CancellationToken);

        (await service.IsTokenRevokedAsync(jti, TestContext.Current.CancellationToken))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task InvalidateAllRefreshTokensAsync_RevokesTokensIssuedBefore()
    {
        var service = CreateService();
        var issuedBefore = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1;

        await service.InvalidateAllRefreshTokensAsync(
            UserId,
            TestContext.Current.CancellationToken
        );

        (await service.IsUserRevokedAsync(
            UserId,
            issuedBefore,
            TestContext.Current.CancellationToken
        ))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task InvalidateAllRefreshTokensAsync_KeepsTokensIssuedAfter()
    {
        var service = CreateService();

        await service.InvalidateAllRefreshTokensAsync(
            UserId,
            TestContext.Current.CancellationToken
        );
        var issuedAfter = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1;

        (await service.IsUserRevokedAsync(
            UserId,
            issuedAfter,
            TestContext.Current.CancellationToken
        ))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task RefreshToken_StoreValidateRevoke_RoundTrip()
    {
        var service = CreateService();
        var refreshToken = service.GenerateRefreshToken();
        var cancellationToken = TestContext.Current.CancellationToken;

        await service.StoreRefreshTokenAsync(UserId, refreshToken, cancellationToken);
        (await service.ValidateRefreshTokenAsync(UserId, refreshToken, cancellationToken))
            .Should()
            .BeTrue();

        await service.RevokeRefreshTokenAsync(UserId, refreshToken, cancellationToken);
        (await service.ValidateRefreshTokenAsync(UserId, refreshToken, cancellationToken))
            .Should()
            .BeFalse();
    }

    private static TokenService CreateService(
        string? secretKey = "integration-test-secret-key-with-32-plus-bytes!"
    )
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = secretKey,
                    ["JwtSettings:Issuer"] = "https://issuer.test",
                    ["JwtSettings:Audience"] = "https://audience.test",
                    ["JwtSettings:ExpirationMinutes"] = "60",
                    ["JwtSettings:RefreshTokenLifetimeMinutes"] = "10080",
                }
            )
            .Build();

        IDistributedCache cache = new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions())
        );

        return new TokenService(cache, configuration);
    }
}