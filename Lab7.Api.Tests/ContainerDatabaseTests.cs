using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Lab7.Api.Tests;

public sealed class ContainerDatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("Lab7ContainerTests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "ContainerDb")]
    public async Task PostgresContainer_AcceptsConnections()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        var result = (int)(await command.ExecuteScalarAsync() ?? 0);

        Assert.Equal(1, result);
    }
}
