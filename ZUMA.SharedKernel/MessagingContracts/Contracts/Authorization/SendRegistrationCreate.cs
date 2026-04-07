using ZUMA.SharedKernel.Messagges.Base;

namespace ZUMA.SharedKernel.Messagges.Contracts.Authorization;

// Místo interface použij record
public record SendRegistrationCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string Username
) : IRequestEvent;

public class RegistrateSuccess : ISuccessResponse
{

}
public class RegistrateFailed : IFailedResponse
{
}