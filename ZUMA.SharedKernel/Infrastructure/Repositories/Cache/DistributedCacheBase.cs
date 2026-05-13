using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ZUMA.SharedKernel.Domain.Interfaces;

namespace ZUMA.SharedKernel.Infrastructure.Repositories.Cache;

public abstract class DistributedCacheBase<T> where T : class, IAuditableEntities
{
    protected readonly IDistributedCache _cache;
    protected readonly ILogger _logger;
    protected readonly string _cachePrefix;

    protected DistributedCacheBase(IDistributedCache cache, ILogger logger)
    {
        _cache = cache;
        _logger = logger;
        _cachePrefix = $"{typeof(T).Name}:";
    }

    protected string GetCacheKey(long id) => $"{_cachePrefix}{id}";
    protected string GetCacheKey(Guid publicId) => $"{_cachePrefix}{publicId}";

    protected abstract bool IsCacheEnabled { get; }

    protected virtual async Task<T?> GetFromCacheAsync(string key, CancellationToken cancellationToken)
    {
        if (!IsCacheEnabled || string.IsNullOrWhiteSpace(key)) return null;

        string? cachedData = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedData)) return null;

        _logger.LogInformation("Cache HIT for {EntityType} with key {CacheKey}", typeof(T).Name, key);
        return JsonSerializer.Deserialize<T>(cachedData);
    }

    protected virtual async Task SetInCacheAsync(string key, T entity, CancellationToken cancellationToken)
    {
        if (!IsCacheEnabled || string.IsNullOrEmpty(key) || entity == null) return;

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(entity), options, cancellationToken);
    }

    protected virtual async Task InvalidateCacheAsync(long id, Guid publicId, CancellationToken cancellationToken)
    {
        if (!IsCacheEnabled) return;

        string? keyId = GetCacheKey(id);
        string? keyPublicId = GetCacheKey(publicId);

        if (!string.IsNullOrEmpty(keyId)) await _cache.RemoveAsync(keyId, cancellationToken);
        if (!string.IsNullOrEmpty(keyPublicId)) await _cache.RemoveAsync(keyPublicId, cancellationToken);
    }
}