namespace CleanArchitectureTemplate.ApplicationCore.Constants;

/// <summary>
/// Provides constant values for common operation names used throughout the application.
/// </summary>
/// <remarks>
/// This class defines string constants for standard operations such as create, read, update, delete,
/// approve, and reject. Use them for consistency in authorization checks and logging.
/// </remarks>
public static class OperationConstants
{
    // Operations
    public static readonly string CreateOperationName = "Create";
    public static readonly string ReadOperationName = "Read";
    public static readonly string UpdateOperationName = "Update";
    public static readonly string DeleteOperationName = "Delete";
    public static readonly string ApproveOperationName = "Approve";
    public static readonly string RejectOperationName = "Reject";

    // Role names live in RoleNames.
}
