using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ZUMA.SharedKernel.Application.Utils;

public static class LoggerExtensions
{
    private static readonly AssemblyName? _assemblyName = Assembly.GetEntryAssembly()?.GetName();

    public static IDisposable? BeginMessageScope(
        this ILogger logger,
        string messageId,
        string? correlationId = null,
        object? identificationData = null)
    {
        var scopeProvider = new Dictionary<string, object>
        {
            ["MessageId"] = messageId,
            ["Version"] = _assemblyName?.Version?.ToString() ?? "0.0.0",
            ["ServiceSource"] = _assemblyName?.Name ?? "Unknown",
        };

        if (!string.IsNullOrEmpty(correlationId))
            scopeProvider["CorrelationId"] = correlationId;

        if (identificationData != null)
            scopeProvider["Identification"] = identificationData;

        return logger.BeginScope(scopeProvider);
    }
}