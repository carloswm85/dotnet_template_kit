using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestInstructor : IEntity
    {
        [NotMapped]
        public int Id => TestInstructorId;

        public object ID => Id;
    }
}
