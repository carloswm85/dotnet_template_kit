using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;

public class UserRolesEditViewModel
{
    public string UserId { get; set; } = default!;

    [Display(Name = "Username")]
    public string UserName { get; set; } = default!;

    [Display(Name = "First name")]
    public string FirstName { get; set; } = default!;

    [Display(Name = "Last name")]
    public string LastName { get; set; } = default!;

    [Display(Name = "Full name")]
    public string? FullName => $"{FirstName} {LastName}";

    [Display(Name = "Email")]
    public string? Email { get; set; } = default!;

    public List<UserRoleEditItemViewModel> Roles { get; set; } = new();
}