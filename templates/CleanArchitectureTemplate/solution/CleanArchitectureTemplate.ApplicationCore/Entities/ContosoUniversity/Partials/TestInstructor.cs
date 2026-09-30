using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestInstructor : IEntity<int>
{
    [NotMapped]
    public int Id => TestInstructorId;
}
