using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestInstructor
{
    [Key]
    public int TestInstructorId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Column("FirstName")]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstMidName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }

    [Display(Name = "Full Name")]
    public string FullName
    {
        get { return LastName + ", " + FirstMidName; }
    }

    /* If a navigation property can hold multiple entities, its type must
     * be a list in which entries can be added, deleted, and updated. You
     * can specify ICollection<T> or a type such as List<T> or HashSet<T>.
     * If you specify ICollection<T>, EF creates a HashSet<T> collection
     * by default.
     */
    public ICollection<TestCourseAssignment> TestCourseAssignments { get; set; } = [];
    public TestOfficeAssignment? TestOfficeAssignment { get; set; }
}
