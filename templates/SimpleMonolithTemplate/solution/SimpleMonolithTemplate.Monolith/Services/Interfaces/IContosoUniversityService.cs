using SimpleMonolithTemplate.Monolith.Dtos.ContosoUniversity;
using SimpleMonolithTemplate.Monolith.Models;

namespace SimpleMonolithTemplate.Monolith.Services.Interfaces.ExampleInterfaces;

public interface IContosoUniversityService
{
    #region TestStudent

    Task<bool> DeleteTestStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<TestStudentDto?> GetTestStudentAsync(
        int studentId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<TestStudentDto>> GetTestStudentListAsync(
        CancellationToken cancellationToken = default
    );
    Task<PaginatedList<TestStudentDto>> GetTestStudentsPaginatedListAsync(
        string currentFilter,
        int pageIndex,
        int pageSize,
        string searchString,
        string sortOrder,
        CancellationToken cancellationToken = default
    );
    Task<int> CreateTestStudentAsync(
        TestStudentDto studentDto,
        CancellationToken cancellationToken = default
    );
    bool TestStudentExists(int studentId);
    bool TestStudentExists(string governmentId);
    Task<bool> UpdateTestStudentAsync(
        int studentId,
        TestStudentDto studentDto,
        CancellationToken cancellationToken = default
    );
    Task<List<TestEnrollmentDateGroupDto>> GetTestEnrollmentDateDataAsync(
        CancellationToken cancellationToken = default
    );

    #endregion
}
