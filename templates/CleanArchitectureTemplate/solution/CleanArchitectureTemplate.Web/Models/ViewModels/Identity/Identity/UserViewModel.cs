using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CleanArchitectureTemplate.Web.Validations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;

public class UserViewModel
{
    [Required]
    public string Id { get; set; } = default!;

    [Required]
    [Display(Name = "Username")]
    [StringLength(256)]
    public string UserName { get; set; } = default!;

    [Required]
    [Display(Name = "First name")]
    [StringLength(100)]
    public string FirstName { get; set; } = default!;

    [Required]
    [Display(Name = "Last name")]
    [StringLength(100)]
    public string LastName { get; set; } = default!;

    [Display(Name = "Full name")]
    public string? FullName => $"{FirstName} {LastName}";

    [Required]
    [Display(Name = "City")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [StringLength(256)]
    public string Email { get; set; } = default!;

    [Display(Name = "Registration date")]
    public DateOnly? RegistrationDate { get; set; }

    #region booleans

    [Display(Name = "Email confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Phone confirmed")]
    public bool PhoneNumberConfirmed { get; set; }
    public bool HasPhoneNumber => !string.IsNullOrWhiteSpace(PhoneNumber);

    [Display(Name = "Two-factor authentication enabled")]
    public bool TwoFactorEnabled { get; set; }

    [Display(Name = "Lockout enabled")]
    public bool LockoutEnabled { get; set; }

    #endregion

    #region image

    public string? ImagePath { get; set; }

    [AllowedFileExtensions([".jpg", ".jpeg", ".png", ".webp"])]
    [MaxFileSize(2 * 1024 * 1024)] // 2 MB
    public IFormFile? Image { get; set; }

    #endregion

    [Phone]
    [Display(Name = "Phone number")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Lockout end")]
    public DateTimeOffset? LockoutEnd { get; set; }

    [Display(Name = "Failed access attempts")]
    public int AccessFailedCount { get; set; }

    [Display(Name = "Roles")]
    public IList<string> Roles { get; set; } = new List<string>();

    [Display(Name = "Claims")]
    public IList<Claim> Claims { get; set; } = new List<Claim>();
}