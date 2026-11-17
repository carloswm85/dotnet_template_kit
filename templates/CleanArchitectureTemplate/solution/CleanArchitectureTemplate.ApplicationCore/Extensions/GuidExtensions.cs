namespace CleanArchitectureTemplate.ApplicationCore.Extensions;

/// <summary>
/// This class provides extension methods for strings that hold a Guid value.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// Creates a new Guid and returns it as a lowercase string, matching the casing
    /// SQL Server displays for uniqueidentifier values.
    /// </summary>
    /// <returns>A new Guid string in lowercase, e.g. a16d5d4a-43b8-48ee-9f4d-9a6bda858fe5.</returns>
    public static string NewLowercaseGuidString()
    {
        return Guid.NewGuid().ToString().ToLowerInvariant();
    }

    /// <summary>
    /// A Guid string is "new" (not yet persisted) when it is null, empty, whitespace,
    /// or the empty Guid (00000000-0000-0000-0000-000000000000).
    /// </summary>
    /// <param name="value">The string holding a Guid value.</param>
    /// <returns>True when the value represents a new record.</returns>
    public static bool IsNewGuidString(this string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || (Guid.TryParse(value, out Guid parsed) && parsed == Guid.Empty);
    }
}
