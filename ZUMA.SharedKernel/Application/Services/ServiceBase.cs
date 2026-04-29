using System.Linq.Expressions;
using ZUMA.SharedKernel.Domain.Entities;
using ZUMA.SharedKernel.Infrastructure.Repositories;

namespace ZUMA.SharedKernel.Application.Services;

public class ServiceBase<T> : IServiceBase<T> where T : IAuditableEntities
{
    protected readonly IRepositoryBase<T> _repository;

    public ServiceBase(IRepositoryBase<T> repository)
    {
        _repository = repository;
    }

    public virtual async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
        => await _repository.ExistsAsync(id, cancellationToken);

    public virtual async Task<bool> ExistsByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
        => await _repository.ExistsByPublicIdAsync(publicId, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetItemsByQueryAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
         => await _repository.GetItemsByQueryAsync(expression, cancellationToken);

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repository.GetByIdAsync(id, cancellationToken);

    public virtual async Task<T?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
        => await _repository.GetByPublicIdAsync(publicId, cancellationToken);

    public virtual async Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllAsync(cancellationToken);

    public virtual async Task<T?> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await BeforeCreateAsync(entity, cancellationToken);
        var result = await _repository.CreateAsync(entity, cancellationToken);
        await AfterCreateAsync(result!, cancellationToken);
        return result;
    }

    public virtual async Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await BeforeUpdateAsync(entity, cancellationToken);
        var result = await _repository.UpdateAsync(entity, cancellationToken);
        await AfterUpdateAsync(result!, cancellationToken);
        return result;
    }

    public virtual async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await BeforeDeleteAsync(id, cancellationToken);
        var result = await _repository.DeleteAsync(id, cancellationToken);
        await AfterDeleteAsync(id, cancellationToken);
        return result;
    }

    #region Hooks (virtual)

    protected virtual Task BeforeCreateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.Created = DateTime.UtcNow;
        return Task.CompletedTask;
    }
    protected virtual Task AfterCreateAsync(T entity, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task BeforeUpdateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.Updated = DateTime.UtcNow;
        return Task.CompletedTask;
    }
    protected virtual Task AfterUpdateAsync(T entity, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task BeforeDeleteAsync(long id, CancellationToken cancellationToken) => Task.CompletedTask;
    protected virtual Task AfterDeleteAsync(long id, CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion
}
