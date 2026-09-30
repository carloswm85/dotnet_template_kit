using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.ContosoUniversity;

public class TestEnrollmentDateGroup
{
    [DataType(DataType.Date)]
    public DateTime? TestEnrollmentDate { get; set; }

    public int TestStudentCount { get; set; }
}
