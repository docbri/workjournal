using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkJournalApi.Infrastructure.Observability;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var applicationInsightsConnectionString =
            configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
        {
            services.AddOpenTelemetry()
                .UseAzureMonitor(options =>
                {
                    options.ConnectionString = applicationInsightsConnectionString;
                });
        }

        return services;
    }
}
