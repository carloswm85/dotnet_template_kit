using CleanArchitectureTemplate.ApplicationCore.Dtos.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Entities;

namespace CleanArchitectureTemplate.ApplicationCore.Interfaces.ContosoUniversity;

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
    Task<int> CreateTestStudentAsync(TestStudentDto studentDto, CancellationToken cancellationToken = default);
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

    #region TestContact

    Task<IEnumerable<TestContact>> GetTestContactsAsync(
        bool isAuthorized,
        string currentUserId,
        CancellationToken cancellationToken = default
    );
    Task<TestContact?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TestContact?> GetByIdAsNoTrackingAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(TestContact contact, CancellationToken cancellationToken = default);
    Task UpdateAsync(TestContact contact, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(
        int id,
        TestContactStatus status,
        CancellationToken cancellationToken = default
    );
    Task DeleteAsync(TestContact contact, CancellationToken cancellationToken = default);

    #endregion
}
