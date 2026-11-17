using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Web;

/// <summary>
/// Verifies the authorization policies through the real pipeline. A test
/// scheme authenticates the request with the roles in <see cref="RolesHeader"/>;
/// challenge and forbid stay with the Identity cookie, so denials redirect.
/// </summary>
public class RoleAccessTests : IClassFixture<IntegrationWebApplicationFactory>
{
    private const string RolesHeader = "X-Test-Roles";
    private const string AccessDeniedPath = "/Account/AccessDenied";

    private readonly WebApplicationFactory<CleanArchitectureTemplate.Web.Program> _factory;

    public RoleAccessTests(IntegrationWebApplicationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, HeaderRolesHandler>(
                        HeaderRolesHandler.SchemeName,
                        _ => { }
                    );
                services.PostConfigure<AuthenticationOptions>(options =>
                    options.DefaultAuthenticateScheme = HeaderRolesHandler.SchemeName
                );
            })
        );
    }

    [Theory]
    [InlineData("/Identity")]
    [InlineData("/Identity/UserList")]
    [InlineData("/Identity/RoleList")]
    public async Task SuperAdmin_IdentityPages_AreAuthorized(string path)
    {
        var response = await GetAsync(path, [RoleNames.User, RoleNames.SuperAdmin]);

        AssertAuthorized(response);
    }

    [Theory]
    [InlineData(RoleNames.Admin)]
    [InlineData(RoleNames.Manager)]
    [InlineData(RoleNames.User)]
    public async Task NonSuperAdmin_IdentityPages_RedirectToAccessDenied(string role)
    {
        var response = await GetAsync("/Identity/UserList", [RoleNames.User, role]);

        AssertAccessDenied(response);
    }

    // A page that needs only a signed-in user (Manage/* also loads the user from
    // the database, which the synthetic test user is not in).
    [Fact]
    public async Task User_AuthenticatedPage_IsAuthorized()
    {
        var response = await GetAsync("/ContosoUniversity/List", [RoleNames.User]);

        AssertAuthorized(response);
    }

    [Fact]
    public async Task Anonymous_IdentityPage_RedirectsToLogin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/Identity/UserList");

        var response = await CreateClient()
            .SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? "");
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
    }

    private async Task<HttpResponseMessage> GetAsync(string path, string[] roles)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add(RolesHeader, string.Join(',', roles));

        return await CreateClient().SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static void AssertAuthorized(HttpResponseMessage response)
    {
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.DoesNotContain(AccessDeniedPath, location);
        Assert.DoesNotContain("/Account/Login", location);
    }

    private static void AssertAccessDenied(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(AccessDeniedPath, response.Headers.Location?.ToString() ?? "");
    }

    private sealed class HeaderRolesHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "TestRoles";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(RolesHeader, out var header))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, "role-access-test@example.test"),
            };
            claims.AddRange(
                header
                    .ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(role => new Claim(ClaimTypes.Role, role))
            );

            // Identity's SignInManager.IsSignedIn checks this authentication type
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}