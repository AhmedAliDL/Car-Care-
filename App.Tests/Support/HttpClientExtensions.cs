using App.Application.Appointments.Commands.CreateAppointment;
using App.Application.Appointments.Dtos;
using App.Application.Auth.Commands.Login;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Dtos;
using App.Application.ServicesCatalog.Dtos;
using App.Application.Vehicles.Commands.CreateVehicle;
using App.Application.Vehicles.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Tests.Support;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<AuthResponse> RegisterAndLoginAsync(this HttpClient client, string email, string? name = null)
    {
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterCustomerCommand
        {
            Name = name ?? "Jane Doe",
            Email = email,
            Phone = "01234567890",
            Password = "SecurePassword123!"
        });
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand
        {
            Email = email,
            Password = "SecurePassword123!"
        });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
                   ?? throw new InvalidOperationException("Login response was empty.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return auth;
    }

    public static async Task<AuthResponse> LoginAsAsync(this HttpClient client, string email, string password)
    {
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand
        {
            Email = email,
            Password = password
        });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
                   ?? throw new InvalidOperationException("Login response was empty.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return auth;
    }

    public static async Task<VehicleResponse> CreateVehicleAsync(this HttpClient client, string vin = "1HGCR2F83HA000000")
    {
        var response = await client.PostAsJsonAsync("/api/vehicles", new CreateVehicleCommand
        {
            PlateNumber = "DEF-5678",
            Vin = vin,
            Make = "Honda",
            Model = "Accord",
            Year = 2021
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>(JsonOptions))!;
    }

    public static async Task<List<ServiceResponse>> GetServicesAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/services");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<ServiceResponse>>(JsonOptions))!;
    }

    public static async Task<AppointmentResponse> CreateAppointmentAsync(
        this HttpClient client,
        Guid vehicleId,
        IEnumerable<Guid> serviceIds,
        DateTime? when = null,
        string? notes = "Slight noise when braking.")
    {
        var response = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentCommand
        {
            VehicleId = vehicleId,
            ServiceIds = serviceIds.ToList(),
            AppointmentDate = when ?? DateTime.UtcNow.AddDays(7),
            Notes = notes
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AppointmentResponse>(JsonOptions))!;
    }
}
