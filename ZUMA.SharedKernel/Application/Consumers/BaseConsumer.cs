using MassTransit;
using Microsoft.Extensions.Logging;
using ZUMA.SharedKernel.Application.Utils;
using ZUMA.SharedKernel.Domain.MessagingContracts.Base;

public abstract class BaseConsumer<TRequest> : IConsumer<TRequest>
    where TRequest : class
{
    protected readonly ILogger _logger;

    protected BaseConsumer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TRequest> context)
    {
        using (_logger.BeginMessageScope(context.MessageId.ToString()!))
        {
            try
            {
                ArgumentNullException.ThrowIfNull(context);

                await OnConsumeAsync(context);
            }
            catch (Exception ex)
            {
                await OnFailedAsync<FailedResponseBase>(context, ex);
                throw;
            }
        }
    }

    protected abstract Task OnConsumeAsync(ConsumeContext<TRequest> context);

    protected virtual async Task OnFailedAsync<TFailedResponse>(ConsumeContext<TRequest> context, Exception ex) where TFailedResponse : FailedResponseBase
    {
        _logger.LogError(ex, "Processing failed for {RequestType}", typeof(TRequest).Name);
        await context.RespondAsync<TFailedResponse>(new
        {
            ErrorMessage = $"INTERNAL_ERROR: {ex.Message}",
            ErrorCode = "500"
        });
    }
}