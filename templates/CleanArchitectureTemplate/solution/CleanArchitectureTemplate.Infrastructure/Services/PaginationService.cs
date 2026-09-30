using MapsterMapper;
using Microsoft.Extensions.Logging;
using CleanArchitectureTemplate.ApplicationCore.Common.Pagination;
using CleanArchitectureTemplate.ApplicationCore.Dtos.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using CleanArchitectureTemplate.Infrastructure.Model;

namespace CleanArchitectureTemplate.Infrastructure.Services;

public class PaginationService : IPaginationService
{
    private readonly ILogger<PaginationService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public PaginationService(
        ILogger<PaginationService> logger,
        IMapper mapper,
        ApplicationDbContext context
    )
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
    }

    public async Task<PaginatedList<TestStudentDto>> GetTestStudentsPaginatedListAsync(
        string currentFilter,
        int pageIndex,
        int pageSize,
        string searchString,
        string sortOrder
    )
    {
        var students = _context.TestStudents.AsQueryable();
        var totalRecords = students.Count();

        // PAGING
        if (searchString != currentFilter)
            pageIndex = 1;
        else
            searchString = currentFilter;

        // SEARCH
        if (!string.IsNullOrEmpty(searchString))
        {
            var term = searchString.Trim().ToUpper();

            students = students.Where(s =>
                s.LastName.ToUpper().Contains(term) || s.FirstMidName.ToUpper().Contains(term)
            );
        }
        var filteredCount = students.Count();

        // SORTING
        switch (sortOrder)
        {
            case CurrentSort.LastNameDesc:
                students = students.OrderByDescending(s => s.LastName);
                break;
            case CurrentSort.DateAsc:
                students = students.OrderBy(s => s.TestEnrollmentDate);
                break;
            case CurrentSort.DateDesc:
                students = students.OrderByDescending(s => s.TestEnrollmentDate);
                break;
            default:
                students = students.OrderBy(s => s.LastName);
                break;
        }

        var count = students.Count();
        var items = students.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        var studentsDto = _mapper.Map<List<TestStudentDto>>(items);

        return new PaginatedList<TestStudentDto>(
            items: studentsDto,
            count: count,
            pageIndex: pageIndex,
            pageSize: pageSize,
            totalRecords: totalRecords,
            filteredCount: filteredCount
        );
    }
}
