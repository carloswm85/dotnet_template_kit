namespace CleanArchitectureTemplate.ApplicationCore.Constants;

/// <summary>
/// Names, categories and types of the rows in the <c>AppSetting</c> table.
/// </summary>
public static class AppSettingConstants
{
    // Setting names
    public const string IsAppOnline = "IsAppOnline";

    // Categories
    public const string ApplicationCategory = "Application";

    // Types
    public const string BooleanType = "Boolean";

    /// <summary>
    /// Value returned when a setting is missing or cannot be read.
    /// </summary>
    public const string ErrorValue = "Error";
}
