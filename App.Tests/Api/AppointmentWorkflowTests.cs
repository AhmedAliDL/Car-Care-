using App.Application.Appointments.Commands.CreateAppointment;
using App.Application.Appointments.Dtos;
using App.Application.ServicesCatalog.Commands.CreateService;
using App.Application.ServicesCatalog.Commands.UpdateService;
using App.Application.ServicesCatalog.Dtos;
using App.Tests.Support;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace App.Tests.Api;


public class AppointmentWorkflowTests : IClassFixture<CarCareApiFactory>
{
    private readonly CarCareApiFactory _factory;

    public AppointmentWorkflowTests(CarCareApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Overlapping_active_appointments_are_rejected()
    {
        var customer = await CreateCustomerClientAsync();
        var vehicle = await customer.CreateVehicleAsync($"1HGCR2F83HA{Guid.NewGuid():N}"[..17]);
        var service = (await customer.GetServicesAsync()).First();
        var start = DateTime.UtcNow.Date.AddDays(7).AddHours(10);

        await customer.CreateAppointmentAsync(vehicle.Id, [service.Id], start);

        var conflict = await customer.PostAsJsonAsync("/api/appointments", new CreateAppointmentCommand
        {
            VehicleId = vehicle.Id,
            ServiceIds = [service.Id],
            AppointmentDate = start.AddMinutes(15)
        });

        conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Updated_catalog_price_does_not_change_existing_appointment_snapshot()
    {
        var customer = await CreateCustomerClientAsync();
        var vehicle = await customer.CreateVehicleAsync($"1HGCR2F83HA{Guid.NewGuid():N}"[..17]);
        var service = (await customer.GetServicesAsync()).First();
        var appointment = await customer.CreateAppointmentAsync(vehicle.Id, [service.Id]);

        var manager = _factory.CreateClient();
        await manager.LoginAsAsync("manager@carcare.local", "Manager123!");
        var update = await manager.PutAsJsonAsync($"/api/services/{service.Id}", new UpdateServiceCommand
        {
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            BasePrice = service.BasePrice + 25m
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await customer.GetFromJsonAsync<List<AppointmentResponse>>("/api/appointments/my");
        appointments!.Single(a => a.Id == appointment.Id).TotalPrice.Should().Be(service.BasePrice);
    }

    [Fact]
    public async Task Deactivated_service_cannot_be_booked()
    {
        var manager = _factory.CreateClient();
        await manager.LoginAsAsync("manager@carcare.local", "Manager123!");
        var serviceName = $"Test service {Guid.NewGuid():N}";
        var create = await manager.PostAsJsonAsync("/api/services", new CreateServiceCommand
        {
            Name = serviceName,
            DurationMinutes = 30,
            BasePrice = 10m
        });
        var service = await create.Content.ReadFromJsonAsync<ServiceResponse>();
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var deactivate = await manager.PatchAsync($"/api/services/{service!.Id}/deactivate", null);
        deactivate.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await CreateCustomerClientAsync();
        var vehicle = await customer.CreateVehicleAsync($"1HGCR2F83HA{Guid.NewGuid():N}"[..17]);
        var booking = await customer.PostAsJsonAsync("/api/appointments", new CreateAppointmentCommand
        {
            VehicleId = vehicle.Id,
            ServiceIds = [service.Id],
            AppointmentDate = DateTime.UtcNow.AddDays(7)
        });

        booking.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<HttpClient> CreateCustomerClientAsync()
    {
        var client = _factory.CreateClient();
        await client.RegisterAndLoginAsync($"customer.{Guid.NewGuid():N}@example.com");
        return client;
    }
}
