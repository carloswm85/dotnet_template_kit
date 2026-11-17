using System.ComponentModel.DataAnnotations;
using CleanArchitectureTemplate.Web.Validations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Account;

public class UserProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(
        80,
        MinimumLength = 2,
        ErrorMessage = "First name must be between 2 and 80 characters."
    )]
    [RegularExpression(
        @"^\p{L}[\p{L}\s'-]*$",
        ErrorMessage = "First name must start with a letter and contain only letters, "
            + "spaces, hyphens and apostrophes."
    )]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(
        80,
        MinimumLength = 2,
        ErrorMessage = "Last name must be between 2 and 80 characters."
    )]
    [RegularExpression(
        @"^\p{L}[\p{L}\s'-]*$",
        ErrorMessage = "Last name must start with a letter and contain only letters, "
            + "spaces, hyphens and apostrophes."
    )]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [Display(Name = "Full name")]
    public string FullName => $"{LastName}, {FirstName}";

    public string? Email { get; set; }

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }

    public string? ImagePath { get; set; }

    [Display(Name = "Image")]
    [AllowedFileExtensions([".jpg", ".jpeg", ".png", ".webp"])]
    [MaxFileSize(2 * 1024 * 1024)] // 2 MB
    public IFormFile? Image { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Registration date")]
    public DateOnly? RegistrationDate { get; set; }
}