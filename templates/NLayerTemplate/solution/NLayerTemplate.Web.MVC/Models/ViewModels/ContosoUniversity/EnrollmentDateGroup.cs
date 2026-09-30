using System.ComponentModel.DataAnnotations;

namespace NLayerTemplate.Web.MVC.Models.ViewModels.TestStudent
{
    public class TestEnrollmentDateGroup
    {
        [DataType(DataType.Date)]
        public DateTime? TestEnrollmentDate { get; set; }

        public int TestStudentCount { get; set; }
    }
}
