using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;

public class RoleViewModel
{
    public string? Id { get; set; }

    [Required]
    [StringLength(256)]
    [Display(Name = "Role name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Public role")]
    public bool IsPublic { get; set; }

    [Display(Name = "Default role")]
    public bool IsDefault { get; set; }

    [Display(Name = "Static role")]
    public bool IsStatic { get; set; }
}
