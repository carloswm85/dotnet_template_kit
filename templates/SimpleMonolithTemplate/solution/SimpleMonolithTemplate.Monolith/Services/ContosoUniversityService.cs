using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SimpleMonolithTemplate.Monolith.Data.Entities.ContosoUniversity;
using SimpleMonolithTemplate.Monolith.Data.Persistence;
using SimpleMonolithTemplate.Monolith.Dtos.ContosoUniversity;
using SimpleMonolithTemplate.Monolith.Models;
using SimpleMonolithTemplate.Monolith.Services.Interfaces.ExampleInterfaces;

namespace SimpleMonolithTemplate.Monolith.Services.ExampleServices;

public class ContosoUniversityService : IContosoUniversityService
{
    private readonly ILogger<ContosoUniversityService> _logger;
    private readonly SimpleMonolithTemplateDbContext _context;
    private readonly IMapper _mapper;

    public ContosoUniversityService(
        ILogger<ContosoUniversityService> logger,
        IMapper mapper,
        SimpleMonolithTemplateDbContext context
    )
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
    }

    public async Task<TestStudentDto?> GetTestStudentAsync(
        int studentId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        TestStudent? student;

        if (asNoTracking)
        {
            student = await _context.TestStudents.FindAsync([studentId], cancellationToken);

            if (student is null)
            {
                return null;
            }

            return _mapper.Map<TestStudentDto>(student);
        }

        student = await _context
            .TestStudents.Include(s => s.TestEnrollments)
                .ThenInclude(e => e.TestCourse)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.TestStudentId == studentId, cancellationToken);

        if (student is null)
        {
            return null;
        }

        return _mapper.Map<TestStudentDto>(student);
    }

    public async Task<IEnumerable<TestStudentDto>> GetTestStudentListAsync(
        CancellationToken cancellationToken = default
    )
    {
        var students = await _context.TestStudents.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TestStudentDto>>(students);
    }

    public async Task<PaginatedList<TestStudentDto>> GetTestStudentsPaginatedListAsync(
        string currentFilter,
        int pageIndex,
        int pageSize,
        string searchString,
        string sortOrder,
        CancellationToken cancellationToken = default
    )
    {
        var students = _context.TestStudents.AsQueryable();
        var totalRecords = await students.CountAsync(cancellationToken);

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
        var filteredCount = await students.CountAsync(cancellationToken);

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

        var count = filteredCount;
        var items = await students
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

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

    public async Task<int> CreateTestStudentAsync(
        TestStudentDto studentDto,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var student = _mapper.Map<TestStudent>(studentDto);

            student.GovernmentId = new string(student.GovernmentId.Where(char.IsDigit).ToArray());

            await _context.TestStudents.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return student.TestStudentId;
        }
        catch (DbUpdateException dbuex)
        {
            _logger.LogError(dbuex, "An error occurred while saving the student.");
            throw;
        }
    }

    public async Task<bool> UpdateTestStudentAsync(
        int studentId,
        TestStudentDto studentDto,
        CancellationToken cancellationToken = default
    )
    {
        if (studentId <= 0 || studentDto == null)
            return false;

        studentDto.Id = studentId;
        var student = _mapper.Map<TestStudent>(studentDto);
        student.TestStudentId = studentId;

        student.GovernmentId = new string(student.GovernmentId.Where(char.IsDigit).ToArray());

        _context.TestStudents.Update(student);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public bool TestStudentExists(int studentId)
    {
        return _context.TestStudents.Any(s => s.TestStudentId == studentId);
    }

    public bool TestStudentExists(string governmentId)
    {
        return _context.TestStudents.Any(s => s.GovernmentId.Equals(governmentId));
    }

    public async Task<bool> DeleteTestStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default
    )
    {
        if (studentId <= 0)
            return false;

        var student = await _context.TestStudents.FindAsync([studentId], cancellationToken);

        if (student == null)
            return false;

        _context.TestStudents.Remove(student);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<TestEnrollmentDateGroupDto>> GetTestEnrollmentDateDataAsync(
        CancellationToken cancellationToken = default
    )
    {
        var students = _context.TestStudents.AsQueryable();

        IQueryable<TestEnrollmentDateGroupDto> data =
            from student in students
            group student by student.TestEnrollmentDate.Year into dateGroup
            select new TestEnrollmentDateGroupDto()
            {
                TestEnrollmentYear = dateGroup.Key,
                TestStudentCount = dateGroup.Count(),
            };

        return await data.ToListAsync(cancellationToken);
    }
}
