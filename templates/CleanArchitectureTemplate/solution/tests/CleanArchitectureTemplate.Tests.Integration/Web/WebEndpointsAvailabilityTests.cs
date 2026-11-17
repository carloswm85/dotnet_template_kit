using FluentAssertions;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Web;

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
