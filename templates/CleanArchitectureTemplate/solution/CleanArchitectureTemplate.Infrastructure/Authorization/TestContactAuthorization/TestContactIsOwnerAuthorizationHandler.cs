using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Entities;
using CleanArchitectureTemplate.Infrastructure.Model.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Authorization.TestContactAuthorization;

public class TestContactIsOwnerAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, TestContact>
{
    UserManager<ApplicationUser> _userManager;

    public TestContactIsOwnerAuthorizationHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Handles ownership-based authorization for <see cref="TestContact"/> resources.
    /// </summary>
    /// <param name="context">
    /// The authorization context containing the current user principal.
    /// </param>
    /// <param name="requirement">
    /// The operation requirement (Create, Read, Update, Delete) being evaluated.
    /// </param>
    /// <param name="resource">
    /// The <see cref="TestContact"/> resource being accessed.
    /// </param>
    /// <returns>
    /// - Calls <c>context.Succeed(requirement)</c> if the current authenticated user is the contact owner.
    /// - If the requirement does not match a CRUD operation, no decision is made and other handlers may run.
    /// - Returning <see cref="Task.CompletedTask"/> without a prior call to <c>context.Succeed</c>
    ///   or <c>context.Fail</c> indicates no explicit success or failure.
    /// - Call <c>context.Fail()</c> to explicitly fail the authorization check if needed.
    /// </returns>
    /// <remarks>
    /// This handler participates in a multi-handler authorization strategy,
    /// where administrators and managers can succeed requirements without ownership.
    /// By not explicitly failing non-owner access, this handler allows
    /// other role-based handlers to grant access when appropriate.
    /// Ownership is determined by comparing the <c>OwnerID</c> of the resource
    /// with the authenticated user's identifier resolved via <c>UserManager</c>.
    /// </remarks>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        TestContact resource
    )
    {
        if (context.User == null || resource == null)
        {
            return Task.CompletedTask;
        }

        // If not asking for CRUD permission, return.
        if (
            requirement.Name != OperationConstants.CreateOperationName
            && requirement.Name != OperationConstants.ReadOperationName
            && requirement.Name != OperationConstants.UpdateOperationName
            && requirement.Name != OperationConstants.DeleteOperationName
        )
        {
            return Task.CompletedTask;
        }

        // If the current authenticated user is the contact owner.
        if (resource.OwnerID == _userManager.GetUserId(context.User))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
