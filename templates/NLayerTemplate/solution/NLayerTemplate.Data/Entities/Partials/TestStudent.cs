using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestStudent : IEntity
    {
        [NotMapped]
        public int Id => TestStudentId;

        public object ID => Id;
    }
}
