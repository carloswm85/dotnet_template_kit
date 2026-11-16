using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.ApplicationCore.Entities;

/// <summary>
/// Runtime application setting stored in the database.
///
/// - Settings are shared by every host (Web and API) that uses the same
///   database, so a change propagates without a redeploy.
/// - Column types (varchar, precision) are configured in
///   <c>ApplicationDbContext.Partial.cs</c> to keep this layer free of EF Core.
/// </summary>
public class AppSetting
{
    [Key]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public bool IsPrivate { get; set; }

    [Required]
    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Value { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PreviousValue { get; set; }

    [StringLength(450)]
    public string? ModifiedByUserId { get; set; }

    public DateTimeOffset? ModificationDate { get; set; }
}
