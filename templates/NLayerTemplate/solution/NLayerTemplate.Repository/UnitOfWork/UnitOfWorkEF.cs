using NLayerTemplate.Data.Model;
using Microsoft.EntityFrameworkCore.Storage;

namespace NLayerTemplate.Repository
{
    public sealed class UnitOfWorkEF : IDisposable, IUnitOfWork
    {
        #region Private Fields

        private readonly NLayerTemplateDbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;

        private IRepository<TestStudent>? _studentRepository;
        private IRepository<TestCourse>? _courseRepository;
        private IRepository<TestEnrollment>? _enrollmentRepository;

        #endregion

        public UnitOfWorkEF(NLayerTemplateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Contoso University Example

        public IRepository<TestStudent> TestStudentRepository =>
            _studentRepository ?? (_studentRepository = new RepositoryEF<TestStudent>(_dbContext));
        public IRepository<TestCourse> TestCourseRepository =>
            _courseRepository ?? (_courseRepository = new RepositoryEF<TestCourse>(_dbContext));
        public IRepository<TestEnrollment> TestEnrollmentRepository =>
            _enrollmentRepository
            ?? (_enrollmentRepository = new RepositoryEF<TestEnrollment>(_dbContext));

        #endregion


        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await _dbContext.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                return;

            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                return;

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public void Dispose() => _dbContext.Dispose();
    }
}
