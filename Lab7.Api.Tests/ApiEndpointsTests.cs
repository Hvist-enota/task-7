namespace Lab7.Api.Tests;

using Xunit;

public sealed class ApiEndpointsTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointsTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Api")]
    public async Task GetStudents_ReturnsSuccessAndPayload()
    {
        var response = await _client.GetAsync("/api/students");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("firstName", payload, StringComparison.OrdinalIgnoreCase);
    }
}
