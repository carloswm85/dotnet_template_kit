using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestCourse : IEntity
    {
        [NotMapped]
        public int Id => TestCourseId;

        public object ID => Id;
    }
}
