using NLayerTemplate.Data.Model;

namespace NLayerTemplate.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        // === Persist all changes (SaveChanges)
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // === EF transaction helpers
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        #region Contoso University Example

        IRepository<TestStudent> TestStudentRepository { get; }
        IRepository<TestCourse> TestCourseRepository { get; }
        IRepository<TestEnrollment> TestEnrollmentRepository { get; }

        #endregion
    }
}
