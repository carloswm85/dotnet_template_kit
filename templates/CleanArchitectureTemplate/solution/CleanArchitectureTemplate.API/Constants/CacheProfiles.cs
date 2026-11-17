using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureTemplate.API.Constants;

public class CacheProfiles
{
    public const string Default10Sec = "Default10Sec";

    public const string Default60Sec = "Default60Sec";

    public static readonly CacheProfile Profile10 = new()
    {
        Duration = 10,
        Location = ResponseCacheLocation.Client,
        NoStore = false,
    };

    public static readonly CacheProfile Profile60 = new() { Duration = 60 };
}
