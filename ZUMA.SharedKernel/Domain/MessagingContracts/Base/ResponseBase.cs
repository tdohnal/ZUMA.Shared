using ZUMA.SharedKernel.Domain.Interfaces;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Base;

public record SuccessResponseBase : ISuccessResponse
{
}

public record FailedResponseBase : IFailedResponse
{
    public string ErrorMessage { get; set; } = string.Empty;

}
