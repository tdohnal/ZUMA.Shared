using ZUMA.SharedKernel.Messagges.Base;

namespace ZUMA.SharedKernel.Messagges.Events;

public class FireEmailEvent : IEvent
{
    public Guid EmailId { get; set; }
    public string Email { get; set; }
}
