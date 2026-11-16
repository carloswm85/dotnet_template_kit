using System.ComponentModel;
using System.Text;
using CleanArchitectureTemplate.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Web;

public class DatabaseUnavailableMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_DatabaseConnectionTimeout_ReturnsServiceUnavailablePage()
    {
        var context = CreateContextWithResponseBody();
        var middleware = new DatabaseUnavailableMiddleware(
            _ => throw new Win32Exception(258),
            NullLogger<DatabaseUnavailableMiddleware>.Instance
        );

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        context.Response.ContentType.Should().Be("text/html; charset=utf-8");
        context.Response.Headers.RetryAfter.ToString().Should().Be("300");
        string body = await ReadResponseBodyAsync(context.Response);
        body.Should().Contain("HEALTH-004");
        body.Should().Contain("#dc3545");
        body.Should().Contain("ACTUAL SQL CONNECTIVITY OUTAGE");
        AssertApplicationContext(body);
        body.Should().Contain("Service temporarily unavailable");
        body.Should().Contain("cannot connect to the database");
    }

    [Fact]
    public async Task InvokeAsync_NonDatabaseException_RethrowsException()
    {
        var context = new DefaultHttpContext();
        var expectedException = new InvalidOperationException("Unexpected failure");
        var middleware = new DatabaseUnavailableMiddleware(
            _ => throw expectedException,
            NullLogger<DatabaseUnavailableMiddleware>.Instance
        );

        Func<Task> action = () => middleware.InvokeAsync(context);

        await action
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .Where(exception => ReferenceEquals(exception, expectedException));
    }

    [Fact]
    public async Task DiagnosticResponse_ReturnsPreparedMessageWithoutDatabaseAccess()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteDatabaseUnavailableDiagnosticAsync(
            context,
            TestContext.Current.CancellationToken
        );

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.Response.Headers.RetryAfter.ToString().Should().BeEmpty();
        string body = await ReadResponseBodyAsync(context.Response);
        body.Should().Contain("HEALTH-003");
        body.Should().Contain("#6f42c1");
        body.Should().Contain("DATABASE-ABSENCE TEST");
        AssertApplicationContext(body);
        body.Should().Contain("The application is prepared to handle a missing database.");
    }

    [Fact]
    public async Task LiveHealthResponse_ReturnsInformationalPage()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteLiveHealthAsync(
            context,
            TestContext.Current.CancellationToken
        );

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        string body = await ReadResponseBodyAsync(context.Response);
        body.Should().Contain("HEALTH-001");
        body.Should().Contain("#0d6efd");
        body.Should().Contain("LIVE HEALTH");
        AssertApplicationContext(body);
        body.Should().Contain("Application running");
        body.Should().Contain("It does not check the database connection");
    }

    [Fact]
    public async Task ReadyHealthResponse_Healthy_ReturnsReadyPage()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteReadyHealthAsync(
            context,
            HealthStatus.Healthy,
            TestContext.Current.CancellationToken
        );

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        string body = await ReadResponseBodyAsync(context.Response);
        body.Should().Contain("HEALTH-002");
        body.Should().Contain("#198754");
        body.Should().Contain("READY AND HEALTHY");
        AssertApplicationContext(body);
        body.Should().Contain("Application ready");
        body.Should().Contain("the database connection are available");
    }

    [Fact]
    public async Task ReadyHealthResponse_Unhealthy_ReturnsServiceUnavailablePage()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteReadyHealthAsync(
            context,
            HealthStatus.Unhealthy,
            TestContext.Current.CancellationToken
        );

        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        context.Response.Headers.RetryAfter.ToString().Should().Be("300");
        string body = await ReadResponseBodyAsync(context.Response);
        body.Should().Contain("HEALTH-002");
        body.Should().Contain("#fd7e14");
        body.Should().Contain("READY BUT UNHEALTHY");
        AssertApplicationContext(body);
        body.Should().Contain("Service temporarily unavailable");
        body.Should().Contain("not ready to serve requests");
    }

    private static DefaultHttpContext CreateContextWithResponseBody()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static void AssertApplicationContext(string body)
    {
        body.Should().Contain(ServiceStatusResponse.ApplicationName);
        body.Should().Contain("Current date and time:");
        body.Should().Contain("<time datetime=");
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Position = 0;

        using var reader = new StreamReader(
            response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true
        );

        return await reader.ReadToEndAsync();
    }
}
