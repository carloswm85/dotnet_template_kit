using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestCourse : IEntity<int>
{
    [NotMapped]
    public int Id => TestCourseId;
}
