using CleanArchitectureTemplate.ApplicationCore.Common.Pagination;
using CleanArchitectureTemplate.ApplicationCore.Dtos.ContosoUniversity;

namespace CleanArchitectureTemplate.ApplicationCore.Interfaces;

public interface IPaginationService
{
    Task<PaginatedList<TestStudentDto>> GetTestStudentsPaginatedListAsync(
        string currentFilter,
        int pageIndex,
        int pageSize,
        string searchString,
        string sortOrder
    );
}
