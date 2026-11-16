using System.ComponentModel;
using System.Security.Claims;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Dtos;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using CleanArchitectureTemplate.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Web;

public class AppAvailabilityMiddlewareTests
{
    private const string PathBase = "/webapps/cleanarchitecturetemplate";

    [Fact]
    public async Task InvokeAsync_DatabaseConnectionTimeout_PropagatesToOutageMiddleware()
    {
        var appSettings = new Mock<IAppSettingService>(MockBehavior.Strict);
        appSettings
            .Setup(service => service.GetSingleUncached(AppSettingConstants.IsAppOnline))
            .Throws(new Win32Exception(258));
        var context = CreateContext(RoleConstants.AdministratorsRole);
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await Assert.ThrowsAsync<Win32Exception>(
            () => middleware.InvokeAsync(context, appSettings.Object)
        );
    }

    [Fact]
    public async Task InvokeAsync_AppOfflineAndSuperAdminRequest_AllowsRequest()
    {
        var appSettings = new Mock<IAppSettingService>(MockBehavior.Strict);
        var context = CreateContext(RoleConstants.SuperAdministratorsRole);
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, appSettings.Object);

        Assert.True(nextCalled);
        appSettings.Verify(
            service => service.GetSingleUncached(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task InvokeAsync_AppOfflineAndNonSuperAdminRequest_RedirectsToOutOfService()
    {
        var appSettings = new Mock<IAppSettingService>(MockBehavior.Strict);
        appSettings
            .Setup(service => service.GetSingleUncached(AppSettingConstants.IsAppOnline))
            .Returns(CreateAvailabilitySetting("false"));
        var context = CreateContext(RoleConstants.AdministratorsRole);
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, appSettings.Object);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.Equal(
            PathBase + AppAvailabilityMiddleware.OutOfServicePath,
            context.Response.Headers.Location.ToString()
        );
    }

    private static AppAvailabilityMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new AppAvailabilityMiddleware(
            next,
            NullLogger<AppAvailabilityMiddleware>.Instance
        );
    }

    private static DefaultHttpContext CreateContext(string role)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role)],
            authenticationType: "Test"
        );
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        context.Request.Method = HttpMethods.Get;
        context.Request.PathBase = PathBase;
        context.Request.Path = "/";

        return context;
    }

    private static AppSettingDto CreateAvailabilitySetting(string value)
    {
        return new AppSettingDto
        {
            Name = AppSettingConstants.IsAppOnline,
            Type = AppSettingConstants.BooleanType,
            Category = AppSettingConstants.ApplicationCategory,
            Description = "Application availability",
            Value = value,
        };
    }
}
