using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestOfficeAssignment : IEntity
    {
        [NotMapped]
        public int Id => TestInstructorId;

        public object ID => Id;
    }
}
