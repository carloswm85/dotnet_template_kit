using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;

public partial class TestEnrollment : IEntity<int>
{
    [NotMapped]
    public int Id => TestEnrollmentId;
}
