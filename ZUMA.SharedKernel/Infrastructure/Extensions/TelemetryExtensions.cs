using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ZUMA.SharedKernel.Infrastructure.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddZumaTelemetry(this IServiceCollection services,
    string serviceName, string otlpEndpoint)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddEnvironmentVariableDetector())
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otlpEndpoint);
                        opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            });

        return services;
    }

}
