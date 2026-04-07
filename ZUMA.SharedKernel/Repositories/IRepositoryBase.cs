using System.Linq.Expressions;
using ZUMA.SharedKernel.Entities;

namespace ZUMA.SharedKernel.Repositories;

public interface IRepositoryBase<T> where T : IAuditableEntities
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<T?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetItemsByQueryAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
}