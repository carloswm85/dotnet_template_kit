using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using CleanArchitectureTemplate.Web;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Web;

public sealed class IntegrationWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Server=(localdb)\\MSSQLLocalDB;Database=CleanArchitectureTemplateIntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
                }
            );
        });
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        });
    }
}

public class WebEndpointsAvailabilityTests : IClassFixture<IntegrationWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WebEndpointsAvailabilityTests(IntegrationWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);
        response.IsSuccessStatusCode.Should().BeTrue(because: "The home page should be reachable");
    }

    [Fact]
    public async Task TestStudentsListPage_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync(
            "/ContosoUniversity/List",
            TestContext.Current.CancellationToken
        );
        response
            .IsSuccessStatusCode.Should()
            .BeTrue(because: "The students listing page should be reachable");
    }

    [Fact]
    public async Task TestStudentsStatistics_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync(
            "/ContosoUniversity/Statistics",
            TestContext.Current.CancellationToken
        );
        response
            .IsSuccessStatusCode.Should()
            .BeTrue(because: "The student statistics page should be reachable");
    }

    [Fact]
    public async Task NewTestStudentCreation_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync(
            "/ContosoUniversity/Create",
            TestContext.Current.CancellationToken
        );
        response
            .IsSuccessStatusCode.Should()
            .BeTrue(because: "The student creation page should be reachable");
    }

    [Fact]
    public async Task SelectedTestStudentDetails_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync(
            "/ContosoUniversity/Details/40",
            TestContext.Current.CancellationToken
        );
        response
            .IsSuccessStatusCode.Should()
            .BeTrue(because: "The student details page should be reachable");
    }

    [Fact]
    public async Task SelectedTestStudentDelete_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync(
            "/ContosoUniversity/Delete/1",
            TestContext.Current.CancellationToken
        );
        response
            .IsSuccessStatusCode.Should()
            .BeTrue(because: "The student delete page should be reachable");
    }

    [Fact]
    public async Task SelectedTestStudentEdit_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync(
            "/ContosoUniversity/Edit/40",
            TestContext.Current.CancellationToken
        );
        response
            .IsSuccessStatusCode.Should()
            .BeTrue(because: "The student edit page should be reachable");
    }
}
