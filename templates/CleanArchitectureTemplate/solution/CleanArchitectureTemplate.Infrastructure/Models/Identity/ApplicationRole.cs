using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Models.Identity;

public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Indicates if the role is automatically assigned to new users.
    /// Common use case:
    ///     `User` role assigned on registration.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Controls role visibility.
    ///     true → Visible/selectable in UI
    ///     false → Internal/system role
    /// Useful for hiding technical roles from admin screens.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Marks roles that are system-defined.
    /// Static roles:
    ///     Usually created by migrations or seed data
    ///     Cannot be deleted via UI
    /// Example: `Admin`, `HostAdmin`
    /// </summary>
    public bool IsStatic { get; set; } = false;
}

// Inherited properties from IdentityUser:
/*
public virtual TKey Id { get; set; } = default!;
public virtual string? Name { get; set; }
public virtual string? NormalizedName { get; set; }
public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
*/
