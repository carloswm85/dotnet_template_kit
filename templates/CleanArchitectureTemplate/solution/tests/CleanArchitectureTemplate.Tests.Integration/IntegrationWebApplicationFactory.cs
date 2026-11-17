using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using Xunit;
// Alias: the API project also defines a top-level Program class.
using WebProgram = CleanArchitectureTemplate.Web.Program;

namespace CleanArchitectureTemplate.Tests.Integration;

/// <summary>
/// Hosts the Web app in memory against a real SQL Server database.
///
/// The database is chosen in this order:
/// 1. The INTEGRATIONTESTS_CONNECTIONSTRING environment variable.
/// 2. LocalDB on Windows (no setup needed with Visual Studio).
/// 3. A throwaway SQL Server container (Testcontainers; requires Docker),
///    for Linux and macOS, including CI runners.
/// </summary>
public sealed class IntegrationWebApplicationFactory
    : WebApplicationFactory<WebProgram>,
        IAsyncLifetime
{
    private const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=CleanArchitectureTemplateIntegrationTests;"
        + "Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

    private MsSqlContainer? _container;
    private string _connectionString = LocalDbConnectionString;

    public async ValueTask InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable("INTEGRATIONTESTS_CONNECTIONSTRING");

        if (!string.IsNullOrWhiteSpace(configured))
        {
            _connectionString = configured;
        }
        else if (!OperatingSystem.IsWindows())
        {
            _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _connectionString,
                    }
                );
            }
        );
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        });
    }
}