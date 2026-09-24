using App.Application.Abstractions.Jobs;
using App.Infrastructure.Persistence;
using App.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace App.Tests.Support;

public sealed class CarCareApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Hangfire:Enabled", "false");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hangfire:Enabled"] = "false",
                ["Jwt:Issuer"] = "CarCare",
                ["Jwt:Audience"] = "CarCare",
                ["Jwt:Key"] = "CarCare_Test_Key_Must_Be_32_Chars!!",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Seed:ManagerEmail"] = "manager@carcare.local",
                ["Seed:ManagerPassword"] = "Manager123!",
                ["Seed:StaffEmail"] = "staff@carcare.local",
                ["Seed:StaffPassword"] = "Staff123!"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CarCareDbContext>>();
            services.RemoveAll<CarCareDbContext>();
            services.RemoveAll<IBackgroundJobQueue>();

            _connection.Open();
            services.AddDbContext<CarCareDbContext>(options => options.UseSqlite(_connection));
            services.AddSingleton<IBackgroundJobQueue, FakeBackgroundJobQueue>();
            services.AddSingleton(_connection);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
