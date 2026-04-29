using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ZUMA.SharedKernel.Infrastructure.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddZumaMassTransitGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                ConfigureRabbitMqHost(cfg, configuration);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddZumaMassTransitWorker<T>(this IServiceCollection services, IConfiguration configuration, params Assembly[] consumerAssemblies) where T : DbContext
    {
        services.AddMassTransit(x =>
        {
            if (consumerAssemblies.Length > 0)
            {
                x.AddConsumers(consumerAssemblies);
            }

            x.AddEntityFrameworkOutbox<T>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.DisableInboxCleanupService();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                ConfigureRabbitMqHost(cfg, configuration);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static void ConfigureRabbitMqHost(IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration)
    {
        var rabbitHost = configuration["RABBITMQ:HOST"] ?? throw new NullReferenceException("RABBITMQ:HOST IS NULL");
        var username = configuration["RABBITMQ:USERNAME"] ?? throw new NullReferenceException("RABBITMQ:USERNAME IS NULL");
        var password = configuration["RABBITMQ:PASSWORD"] ?? throw new NullReferenceException("RABBITMQ:PASSWORD IS NULL");

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.UseMessageRetry(r => r.Exponential(3,
                   TimeSpan.FromSeconds(2),
                   TimeSpan.FromSeconds(30),
                   TimeSpan.FromSeconds(5)));

        cfg.UseCircuitBreaker(cb =>
        {
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });
    }
}
