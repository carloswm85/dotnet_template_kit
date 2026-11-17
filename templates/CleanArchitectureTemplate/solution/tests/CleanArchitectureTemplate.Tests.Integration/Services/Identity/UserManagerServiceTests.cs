using Microsoft.AspNetCore.Identity;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using CleanArchitectureTemplate.Infrastructure.Services.Identity;
using Moq;
using Xunit;

namespace CleanArchitectureTemplate.Tests.Integration.Services.Identity;

public class UserManagerServiceTests
{
    [Fact]
    public async Task CreateAsync_WithPassword_AssignsBaseUserRole()
    {
        const string password = "Test-password-123!";
        var user = new ApplicationUser();
        var userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.CreateAsync(user, password))
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(manager => manager.AddToRoleAsync(user, RoleNames.User))
            .ReturnsAsync(IdentityResult.Success);
        var service = new UserManagerService(userManager.Object);

        var result = await service.CreateAsync(user, password);

        Assert.True(result.Succeeded);
        userManager.Verify(
            manager => manager.AddToRoleAsync(user, RoleNames.User),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_WithoutPassword_AssignsBaseUserRole()
    {
        var user = new ApplicationUser();
        var userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.CreateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(manager => manager.AddToRoleAsync(user, RoleNames.User))
            .ReturnsAsync(IdentityResult.Success);
        var service = new UserManagerService(userManager.Object);

        var result = await service.CreateAsync(user);

        Assert.True(result.Succeeded);
        userManager.Verify(
            manager => manager.AddToRoleAsync(user, RoleNames.User),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_BaseRoleAssignmentFails_DeletesCreatedUser()
    {
        var user = new ApplicationUser();
        var roleError = new IdentityError
        {
            Code = "RoleAssignmentFailed",
            Description = "The base role could not be assigned."
        };
        var userManager = CreateUserManager();
        userManager
            .Setup(manager => manager.CreateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(manager => manager.AddToRoleAsync(user, RoleNames.User))
            .ReturnsAsync(IdentityResult.Failed(roleError));
        userManager
            .Setup(manager => manager.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        var service = new UserManagerService(userManager.Object);

        var result = await service.CreateAsync(user);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == roleError.Code);
        userManager.Verify(manager => manager.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task RemoveFromRolesAsync_BaseAndOtherRole_KeepsBaseUserRole()
    {
        var user = new ApplicationUser();
        var userManager = CreateUserManager();
        userManager
            .Setup(manager =>
                manager.RemoveFromRolesAsync(
                    user,
                    It.Is<IEnumerable<string>>(roles =>
                        roles.SequenceEqual(new[] { RoleNames.Admin })
                    )
                )
            )
            .ReturnsAsync(IdentityResult.Success);
        var service = new UserManagerService(userManager.Object);

        var result = await service.RemoveFromRolesAsync(
            user,
            [RoleNames.User, RoleNames.Admin]
        );

        Assert.True(result.Succeeded);
        userManager.VerifyAll();
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
    }
}
