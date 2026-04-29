using ZUMA.SharedKernel.Domain.Interfaces;
using ZUMA.SharedKernel.Domain.MessagingContracts.Base;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Contracts.Authorization;

public record SendAuthorizeUserRequest : BaseRequestEvent
{
    public string Email { get; set; } = null!;

}

public record AuthorizeUserSuccess : ISuccessResponse
{
    public DateTime SentAt { get; set; }
}

public record AuthorizeUserFailed : IFailedResponse
{
}
