using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTemplate.Infrastructure.Tools;

/// <summary>
/// Recognizes SQL Server unique-index violations, so a duplicate can be
/// reported as a validation error instead of a 500. Pass the index name from
/// the model configuration, for example "UX_AspNetUsers_Email".
/// </summary>
public static class SqlServerExceptionHelper
{
    private const int CannotInsertDuplicateKeyRow = 2601;
    private const int UniqueConstraintViolation = 2627;

    public static bool IsUniqueConstraintViolation(
        DbUpdateException exception,
        string indexName
    ) =>
        exception.InnerException is SqlException sqlException
        && sqlException.Number is CannotInsertDuplicateKeyRow or UniqueConstraintViolation
        && sqlException.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
}
