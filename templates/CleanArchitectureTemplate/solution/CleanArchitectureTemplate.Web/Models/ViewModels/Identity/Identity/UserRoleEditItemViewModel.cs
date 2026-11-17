namespace CleanArchitectureTemplate.Web.Models.ViewModels.Identity.Identity;

public class UserRoleEditItemViewModel
{
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    public bool IsAssigned { get; set; }
    public bool IsRequired { get; set; }
}
