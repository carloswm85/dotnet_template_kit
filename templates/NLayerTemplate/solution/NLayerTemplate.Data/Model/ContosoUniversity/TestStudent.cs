using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerTemplate.Data.Model
{
    public partial class TestStudent
    {
        [Key]
        public int TestStudentId { get; set; }
        public string GovernmentId { get; set; } = string.Empty;

        [Column("FirstName")]
        public string FirstMidName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [NotMapped] // Explicitly mark as not mapped to DB
        public string FullName => $"{LastName}, {FirstMidName}";

        public string? ImagePath { get; set; }

        public DateOnly TestEnrollmentDate { get; set; }

        public List<TestEnrollment> TestEnrollments { get; set; } = [];
    }
}
