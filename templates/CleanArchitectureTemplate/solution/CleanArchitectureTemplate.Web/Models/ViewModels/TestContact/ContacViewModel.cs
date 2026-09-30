using System.ComponentModel.DataAnnotations;
using CleanArchitectureTemplate.ApplicationCore.Entities;
using TestContactEntity = CleanArchitectureTemplate.ApplicationCore.Entities.TestContact;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.TestContact;

public class TestContactViewModel
{
    public int TestContactId { get; set; }

    public string? OwnerID { get; set; } // user ID from AspNetUser table.

    [Required]
    [StringLength(100, ErrorMessage = "Name must be 100 characters or fewer")]
    public string? Name { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Address must be 200 characters or fewer")]
    public string? Address { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "City must be 100 characters or fewer")]
    public string? City { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "State must be 50 characters or fewer")]
    public string? State { get; set; }

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP format")]
    public string? Zip { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Required]
    public TestContactStatus Status { get; set; }

    public TestContactEntity? TestContact { get; internal set; }
}
