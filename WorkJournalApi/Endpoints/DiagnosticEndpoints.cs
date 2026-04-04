using Microsoft.Extensions.Options;
using WorkJournalApi.Options;

namespace WorkJournalApi.Endpoints;

public static class DiagnosticEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Utc = DateTimeOffset.UtcNow
        }));

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

        return app;
    }
}
