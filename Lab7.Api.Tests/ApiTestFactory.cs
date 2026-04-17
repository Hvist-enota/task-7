using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace Lab7.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly bool _useExternalDatabase;
    private readonly PostgreSqlContainer? _postgres;

    public string ConnectionString { get; }

    public ApiTestFactory()
    {
        _useExternalDatabase = string.Equals(
            Environment.GetEnvironmentVariable("USE_EXTERNAL_DB_FOR_API_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (_useExternalDatabase)
        {
            ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings__DefaultConnection must be set when USE_EXTERNAL_DB_FOR_API_TESTS=true.");
            return;
        }

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("Lab7Tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        ConnectionString = _postgres.GetConnectionString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Seed:Count"] = "100"
            });
        });
    }

    public async Task InitializeAsync()
    {
        if (_useExternalDatabase)
        {
            return;
        }

        if (_postgres is not null)
        {
            await _postgres.StartAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }

        Dispose();
    }
}
