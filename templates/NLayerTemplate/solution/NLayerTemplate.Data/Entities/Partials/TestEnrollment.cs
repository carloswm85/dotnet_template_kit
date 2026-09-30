using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestEnrollment : IEntity
    {
        [NotMapped]
        public int Id => TestEnrollmentId;

        public object ID => Id;
    }
}
