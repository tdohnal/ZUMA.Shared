using ZUMA.SharedKernel.Domain.Interfaces;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Events;

public class FireEmailEvent : IEvent
{
    public Guid EmailId { get; set; }
    public string Email { get; set; }
}
