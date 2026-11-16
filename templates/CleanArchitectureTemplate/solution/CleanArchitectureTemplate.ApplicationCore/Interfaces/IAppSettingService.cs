using CleanArchitectureTemplate.ApplicationCore.Dtos;

namespace CleanArchitectureTemplate.ApplicationCore.Interfaces;

/// <summary>
/// Reads runtime settings from the <c>AppSetting</c> table.
/// </summary>
public interface IAppSettingService
{
    /// <summary>Returns every setting, cached in memory for 10 minutes.</summary>
    IReadOnlyList<AppSettingDto> GetAll();

    /// <summary>Returns one setting, cached in memory for 10 minutes.</summary>
    AppSettingDto GetSingle(string name);

    /// <summary>
    /// Returns one setting straight from the database. Use it for values that
    /// must propagate immediately between hosts, such as availability flags.
    /// </summary>
    AppSettingDto GetSingleUncached(string name);

    /// <summary>Clears the cached list of settings.</summary>
    void Remove();

    /// <summary>Clears one cached setting and the cached list.</summary>
    void Remove(string name);
}
