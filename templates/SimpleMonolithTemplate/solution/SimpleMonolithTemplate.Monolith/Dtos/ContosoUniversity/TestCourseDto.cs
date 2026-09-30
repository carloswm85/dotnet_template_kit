namespace SimpleMonolithTemplate.Monolith.Dtos.ContosoUniversity;

public class TestCourseDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int Credits { get; set; }

    public ICollection<TestEnrollmentDto>? TestEnrollments { get; set; }
}
