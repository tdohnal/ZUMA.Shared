using ZUMA.SharedKernel.Domain.Interfaces;
using ZUMA.SharedKernel.Domain.MessagingContracts.Base;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Contracts.Authorization;

// Místo interface použij record
public record SendRegistrationCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string Username
) : BaseRequestEvent;

public class RegistrateSuccess : ISuccessResponse
{

}
public class RegistrateFailed : IFailedResponse
{
}