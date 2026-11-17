using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Web.Controllers.Identity;
using CleanArchitectureTemplate.Web.Services.Interfaces;
using Moq;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Controllers.Identity;

public class AccountControllerTests
{
    [Fact]
    public void Login_AuthenticatedUser_RedirectsToHome()
    {
        var controller = CreateController(isAuthenticated: true);

        var result = controller.Login();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void Login_AnonymousUser_ReturnsLoginView()
    {
        var controller = CreateController(isAuthenticated: false);

        var result = controller.Login();

        Assert.IsType<ViewResult>(result);
    }

    private static AccountController CreateController(bool isAuthenticated)
    {
        var identity = isAuthenticated
            ? new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "test@example.com")],
                authenticationType: "Test"
            )
            : new ClaimsIdentity();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        return new AccountController(
            Mock.Of<IUserManagerService>(),
            Mock.Of<ISignInManagerService>(),
            Mock.Of<IEmailSender>(),
            Mock.Of<ISmsSender>(),
            NullLoggerFactory.Instance,
            Mock.Of<IRoleManagerService>(),
            Mock.Of<IImageStorageService>()
        )
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }
}
