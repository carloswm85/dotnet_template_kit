using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CleanArchitectureTemplate.ApplicationCore.Dtos.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Entities;
using CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.ContosoUniversity;
using CleanArchitectureTemplate.Infrastructure.Model;

namespace CleanArchitectureTemplate.Infrastructure.Services.ContosoServices;

public class ContosoUniversityService : IContosoUniversityService
{
    private readonly ILogger<ContosoUniversityService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public ContosoUniversityService(
        ILogger<ContosoUniversityService> logger,
        IMapper mapper,
        ApplicationDbContext context
    )
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = context;
    }

    #region TestStudent

    public async Task<TestStudentDto?> GetTestStudentAsync(
        int studentId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        TestStudent? student;

        if (asNoTracking)
        {
            student = await _dbContext.TestStudents.FindAsync([studentId], cancellationToken);

            if (student == null)
            {
                _logger.LogWarning("TestStudent with ID {TestStudentId} not found.", studentId);
                return null;
            }

            return _mapper.Map<TestStudentDto>(student);
        }

        student = await _dbContext
            .TestStudents.Include(s => s.TestEnrollments)
                .ThenInclude(e => e.TestCourse)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.TestStudentId == studentId, cancellationToken);

        if (student == null)
        {
            _logger.LogWarning("TestStudent with ID {TestStudentId} not found.", studentId);
            return null;
        }

        return _mapper.Map<TestStudentDto>(student);
    }

    public async Task<IEnumerable<TestStudentDto>> GetTestStudentListAsync(
        CancellationToken cancellationToken = default
    )
    {
        var students = await _dbContext.TestStudents.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TestStudentDto>>(students);
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

            await _dbContext.TestStudents.AddAsync(student, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
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

        student.GovernmentId = new string(student.GovernmentId.Where(char.IsDigit).ToArray());

        _dbContext.TestStudents.Update(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public bool TestStudentExists(int studentId)
    {
        return _dbContext.TestStudents.Any(s => s.TestStudentId == studentId);
    }

    public bool TestStudentExists(string governmentId)
    {
        return _dbContext.TestStudents.Any(s => s.GovernmentId.Equals(governmentId));
    }

    public async Task<bool> DeleteTestStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default
    )
    {
        if (studentId <= 0)
            return false;

        var student = await _dbContext.TestStudents.FindAsync([studentId], cancellationToken);

        if (student == null)
            return false;

        _dbContext.TestStudents.Remove(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<TestEnrollmentDateGroupDto>> GetTestEnrollmentDateDataAsync(
        CancellationToken cancellationToken = default
    )
    {
        var students = _dbContext.TestStudents.AsQueryable();

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

    #endregion

    #region TestContact

    public async Task<IEnumerable<TestContact>> GetTestContactsAsync(
        bool isAuthorized,
        string currentUserId,
        CancellationToken cancellationToken = default
    )
    {
        var contacts = _dbContext.TestContact.AsQueryable();

        if (!isAuthorized)
        {
            contacts = contacts.Where(c =>
                c.Status == TestContactStatus.Approved || c.OwnerID == currentUserId
            );
        }

        return await contacts.ToListAsync(cancellationToken);
    }

    public async Task<TestContact?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TestContact.FirstOrDefaultAsync(
            c => c.TestContactId == id,
            cancellationToken
        );
    }

    public async Task<TestContact?> GetByIdAsNoTrackingAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.TestContact.AsNoTracking().FirstOrDefaultAsync(
            c => c.TestContactId == id,
            cancellationToken
        );
    }

    public async Task CreateAsync(TestContact contact, CancellationToken cancellationToken = default)
    {
        _dbContext.TestContact.Add(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TestContact contact, CancellationToken cancellationToken = default)
    {
        _dbContext.Attach(contact).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        int id,
        TestContactStatus status,
        CancellationToken cancellationToken = default
    )
    {
        var contact = await GetByIdAsync(id, cancellationToken);

        if (contact == null)
            return;

        contact.Status = status;
        _dbContext.TestContact.Update(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TestContact contact, CancellationToken cancellationToken = default)
    {
        _dbContext.TestContact.Remove(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
