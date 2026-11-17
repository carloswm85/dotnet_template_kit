using System.Reflection;

namespace CleanArchitectureTemplate.Web.Tools;

/// <summary>
/// Version and build metadata for footers and banners.
/// </summary>
public static class VersionHelper
{
    private static Assembly AppAssembly => typeof(VersionHelper).Assembly;

    /// <summary>App version in Major.Minor format, e.g. "v1.2".</summary>
    public static string GetAppVersion()
    {
        var version = AppAssembly.GetName().Version;
        return version is null ? "v?" : $"v{version.Major}.{version.Minor}";
    }

    /// <summary>Full version number, e.g. "v1.2.0.0".</summary>
    public static string GetFullAppVersion() => $"v{AppAssembly.GetName().Version}";

    /// <summary>
    /// The &lt;InformationalVersion&gt; (or &lt;Version&gt;) MSBuild property,
    /// e.g. "v1.2.0-beta+abc123".
    /// </summary>
    public static string GetInformationalVersion()
    {
        var attribute =
            AppAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        return $"v{attribute?.InformationalVersion ?? "N/A"}";
    }

    /// <summary>Last write time of the app assembly, as a build-date proxy.</summary>
    public static string GetBuildDate()
    {
        var filePath = AppAssembly.Location;

        return File.Exists(filePath)
            ? File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm")
            : "Unknown";
    }

    /// <summary>
    /// Git commit from the GIT_COMMIT_HASH environment variable (set it in CI or
    /// the Dockerfile), or from App_Data/git_commit.txt
    /// (`git rev-parse HEAD > App_Data/git_commit.txt`).
    /// </summary>
    public static string GetGitCommitHash()
    {
        var hash = Environment.GetEnvironmentVariable("GIT_COMMIT_HASH");
        if (!string.IsNullOrEmpty(hash))
        {
            return hash;
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "App_Data", "git_commit.txt");
        return File.Exists(filePath) ? File.ReadAllText(filePath).Trim() : "Unknown";
    }

    /// <summary>All version data in one line, for test-environment banners.</summary>
    public static string GetBannerVersion() =>
        $"{GetInformationalVersion()} ({GetBuildDate()}, {GetGitCommitHash()})";
}