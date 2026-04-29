using ZUMA.SharedKernel.Domain.MessagingContracts.Base;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Events;

public class CreateEmailEvent : IEvent
{
    public required Guid UserId { get; set; }
    public string Code { get; set; }
    public required string FullName { get; set; }
    public required string Subject { get; set; }
    public string Body { get; set; }
    public required string Email { get; set; }

    public required EmailTemplateType EmailTemplateType { get; set; }
}
