using ZUMA.SharedKernel.Domain.MessagingContracts.Base;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Contracts.Authorization;

// Místo interface použij record
public record SendRegistrationCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string Username
) : BaseRequestEvent;

public record RegistrateSuccess : SuccessResponseBase
{

}
public record RegistrateFailed : FailedResponseBase
{
}