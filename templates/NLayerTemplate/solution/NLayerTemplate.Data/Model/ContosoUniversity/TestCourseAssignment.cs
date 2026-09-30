namespace NLayerTemplate.Data.Model.ContosoUniversity
{
    /// <summary>
    /// `TestCourseTestInstructor` relationship class. Join table for the
    /// TestInstructor-to-TestCourses many-to-many relationship,
    /// </summary>
    public class TestCourseAssignment
    {
        public int TestInstructorId { get; set; }
        public int TestCourseId { get; set; }
        public TestInstructor TestInstructor { get; set; } = default!;
        public TestCourse TestCourse { get; set; } = default!;
    }
}
