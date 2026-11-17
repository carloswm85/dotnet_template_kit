using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureTemplate.Infrastructure.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? ImagePath { get; set; }

    /// <summary>Date the account was created (UTC).</summary>
    public DateOnly? RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
