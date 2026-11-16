using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Dtos;
using CleanArchitectureTemplate.ApplicationCore.Entities;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using CleanArchitectureTemplate.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitectureTemplate.Infrastructure.Services;

public sealed class AppSettingService : IAppSettingService
{
    private const string CacheKey = "AppSettings";
    private const string SingleCacheKeyPrefix = "AppSetting:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly ApplicationDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public AppSettingService(ApplicationDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public IReadOnlyList<AppSettingDto> GetAll()
    {
        return _cache.GetOrCreate(
                CacheKey,
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                    return _dbContext
                        .AppSettings.AsNoTracking()
                        .OrderBy(x => x.Category)
                        .ThenBy(x => x.Name)
                        .AsEnumerable()
                        .Select(ToDto)
                        .ToList();
                }
            ) ?? [];
    }

    public AppSettingDto GetSingle(string name)
    {
        return _cache.GetOrCreate(
                SingleCacheKeyPrefix + name,
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                    return FindDto(name);
                }
            ) ?? CreateErrorDto();
    }

    public AppSettingDto GetSingleUncached(string name)
    {
        return FindDto(name) ?? CreateErrorDto();
    }

    public void Remove()
    {
        _cache.Remove(CacheKey);
    }

    public void Remove(string name)
    {
        _cache.Remove(SingleCacheKeyPrefix + name);
        _cache.Remove(CacheKey);
    }

    private AppSettingDto? FindDto(string name)
    {
        var setting = _dbContext
            .AppSettings.AsNoTracking()
            .FirstOrDefault(x => x.Name == name);

        return setting is null ? null : ToDto(setting);
    }

    private static AppSettingDto ToDto(AppSetting setting)
    {
        return new AppSettingDto
        {
            Name = setting.Name,
            Type = setting.Type,
            Category = setting.Category,
            Description = setting.Description,
            Value = setting.Value,
            ModifiedByUserId = setting.ModifiedByUserId,
            ModificationDate = setting.ModificationDate,
        };
    }

    private static AppSettingDto CreateErrorDto()
    {
        return new AppSettingDto
        {
            Name = string.Empty,
            Type = string.Empty,
            Category = string.Empty,
            Description = string.Empty,
            Value = AppSettingConstants.ErrorValue,
        };
    }
}
