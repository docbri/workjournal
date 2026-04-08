using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkJournalApi.Data;

namespace WorkJournalApi.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _configurationOverrides;

    public CustomWebApplicationFactory(Dictionary<string, string?>? configurationOverrides = null)
    {
        _configurationOverrides = configurationOverrides ?? new Dictionary<string, string?>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__WorkJournal")
                ?? "Server=localhost,1433;Initial Catalog=WorkJournalIntegrationTests;User ID=sa;Password=WorkJ0urnal42;Encrypt=True;TrustServerCertificate=True;";

            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:WorkJournal"] = connectionString
            };

            foreach (var pair in _configurationOverrides)
            {
                settings[pair.Key] = pair.Value;
            }

            configBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkJournalDbContext>();

            db.Database.Migrate();
        });
    }
}
