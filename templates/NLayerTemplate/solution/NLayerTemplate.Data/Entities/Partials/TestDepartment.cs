using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestDepartment : IEntity
    {
        [NotMapped]
        public int Id => TestDepartmentId;

        public object ID => Id;
    }
}
