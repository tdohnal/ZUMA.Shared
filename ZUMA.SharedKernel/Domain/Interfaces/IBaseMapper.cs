namespace ZUMA.SharedKernel.Domain.Interfaces;

public interface IBaseMapper<TRequest, TEntity, TMessageModel>
{
    TEntity MapToEntity(TRequest request);
    TMessageModel MapToMessage(TEntity entity);
    void UpdateEntity(TRequest request, TEntity entity);
}
