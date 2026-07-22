using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartAttendance.Application.Common.Interfaces;

namespace SmartAttendance.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        var value = _memoryCache.TryGetValue(key, out T? result) ? result : default;
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);

        _memoryCache.Set(key, value, options);
        _logger.LogDebug("Cache set for key {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Cache removed for key {Key}", key);
        return Task.CompletedTask;
    }
}
