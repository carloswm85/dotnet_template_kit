using System.ComponentModel.DataAnnotations;
using NLayerTemplate.Data.Constants;

namespace NLayerTemplate.Data.Model
{
    /// <summary>
    /// There's a many-to-many relationship between the TestStudent and TestCourse
    /// entities,and the TestEnrollment entity functions as a many-to-many join
    /// table with payload in the database. "With payload" means that the
    /// TestEnrollment table contains additional data besides foreign keys for
    /// the joined tables (in this case, a primary key and a Grade property).
    /// </summary>
    public partial class TestEnrollment
    {
        [Key]
        public int TestEnrollmentId { get; set; }
        public int TestCourseId { get; set; }
        public int TestStudentId { get; set; }

        [DisplayFormat(NullDisplayText = "No grade")]
        public Grade? Grade { get; set; }

        public TestCourse TestCourse { get; set; } = default!;
        public TestStudent TestStudent { get; set; } = default!;
    }
}
