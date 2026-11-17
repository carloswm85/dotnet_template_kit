using System.Net;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Web;

/// <summary>
/// The Authentication rate-limit policy (10 requests per minute per IP by
/// default) rejects further login attempts with 429.
/// </summary>
public class RateLimitingTests : IClassFixture<IntegrationWebApplicationFactory>
{
    private readonly IntegrationWebApplicationFactory _factory;

    public RateLimitingTests(IntegrationWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginPost_OverLimit_Returns429()
    {
        var client = _factory.CreateClient();
        HttpStatusCode lastStatus = HttpStatusCode.OK;

        for (var attempt = 0; attempt < 11; attempt++)
        {
            using var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = "nobody@example.test",
                    ["Password"] = "wrong",
                }
            );
            var response = await client.PostAsync(
                "/Account/Login",
                content,
                TestContext.Current.CancellationToken
            );
            lastStatus = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastStatus);
    }
}