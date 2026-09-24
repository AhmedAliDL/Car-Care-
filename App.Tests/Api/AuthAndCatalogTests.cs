using App.Application.Auth.Commands.Login;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Dtos;
using App.Application.ServicesCatalog.Dtos;
using App.Tests.Support;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace App.Tests.Api;

public class AuthAndCatalogTests : IClassFixture<CarCareApiFactory>
{
    private readonly HttpClient _client;

    public AuthAndCatalogTests(CarCareApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_creates_customer_only_and_duplicate_email_conflicts()
    {
        var first = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCustomerCommand
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "01234567890",
            Password = "SecurePassword123!"
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCustomerCommand
        {
            Name = "Jane Two",
            Email = "jane.doe@example.com",
            Phone = "01234567890",
            Password = "SecurePassword123!"
        });
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Weak_password_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCustomerCommand
        {
            Name = "Weak User",
            Email = "weak@example.com",
            Phone = "01234567890",
            Password = "password"
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_rejects_invalid_credentials_and_issues_jwt_for_valid_ones()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterCustomerCommand
        {
            Name = "Login User",
            Email = "login.user@example.com",
            Phone = "01234567890",
            Password = "SecurePassword123!"
        });

        var bad = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand
        {
            Email = "login.user@example.com",
            Password = "WrongPassword123!"
        });
        bad.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var good = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand
        {
            Email = "login.user@example.com",
            Password = "SecurePassword123!"
        });
        good.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await good.Content.ReadFromJsonAsync<AuthResponse>();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Me_requires_token_and_returns_profile()
    {
        var anonymous = await _client.GetAsync("/api/auth/me");
        anonymous.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var authed = _client;
        await authed.RegisterAndLoginAsync("profile.user@example.com", "Profile User");
        var me = await authed.GetFromJsonAsync<ProfileResponse>("/api/auth/me");
        me!.Email.Should().Be("profile.user@example.com");
        me.CustomerId.Should().NotBeNull();
        me.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Public_catalog_returns_only_active_services()
    {
        var services = await _client.GetFromJsonAsync<List<ServiceResponse>>("/api/services");
        services.Should().NotBeEmpty();
        services!.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task Vehicles_my_requires_authentication()
    {
        var response = await _client.GetAsync("/api/vehicles/my");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
