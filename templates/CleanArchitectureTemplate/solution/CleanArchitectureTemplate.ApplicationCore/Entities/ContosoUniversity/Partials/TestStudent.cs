using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestStudent : IEntity<int>
{
    [NotMapped]
    public int Id => TestStudentId;
}
