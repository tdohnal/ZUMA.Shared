using ZUMA.SharedKernel.Domain.MessagingContracts.Base;

public interface IMessageService
{
    Task<BusinessResult<TSuccess, TFailure>> SendAsync<TRequest, TSuccess, TFailure>(
        TRequest message,
        CancellationToken ct = default)
        where TRequest : class, IRequestEvent
        where TSuccess : class, ISuccessResponse
        where TFailure : class, IFailedResponse;
}