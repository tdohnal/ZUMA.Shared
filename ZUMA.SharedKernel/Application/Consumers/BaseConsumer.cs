using MassTransit;
using Microsoft.Extensions.Logging;
using ZUMA.SharedKernel.Application.Utils;

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
        using (_logger.BeginMessageScope(context.MessageId.ToString()!, identificationData: context))
        {
            try
            {
                await OnConsume(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed");
                throw;
            }
        }
    }

    protected abstract Task OnConsume(ConsumeContext<TRequest> context);

}