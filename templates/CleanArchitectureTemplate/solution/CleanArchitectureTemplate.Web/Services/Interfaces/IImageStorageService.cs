namespace CleanArchitectureTemplate.Web.Services.Interfaces;

/// <summary>
/// Stores uploaded images and returns the URL to save in the entity.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Saves <paramref name="file"/> under <paramref name="folder"/> and
    /// returns its URL, e.g. "/uploads/users/abc.png". Returns null when
    /// <paramref name="file"/> is null.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The file type is not allowed or the file is too large.
    /// </exception>
    Task<string?> SaveAsync(
        IFormFile? file,
        string folder,
        CancellationToken cancellationToken = default
    );
}