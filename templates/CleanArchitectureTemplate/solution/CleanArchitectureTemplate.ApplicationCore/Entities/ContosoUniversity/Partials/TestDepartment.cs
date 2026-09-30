using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestDepartment : IEntity<int>
{
    [NotMapped]
    public int Id => TestDepartmentId;
}
