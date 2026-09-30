using NLayerTemplate.Data;
using NLayerTemplate.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace NLayerTemplate.Repository
{
    //The reading is: It is a class, that takes a generic type, and implements an interface, AND this generic type is limited to be any class + entities interface.
    public class RepositoryEF<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity
    {
        #region Private fields

        private readonly NLayerTemplateDbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        #endregion

        #region Constructors

        public RepositoryEF(NLayerTemplateDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        #endregion

        #region Public Methods

        public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

        public async Task<TEntity?> GetByIdAsync(
            object[] keyValues,
            CancellationToken cancellationToken = default
        ) => await _dbSet.FindAsync(keyValues, cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default
        ) => await _dbSet.ToListAsync(cancellationToken);

        public async Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        ) => await _dbSet.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default
        ) => await _dbSet.AddRangeAsync(entities, cancellationToken);

        public void Update(TEntity entity) => _dbSet.Update(entity);

        public void UpdateRange(IEnumerable<TEntity> entities) => _dbSet.UpdateRange(entities);

        public void Remove(TEntity entity) => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);

        #endregion
    }
}
