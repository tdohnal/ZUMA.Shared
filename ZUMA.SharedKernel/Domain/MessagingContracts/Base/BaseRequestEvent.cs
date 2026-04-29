using ZUMA.SharedKernel.Domain.Interfaces;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Base;

public record class BaseRequestEvent : IRequestEvent
{
    public Guid? PublicId { get; set; } = null!;
    public required HttpMethod Method { get; set; }
}
