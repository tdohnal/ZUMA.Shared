using ZUMA.SharedKernel.MessagingContracts.Base;

namespace ZUMA.SharedKernel.MessagingContracts.Events;

public class FireEmailEvent : IEvent
{
    public Guid EmailId { get; set; }
    public string Email { get; set; }
}
