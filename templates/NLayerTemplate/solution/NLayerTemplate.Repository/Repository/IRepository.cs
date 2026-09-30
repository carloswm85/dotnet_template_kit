namespace NLayerTemplate.Repository
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        // === Query root – exposes IQueryable for complex LINQ queries
        IQueryable<TEntity> Query();

        // === CRUD OPERATIONS ===
        Task<TEntity?> GetByIdAsync(
            object[] keyValues,
            CancellationToken cancellationToken = default
        );

        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default
        );

        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
