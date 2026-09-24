using System.Net.Http.Json;
using App.Tests.Support;
using Xunit;
using Xunit.Abstractions;

namespace App.Tests.Api;

public class DiagnosticsTests : IClassFixture<CarCareApiFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DiagnosticsTests(CarCareApiFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Diagnose_401()
    {
        await _client.RegisterAndLoginAsync("diag@example.com");
        var response = await _client.GetAsync("/api/auth/me");
        _output.WriteLine($"Status: {(int)response.StatusCode}");
        foreach (var header in response.Headers)
            _output.WriteLine($"{header.Key}: {string.Join(",", header.Value)}");
        _output.WriteLine(await response.Content.ReadAsStringAsync());
    }
}
