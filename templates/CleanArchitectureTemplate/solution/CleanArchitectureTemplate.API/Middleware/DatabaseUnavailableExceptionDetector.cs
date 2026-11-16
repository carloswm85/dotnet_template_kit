using System.ComponentModel;
using Microsoft.Data.SqlClient;

namespace CleanArchitectureTemplate.API.Middleware;

/// <summary>
/// Detects SQL Server connectivity failures (timeouts, unreachable host,
/// broken connections) anywhere in an exception chain.
/// </summary>
public static class DatabaseUnavailableExceptionDetector
{
    private static readonly HashSet<int> SqlConnectivityErrorNumbers =
    [
        -2,
        2,
        20,
        53,
        64,
        121,
        233,
        258,
        10053,
        10054,
        10060,
        11001,
    ];

    private static readonly HashSet<int> Win32ConnectivityErrorCodes =
    [
        2,
        53,
        64,
        121,
        258,
        10053,
        10054,
        10060,
        11001,
    ];

    public static bool IsDatabaseUnavailable(Exception? exception)
    {
        for (
            Exception? current = exception;
            current is not null;
            current = current.InnerException
        )
        {
            if (
                current is SqlException sqlException
                && sqlException
                    .Errors.Cast<SqlError>()
                    .Any(error => SqlConnectivityErrorNumbers.Contains(error.Number))
            )
            {
                return true;
            }

            if (
                current is Win32Exception win32Exception
                && Win32ConnectivityErrorCodes.Contains(win32Exception.NativeErrorCode)
            )
            {
                return true;
            }
        }

        return false;
    }
}
