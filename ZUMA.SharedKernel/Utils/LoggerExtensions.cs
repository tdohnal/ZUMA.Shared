using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ZUMA.SharedKernel.Utils;

public static class LoggerExtensions
{
    private static readonly string? _assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

    public static IDisposable? BeginMessageScope(
        this ILogger logger,
        string messageId,
        string? correlationId = null,
        object? identificationData = null)
    {
        var scopeProvider = new Dictionary<string, object>
        {
            ["MessageId"] = messageId,
            ["ServiceSource"] = _assemblyName ?? "UnknownService",
            ["TimestampUtc"] = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(correlationId))
            scopeProvider["CorrelationId"] = correlationId;

        if (identificationData != null)
            scopeProvider["Identification"] = identificationData;

        return logger.BeginScope(scopeProvider);
    }
}