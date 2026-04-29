using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace ZUMA.SharedKernel.Infrastructure.Extensions;

public static class SerilogExtensions
{
    public static void SetZumaLoggerConfigurationSerilog(this IHostApplicationBuilder host)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName();
        var serviceName = assemblyName?.Name;
        var version = assemblyName?.Version?.ToString();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceSource", serviceName)
            .Enrich.WithProperty("BuildVersion", version)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({ServiceSource}) {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(host.Configuration["SERILOG:SEQ:URL"]
                ?? throw new Exception("Key SERILOG:SEQ:URL not found in configuration!"))
            .CreateLogger();
    }
}