namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;

// DELETE?
public class UserRoleViewModel
{
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    public bool Selected { get; set; }
}
