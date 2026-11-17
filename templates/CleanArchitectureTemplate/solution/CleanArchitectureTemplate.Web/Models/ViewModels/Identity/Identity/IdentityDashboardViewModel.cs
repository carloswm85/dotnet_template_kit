namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;

/// <summary>
/// Data for the administration panel (Identity/Index): account KPIs and users per role.
/// </summary>
public class IdentityDashboardViewModel
{
    public const int NewUsersWindowDays = 30;

    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int UnconfirmedEmailUsers { get; set; }
    public int LockedOutUsers { get; set; }
    public int TwoFactorUsers { get; set; }
    public int NewUsers { get; set; }

    public List<IdentityDashboardRoleItemViewModel> Roles { get; set; } = [];

    #region Needs attention

    public const int AttentionListSize = 5;

    /// <summary>Total users needing attention (the list shows at most <see cref="AttentionListSize"/>).</summary>
    public int AttentionTotal { get; set; }

    public List<IdentityAttentionItemViewModel> AttentionItems { get; set; } = [];

    #endregion
}

/// <summary>
/// A user account that likely needs an administrator: locked out, inactive,
/// or holding no business role (only the base `User` role, or none).
/// Unconfirmed email is intentionally not a criterion until the email service is available.
/// </summary>
public class IdentityAttentionItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? RegistrationDate { get; set; }
    public bool IsLockedOut { get; set; }
    public bool IsInactive { get; set; }
    public bool HasOnlyBaseRole { get; set; }
}

public class IdentityDashboardRoleItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
    public int UserCount { get; set; }
}
