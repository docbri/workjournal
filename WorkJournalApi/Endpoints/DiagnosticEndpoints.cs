using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WorkJournalApi.Options;

namespace WorkJournalApi.Endpoints;

public static class DiagnosticEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapGet("/diagnostics/config", (IOptions<DiagnosticsOptions> options) =>
        {
            if (!options.Value.EnableConfigEndpoint)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                options.Value.EnvironmentName,
                options.Value.EnableConfigEndpoint
            });
        });

        app.MapGet("/diagnostics/throw", () =>
        {
            throw new InvalidOperationException("Test exception");
        });

        return app;
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration,
                Description = entry.Value.Description,
                Error = entry.Value.Exception?.Message
            }),
            Utc = DateTimeOffset.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
