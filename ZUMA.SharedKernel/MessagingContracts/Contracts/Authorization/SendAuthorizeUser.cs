using ZUMA.SharedKernel.MessagingContracts.Base;

namespace ZUMA.SharedKernel.MessagingContracts.Contracts.Authorization;

public class SendAuthorizeUserRequest : IRequestEvent
{
    public string Email { get; set; } = null!;
}

public class AuthorizeUserSuccess : ISuccessResponse
{
    public DateTime SentAt { get; set; }
}

public class AuthorizeUserFailed : IFailedResponse
{
}
