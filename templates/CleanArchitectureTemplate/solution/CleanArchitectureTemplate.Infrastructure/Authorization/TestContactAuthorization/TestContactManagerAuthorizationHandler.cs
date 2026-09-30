using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Entities;

namespace CleanArchitectureTemplate.Infrastructure.Authorization.TestContactAuthorization;

public class TestContactManagerAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, TestContact>
{
    /// <summary>
    /// Handles authorization for manager users performing approval-related operations on
    /// <see cref="TestContact"/> resources.
    /// </summary>
    /// <param name="context">
    /// The authorization context containing the current user principal.
    /// </param>
    /// <param name="requirement">
    /// The operation requirement (Approve or Reject) being evaluated.
    /// </param>
    /// <param name="resource">
    /// The <see cref="TestContact"/> resource being accessed.
    /// </param>
    /// <returns>
    /// - Calls <c>context.Succeed(requirement)</c> if the current user is in the manager role
    ///   and the operation is an approval-related action.
    /// - Returning <see cref="Task.CompletedTask"/> without calling <c>context.Succeed</c>
    ///   or <c>context.Fail</c> makes no explicit success or failure determination,
    ///   allowing other handlers to participate.
    /// - To explicitly deny access, call <c>context.Fail()</c>.
    /// </returns>
    /// <remarks>
    /// This handler authorizes only approval-related operations (Approve and Reject)
    /// and grants access exclusively to users in the manager role.
    /// It does not address CRUD permissions; those are expected to be handled by other handlers
    /// such as owner-based or administrator-based authorization handlers.
    /// By not calling <c>context.Fail</c> when the user is not in the manager role,
    /// this handler allows the authorization pipeline to continue, enabling other handlers
    /// to evaluate and potentially authorize the request.
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

        // If not asking for approval/reject, return.
        if (
            requirement.Name != OperationConstants.ApproveOperationName
            && requirement.Name != OperationConstants.RejectOperationName
        )
        {
            return Task.CompletedTask;
        }

        // Managers can approve or reject.
        if (context.User.IsInRole(RoleConstants.ManagersRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
