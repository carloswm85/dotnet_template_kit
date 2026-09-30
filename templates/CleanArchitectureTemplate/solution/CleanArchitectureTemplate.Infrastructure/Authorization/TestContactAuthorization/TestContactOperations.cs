using Microsoft.AspNetCore.Authorization.Infrastructure;
using CleanArchitectureTemplate.ApplicationCore.Constants;

namespace CleanArchitectureTemplate.Infrastructure.Authorization.TestContactAuthorization;

/// <summary>
/// Provides predefined operation requirements for contact-related authorization actions.
/// </summary>
/// <remarks>
/// This class defines static instances of common authorization operations, such as create, read, update,
/// delete, approve, and reject, for use with contact resources. These requirements can be used with authorization
/// handlers to enforce access control policies in applications that manage contacts.
/// </remarks>
public static class TestContactOperations
{
    public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement
    {
        Name = OperationConstants.CreateOperationName,
    };
    public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement
    {
        Name = OperationConstants.ReadOperationName,
    };
    public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement
    {
        Name = OperationConstants.UpdateOperationName,
    };
    public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement
    {
        Name = OperationConstants.DeleteOperationName,
    };
    public static OperationAuthorizationRequirement Approve = new OperationAuthorizationRequirement
    {
        Name = OperationConstants.ApproveOperationName,
    };
    public static OperationAuthorizationRequirement Reject = new OperationAuthorizationRequirement
    {
        Name = OperationConstants.RejectOperationName,
    };
}
