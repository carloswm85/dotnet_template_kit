using System.ComponentModel;
using System.Text.Json;
using CleanArchitectureTemplate.API.Middleware;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.API;

public class DatabaseUnavailableHandlingTests
{
    [Fact]
    public async Task OutageMiddleware_DatabaseConnectionTimeout_ReturnsJsonServiceUnavailable()
    {
        var context = CreateContextWithResponseBody();
        var middleware = new DatabaseUnavailableMiddleware(
            _ => throw new Win32Exception(258),
            NullLogger<DatabaseUnavailableMiddleware>.Instance
        );

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        context.Response.ContentType.Should().Be("application/json; charset=utf-8");
        context.Response.Headers.RetryAfter.ToString().Should().Be("300");

        JsonElement body = await ReadResponseBodyAsync(context.Response);
        AssertCommonResponse(body, StatusCodes.Status503ServiceUnavailable, "HEALTH-004");
        body.GetProperty("headerMessage")
            .GetString()
            .Should()
            .Be("ACTUAL SQL CONNECTIVITY OUTAGE");
        body.GetProperty("title").GetString().Should().Be("Service temporarily unavailable");
    }

    [Fact]
    public async Task OutageMiddleware_NonDatabaseException_RethrowsException()
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
    public void ExceptionDetector_NestedConnectionTimeout_ReturnsTrue()
    {
        var exception = new InvalidOperationException(
            "Database operation failed.",
            new Win32Exception(258)
        );

        DatabaseUnavailableExceptionDetector.IsDatabaseUnavailable(exception).Should().BeTrue();
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

        JsonElement body = await ReadResponseBodyAsync(context.Response);
        AssertCommonResponse(body, StatusCodes.Status200OK, "HEALTH-003");
        body.GetProperty("headerMessage").GetString().Should().Be("DATABASE-ABSENCE TEST");
        body.GetProperty("message")
            .GetString()
            .Should()
            .Be("The API is prepared to handle a missing database.");
    }

    [Fact]
    public async Task LiveHealthResponse_ReturnsInformationalJson()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteLiveHealthAsync(
            context,
            TestContext.Current.CancellationToken
        );

        JsonElement body = await ReadResponseBodyAsync(context.Response);
        AssertCommonResponse(body, StatusCodes.Status200OK, "HEALTH-001");
        body.GetProperty("headerMessage").GetString().Should().Be("LIVE HEALTH");
        body.GetProperty("detail")
            .GetString()
            .Should()
            .Contain("It does not check the database connection");
    }

    [Fact]
    public async Task ReadyHealthResponse_Healthy_ReturnsReadyJson()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteReadyHealthAsync(
            context,
            HealthStatus.Healthy,
            TestContext.Current.CancellationToken
        );

        JsonElement body = await ReadResponseBodyAsync(context.Response);
        AssertCommonResponse(body, StatusCodes.Status200OK, "HEALTH-002");
        body.GetProperty("headerMessage").GetString().Should().Be("READY AND HEALTHY");
        body.GetProperty("message")
            .GetString()
            .Should()
            .Contain("the database connection are available");
    }

    [Fact]
    public async Task ReadyHealthResponse_Unhealthy_ReturnsServiceUnavailableJson()
    {
        var context = CreateContextWithResponseBody();

        await ServiceStatusResponse.WriteReadyHealthAsync(
            context,
            HealthStatus.Unhealthy,
            TestContext.Current.CancellationToken
        );

        context.Response.Headers.RetryAfter.ToString().Should().Be("300");
        JsonElement body = await ReadResponseBodyAsync(context.Response);
        AssertCommonResponse(body, StatusCodes.Status503ServiceUnavailable, "HEALTH-002");
        body.GetProperty("headerMessage").GetString().Should().Be("READY BUT UNHEALTHY");
        body.GetProperty("detail").GetString().Should().Contain("not ready to serve requests");
    }

    [Fact]
    public async Task AvailabilityMiddleware_DatabaseConnectionTimeout_PropagatesToOutageMiddleware()
    {
        var appSettings = new Mock<IAppSettingService>(MockBehavior.Strict);
        appSettings
            .Setup(service => service.GetSingleUncached("IsAppOnline"))
            .Throws(new Win32Exception(258));
        var context = CreateApiRequestContext("/api/v1/example");
        var middleware = CreateAvailabilityMiddleware(_ => Task.CompletedTask);

        await Assert.ThrowsAsync<Win32Exception>(
            () => middleware.InvokeAsync(context, appSettings.Object)
        );
    }

    [Fact]
    public async Task AvailabilityMiddleware_DiagnosticPath_BypassesDatabaseConfiguration()
    {
        var appSettings = new Mock<IAppSettingService>(MockBehavior.Strict);
        var context = CreateApiRequestContext(
            ServiceStatusResponse.DatabaseUnavailableDiagnosticPath
        );
        bool nextCalled = false;
        var middleware = CreateAvailabilityMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, appSettings.Object);

        nextCalled.Should().BeTrue();
        appSettings.Verify(
            service => service.GetSingleUncached(It.IsAny<string>()),
            Times.Never
        );
    }

    private static AppAvailabilityMiddleware CreateAvailabilityMiddleware(RequestDelegate next)
    {
        return new AppAvailabilityMiddleware(
            next,
            NullLogger<AppAvailabilityMiddleware>.Instance
        );
    }

    private static DefaultHttpContext CreateContextWithResponseBody()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static DefaultHttpContext CreateApiRequestContext(string path)
    {
        var context = CreateContextWithResponseBody();
        context.Request.Method = HttpMethods.Get;
        context.Request.PathBase = "/webapis/cleanarchitecturetemplate";
        context.Request.Path = path;

        return context;
    }

    private static async Task<JsonElement> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(response.Body);

        return document.RootElement.Clone();
    }

    private static void AssertCommonResponse(
        JsonElement body,
        int expectedStatus,
        string expectedDisplayCode
    )
    {
        body.GetProperty("applicationName")
            .GetString()
            .Should()
            .Be(ServiceStatusResponse.ApplicationName);
        body.GetProperty("timestamp").GetDateTimeOffset().Should().NotBe(default);
        body.GetProperty("timeZone").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("status").GetInt32().Should().Be(expectedStatus);
        body.GetProperty("displayCode").GetString().Should().Be(expectedDisplayCode);
        body.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
