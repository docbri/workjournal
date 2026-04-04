using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkJournalApi.Data;

namespace WorkJournalApi.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly Dictionary<string, string?> _configurationOverrides;
    private SqliteConnection? _connection;

    public CustomWebApplicationFactory(Dictionary<string, string?>? configurationOverrides = null)
    {
        _configurationOverrides = configurationOverrides ?? new Dictionary<string, string?>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");

        if (_configurationOverrides.Count > 0)
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(_configurationOverrides);
            });
        }

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(DbContextOptions<WorkJournalDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<WorkJournalDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
