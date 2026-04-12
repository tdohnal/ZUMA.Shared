using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ZUMA.SharedKernel.Entities;

namespace ZUMA.SharedKernel.Repositories;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class, IAuditableEntities
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    public RepositoryBase(DbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
        _logger = logger;
    }

    public virtual async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {

        _logger.LogInformation("Checking existence of entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
        return await _dbSet.AnyAsync(x => x.Id == id && !x.Deleted.HasValue, cancellationToken);
    }

    public virtual async Task<bool> ExistsByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking existence of entity of type {EntityType} with PublicId {publicId}", typeof(T).Name, publicId);
        return await _dbSet.AnyAsync(x => x.PublicId == publicId && !x.Deleted.HasValue, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetItemsByQueryAsync(
      Expression<Func<T, bool>> expression,
      CancellationToken cancellationToken = default)
    {
        var items = await _dbSet
            .AsNoTracking()
            .Where(expression)
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }

    /// <summary>
    /// Virtuální metoda pro konfiguraci Include. 
    /// V základu neprovádí žádné Joiny.
    /// </summary>
    protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query)
    {
        return query;
    }

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
        return await ApplyIncludes(_dbSet).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task<T?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting entity of type {EntityType} with ID {EntityId}", typeof(T).Name, publicId);
        return await ApplyIncludes(_dbSet).SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public virtual async Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all entities of type {EntityType}", typeof(T).Name);
        return await ApplyIncludes(_dbSet).Where(x => !x.Deleted.HasValue).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating a new entity of type {EntityType}", typeof(T).Name);
        await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating entity of type {EntityType} with ID {EntityId}", typeof(T).Name, entity.Id);
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        entity.Deleted = DateTime.UtcNow;
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).Property(x => x.Deleted).IsModified = true;

        var affected = await _dbContext.SaveChangesAsync(cancellationToken);
        return affected > 0;
    }
}
