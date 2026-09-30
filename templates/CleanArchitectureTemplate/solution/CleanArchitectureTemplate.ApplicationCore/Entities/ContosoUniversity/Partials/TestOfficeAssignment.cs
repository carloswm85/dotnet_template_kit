using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestOfficeAssignment : IEntity<int>
{
    [NotMapped]
    public int Id => TestInstructorId;
}
