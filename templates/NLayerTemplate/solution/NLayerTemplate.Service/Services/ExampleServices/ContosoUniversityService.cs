using NLayerTemplate.Data.Model;
using NLayerTemplate.Repository;
using NLayerTemplate.Service.Dtos.ContosoUniversity;
using NLayerTemplate.Service.Models;
using NLayerTemplate.Service.Services.ExampleServices.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NLayerTemplate.Service.Services.ExampleServices
{
    public class ContosoUniversityService : IContosoUniversityService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ContosoUniversityService> _logger;
        private readonly IMapper _mapper;

        public ContosoUniversityService(
            ILogger<ContosoUniversityService> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _logger = logger;
            _uow = unitOfWork;
            _mapper = mapper;
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
                student = await _uow.TestStudentRepository.GetByIdAsync(
                    [studentId],
                    cancellationToken
                );

                if (student is null)
                {
                    return null;
                }

                return _mapper.Map<TestStudentDto>(student);
            }

            student = await _uow
                .TestStudentRepository.Query()
                .Include(s => s.TestEnrollments)
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
            var students = await _uow.TestStudentRepository.GetAllAsync(cancellationToken);
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
            var students = await _uow.TestStudentRepository.GetAllAsync(cancellationToken);
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

        public async Task<int> CreateTestStudentAsync(
            TestStudentDto studentDto,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var student = _mapper.Map<TestStudent>(studentDto);

                student.GovernmentId = new string(
                    student.GovernmentId.Where(char.IsDigit).ToArray()
                );

                await _uow.TestStudentRepository.AddAsync(student, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
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

            _uow.TestStudentRepository.Update(student);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        public bool TestStudentExists(int studentId)
        {
            return _uow.TestStudentRepository.Query().Any(s => s.TestStudentId == studentId);
        }

        public bool TestStudentExists(string governmentId)
        {
            return _uow.TestStudentRepository.Query().Any(s => s.GovernmentId.Equals(governmentId));
        }

        public async Task<bool> DeleteTestStudentAsync(
            int studentId,
            CancellationToken cancellationToken = default
        )
        {
            if (studentId <= 0)
                return false;

            var student = await _uow.TestStudentRepository.GetByIdAsync(
                [studentId],
                cancellationToken
            );

            if (student == null)
                return false;

            _uow.TestStudentRepository.Remove(student);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<TestEnrollmentDateGroupDto>> GetTestEnrollmentDateDataAsync(
            CancellationToken cancellationToken = default
        )
        {
            var students = _uow.TestStudentRepository.Query();

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
}
