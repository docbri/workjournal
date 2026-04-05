namespace WorkJournalApi.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            message = "WorkJournal API is running",
            environment = app.ServiceProvider
                .GetRequiredService<IHostEnvironment>()
                .EnvironmentName,
            utc = DateTime.UtcNow
        }));

        return app;
    }
}
