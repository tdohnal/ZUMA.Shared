namespace ZUMA.SharedKernel.Domain.Interfaces;

public interface IBaseEntity
{
    long Id { get; set; }

    Guid PublicId { get; set; }
}

public interface IAuditableEntities : IBaseEntity
{
    DateTime Created { get; set; }
    DateTime? Updated { get; set; }
    DateTime? Deleted { get; set; }
}