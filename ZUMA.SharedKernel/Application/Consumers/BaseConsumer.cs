using MassTransit;
using Microsoft.Extensions.Logging;
using ZUMA.SharedKernel.Application.Utils;
using ZUMA.SharedKernel.Domain.Interfaces; // Předpokládaný namespace pro IAuditableEntities atd.

namespace ZUMA.SharedKernel.Application.Consumers;

public abstract class BaseConsumer<TRequest, TSuccess, TFailed, TEntity, TMessageContract, TService, TMapper>
    : IConsumer<TRequest>
    where TRequest : class, IRequestEvent
    where TSuccess : class, ISuccessResponse
    where TFailed : class, IFailedResponse
    where TEntity : class, IAuditableEntities
    where TService : IServiceBase<TEntity>
    where TMapper : IBaseMapper<TRequest, TEntity, TMessageContract>
{
    protected readonly ILogger Logger;
    protected readonly TService Service;
    protected readonly TMapper Mapper;

    protected BaseConsumer(ILogger logger, TService service, TMapper mapper)
    {
        Logger = logger;
        Service = service;
        Mapper = mapper;
    }

    public async Task Consume(ConsumeContext<TRequest> context)
    {
        using (Logger.BeginMessageScope(context.MessageId.ToString()!))
        {
            try
            {
                Logger.LogInformation("Consumer {ConsumerName} started processing {RequestType}",
                    GetType().Name, typeof(TRequest).Name);

                await ProcessMessage(context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled error in {ConsumerName} during {RequestType}. MessageId: {MessageId}",
                    GetType().Name, typeof(TRequest).Name, context.MessageId);

                await MapAndRespondFailed(context, ex);

                throw;
            }
        }
    }

    protected virtual async Task ProcessMessage(ConsumeContext<TRequest> context)
    {
        var msg = context.Message;

        switch (msg.Method.Method.ToUpper())
        {
            case "POST":
                var newEntity = Mapper.MapToEntity(msg);
                var created = await Service.CreateAsync(newEntity);
                await RespondOrFailed(context, created);
                break;

            case "GET":
                if (!msg.PublicId.HasValue)
                    throw new NullReferenceException(nameof(msg.PublicId));
                var entity = await Service.GetByPublicIdAsync(msg.PublicId.Value);
                await RespondOrFailed(context, entity);
                break;

            case "PUT":
            case "PATCH":
                if (!msg.PublicId.HasValue)
                    throw new NullReferenceException(nameof(msg.PublicId));
                var existing = await Service.GetByPublicIdAsync(msg.PublicId.Value);
                if (existing != null)
                {
                    Mapper.UpdateEntity(msg, existing);
                    var updated = await Service.UpdateAsync(existing);
                    await RespondOrFailed(context, updated);
                }
                else await RespondOrFailed(context, null);
                break;

            case "DELETE":
                if (!msg.PublicId.HasValue)
                    throw new NullReferenceException(nameof(msg.PublicId));
                var toDelete = await Service.GetByPublicIdAsync(msg.PublicId.Value);

                if (toDelete != null)
                {
                    var success = await Service.DeleteAsync(toDelete.Id);
                    if (success) await context.RespondAsync<TSuccess>(new { });
                    else await RespondOrFailed(context, null);
                }
                else await RespondOrFailed(context, null);
                break;

            default:
                throw new NotImplementedException($"Method {msg.Method} is not supported in BaseConsumer.");
        }
    }

    protected async Task RespondOrFailed(ConsumeContext<TRequest> context, TEntity? entity)
    {
        if (entity == null)
        {
            await context.RespondAsync<TFailed>(new
            {
                ErrorMessage = "Resource not found or operation failed.",
                ErrorCode = "NOT_FOUND"
            });
            return;
        }

        var contract = Mapper.MapToMessage(entity);
        await context.RespondAsync<TSuccess>(new { Data = contract });
    }

    protected virtual async Task MapAndRespondFailed(ConsumeContext<TRequest> context, Exception ex)
    {
        await context.RespondAsync<TFailed>(new
        {
            ErrorMessage = "An internal error occurred.",
            ErrorCode = "INTERNAL_ERROR"
        });
    }
}