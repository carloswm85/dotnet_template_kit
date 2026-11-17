using CleanArchitectureTemplate.Web.Services.Interfaces;

namespace CleanArchitectureTemplate.Web.Services;

/// <summary>
/// Saves uploads to the folder in "Uploads:RootPath" (default:
/// wwwroot/uploads) and serves them at <see cref="RequestPath"/>.
///
/// - In Docker, point "Uploads:RootPath" at a mounted volume (see
///   docker-compose.yml); files inside the container are lost on rebuild.
/// - File names are random; the extension and size are validated here as well
///   as in the view models, so a crafted request cannot store scripts.
/// </summary>
public sealed class ImageStorageService : IImageStorageService
{
    public const string RequestPath = "/uploads";
    public const string DefaultImagePath = "/images/defaults/default-person.png";
    public const long MaxFileSizeBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(
        [".jpg", ".jpeg", ".png", ".webp"],
        StringComparer.OrdinalIgnoreCase
    );

    public ImageStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        RootPath = GetRootPath(environment, configuration);
    }

    /// <summary>Physical folder that holds the uploads.</summary>
    public string RootPath { get; }

    public static string GetRootPath(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configured = configuration["Uploads:RootPath"];

        return string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.WebRootPath, "uploads")
            : configured;
    }

    public async Task<string?> SaveAsync(
        IFormFile? file,
        string folder,
        CancellationToken cancellationToken = default
    )
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type '{extension}' is not allowed.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("The file is larger than 2 MB.");
        }

        // Only letters, digits and hyphens: the folder comes from code, but keep
        // it from ever escaping RootPath.
        if (folder.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
        {
            throw new ArgumentException("Invalid folder name.", nameof(folder));
        }

        var directory = Path.Combine(RootPath, folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        await using var stream = new FileStream(
            Path.Combine(directory, fileName),
            FileMode.CreateNew
        );
        await file.CopyToAsync(stream, cancellationToken);

        return $"{RequestPath}/{folder}/{fileName}";
    }
}