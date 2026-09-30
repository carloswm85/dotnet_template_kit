using CleanArchitectureTemplate.ApplicationCore.Constants;

namespace CleanArchitectureTemplate.ApplicationCore.Dtos.ContosoUniversity;

public class TestEnrollmentDto
{
    public int Id { get; set; }
    public int? TestCourseId { get; set; }
    public int? TestStudentId { get; set; }
    public Grade? Grade { get; set; }
    public TestCourseDto? TestCourse { get; set; }
    public TestStudentDto? TestStudent { get; set; }
}
